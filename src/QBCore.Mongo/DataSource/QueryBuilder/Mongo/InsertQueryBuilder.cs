using MongoDB.Bson;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class InsertQueryBuilder<TDoc, TCreate> : QueryBuilder<TDoc, TCreate>, IInsertQueryBuilder<TDoc, TCreate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public InsertQueryBuilder(InsertQBBuilder<TDoc, TCreate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<object> InsertAsync(TCreate dto, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (dto is null) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(dto));

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

		TDoc document;
		if (typeof(TDoc) == typeof(TCreate))
		{
			document = ConvertTo<TDoc>.From(dto);
		}
		else if (options?.DocumentMapper != null)
		{
			if (options.DocumentMapper is not Func<TCreate, TDoc> mapper)
			{
				throw new ArgumentException(nameof(DataSourceInsertOptions.DocumentMapper));
			}

			document = mapper(dto);
		}
		else if (Builder.IsDocumentMapperRequired)
		{
			throw EX.QueryBuilder.Make.QueryBuilderRequiresMapper(Builder.DataLayer.Name, QueryBuilderType.ToString(), typeof(TDoc).ToPretty());
		}
		else
		{
			document = ConvertTo<TDoc>.MapFrom(dto);
		}

		var collection = _mongoDbContext.AsMongoDatabase().GetCollection<TDoc>(top.DBSideName);

		var insertOneOptions = (InsertOneOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocInfo.IdField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocInfo.DocumentType.ToPretty());
		var deCreated = (MongoDEInfo?)Builder.DocInfo.DateCreatedField;
		var deModified = (MongoDEInfo?)Builder.DocInfo.DateModifiedField;

		object? id = null;
		var generator = Builder.IdGenerator != null ? Builder.IdGenerator() : null;
		var useGenerator = generator != null && deId.Setter != null;
		if (useGenerator)
		{
			id = deId.Getter(document!)!;
			useGenerator = generator!.IsEmpty(id);
		}

		DataSourceIdGeneratorOptions? generatorOptions = null;

		if (useGenerator && options != null)
		{
			generatorOptions = options.GeneratorOptions ?? new DataSourceIdGeneratorOptions();
			generatorOptions.NativeClientSession ??= options.NativeClientSession;
			generatorOptions.QueryStringCallback ??= options.QueryStringCallback;
			generatorOptions.QueryStringCallbackAsync ??= options.QueryStringCallbackAsync;
		}

		for (int attempt = 0; ; )
		{
			if (useGenerator)
			{
				id = await generator!.GenerateIdAsync(collection, document!, generatorOptions, cancellationToken);
				deId.Setter!(document!, id);
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

				return useGenerator ? id! : deId.Getter(document!)!;
			}
			catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
			{
				if (useGenerator && ++attempt < generator!.MaxAttempts)
				{
					continue;
				}

				throw;
			}
		}
	}
}