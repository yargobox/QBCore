using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class InsertQueryBuilder<TDocument, TCreate> : QueryBuilder<TDocument, TCreate>, IInsertQueryBuilder<TDocument, TCreate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public IQBInsertBuilder<TDocument, TCreate> InsertBuilder => (IQBInsertBuilder<TDocument, TCreate>)Builder;

	private static readonly Func<object, object> _getDocumentId = BsonClassMap.LookupClassMap(typeof(TDocument)).IdMemberMap?.Getter
		?? throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TCreate).ToPretty()}': unknown documnt id field.");
	private static readonly Action<object, object> _setDocumentId = BsonClassMap.LookupClassMap(typeof(TDocument)).IdMemberMap?.Setter
		?? throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TCreate).ToPretty()}': unknown documnt id field.");

	public InsertQueryBuilder(QBInsertBuilder<TDocument, TCreate> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}

	public Task<TDocument> InsertAsync(
		TDocument document,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		var top = Builder.Containers.First();
		if (top.ContainerOperation == ContainerOperations.Insert)
		{
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

			var collection = _mongoDbContext.DB.GetCollection<TDocument>(top.DBSideName);

			var insertOneOptions = (InsertOneOptions?)options?.NativeOptions;
			var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;


			var customIdGenerator = InsertBuilder.CustomIdGenerator != null ? InsertBuilder.CustomIdGenerator() : null;
			var generateId = customIdGenerator != null && customIdGenerator.IsEmpty(entity.Id);
			var fillCreated = false;
			var fillModified = false;

			for (int i = 0; ; )
			{
				if (generateId)
				{
					entity.Id = await IdentityGenerator!.GenerateIdAsync(_col, entity);
				}

				if (_hasCreated)
				{
					var createdEntity = (ICreatedEntity<K, TDateTime>)entity;
					if (createdEntity.Created == null || fillCreated)
					{
						fillCreated = true;
						createdEntity.Created = (TDateTime)(object)DateTimeOffset.Now;
					}
				}
				else if (_hasModified)
				{
					var modifiedEntity = (IModifiedEntity<K, TDateTime>)entity;
					if (modifiedEntity.Modified == null || fillModified)
					{
						fillModified = true;
						modifiedEntity.Modified = (TDateTime)(object)DateTimeOffset.Now;
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

					return await Task.FromResult(entity);
				}
				catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
				{
					if (generateId && ++i < IdentityGenerator!.MaxAttempts)
					{
						continue;
					}

					throw;
				}
			}
		}
		else if (top.ContainerOperation == ContainerOperations.Exec)
		{

		}
	}
}