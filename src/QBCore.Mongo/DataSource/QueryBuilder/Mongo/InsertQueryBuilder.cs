using MongoDB.Bson;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class InsertQueryBuilder<TDocument, TCreate> : QueryBuilder<TDocument, TCreate>, IInsertQueryBuilder<TDocument, TCreate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public InsertQueryBuilder(QBInsertBuilder<TDocument, TCreate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<TDocument> InsertAsync(TDocument document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Insert)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
		}

		if (options != null)
		{
			if (options.NativeOptions != null && options.NativeOptions is not InsertOneOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
		}

		var collection = _mongoDbContext.AsMongoDatabase().GetCollection<TDocument>(top.DBSideName);

		var insertOneOptions = (InsertOneOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocumentInfo.IdField;
		var deCreated = (MongoDEInfo?)Builder.DocumentInfo.DateCreatedField;
		var deModified = (MongoDEInfo?)Builder.DocumentInfo.DateModifiedField;

		var customIdGenerator = Builder.IdGenerator != null ? Builder.IdGenerator() : null;
		var generateId = customIdGenerator != null && deId?.Setter != null && customIdGenerator.IsEmpty(deId.Getter(document!));
		object id;
		DataSourceIdGeneratorOptions? generatorOptions = null;

		if (generateId && options != null)
		{
			generatorOptions = options.GeneratorOptions ?? new DataSourceIdGeneratorOptions();
			generatorOptions.NativeClientSession ??= options.NativeClientSession;
			generatorOptions.QueryStringCallback ??= options.QueryStringCallback;
			generatorOptions.QueryStringCallbackAsync ??= options.QueryStringCallbackAsync;
		}

		for (int i = 0; ; )
		{
			if (generateId)
			{
				id = await customIdGenerator!.GenerateIdAsync(collection, document!, generatorOptions, cancellationToken);
				deId!.Setter!(document!, id);
			}

			if (deCreated?.Flags.HasFlag(DataEntryFlags.ReadOnly) == false && deCreated.Setter != null)
			{
				if (deCreated.DataEntryType != typeof(BsonTimestamp))
				{
					var dateValue = deCreated.Getter(document!);
					var zero = deCreated.DataEntryType.GetDefaultValue();
					if (dateValue == zero)
					{
						dateValue = ArgumentHelper.GetNowValue(deCreated.UnderlyingType);
						deCreated.Setter(document!, dateValue);
					}
				}
			}

			if (deModified?.Flags.HasFlag(DataEntryFlags.ReadOnly) == false && deModified.Setter != null)
			{
				if (deModified.DataEntryType != typeof(BsonTimestamp))
				{
					var dateValue = deModified.Getter(document!);
					var zero = deModified.DataEntryType.GetDefaultValue();
					if (dateValue == zero)
					{
						dateValue = ArgumentHelper.GetNowValue(deModified.DataEntryType);
						deModified.Setter(document!, dateValue);
					}
				}
			}

			if (options != null)
			{
				if (options.QueryStringCallbackAsync != null)
				{
					var queryString = string.Concat("db.", top.DBSideName, ".insertOne(", document.ToBsonDocument().ToString(), ");");
					await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
				}
				else if (options.QueryStringCallback != null)
				{
					var queryString = string.Concat("db.", top.DBSideName, ".insertOne(", document.ToBsonDocument().ToString(), ");");
					options.QueryStringCallback(queryString);
				}
			}

			try
			{
				if (clientSessionHandle == null)
				{
					await collection.InsertOneAsync(document, insertOneOptions, cancellationToken);
				}
				else
				{
					await collection.InsertOneAsync(clientSessionHandle, document, insertOneOptions, cancellationToken);
				}

				return document;
			}
			catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
			{
				if (generateId && ++i < customIdGenerator!.MaxAttempts)
				{
					continue;
				}

				throw;
			}
		}
	}
}