using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class RestoreQueryBuilder<TDocument, TRestore> : QueryBuilder<TDocument, TRestore>, IRestoreQueryBuilder<TDocument, TRestore>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public RestoreQueryBuilder(QBRestoreBuilder<TDocument, TRestore> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task RestoreAsync(object id, TRestore? document = default(TRestore?), DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default)
	{
		if (id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document == null && typeof(TRestore) != typeof(EmptyDto))
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw new NotSupportedException($"Mongo restore query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		if (options != null)
		{
			if (options.NativeOptions != null && options.NativeOptions is not UpdateOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
		}

		var collection = _mongoDbContext.DB.GetCollection<TDocument>(top.DBSideName);

		var updateOptions = (UpdateOptions?)options?.NativeOptions ?? new UpdateOptions();
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field.");
		var deDeleted = (MongoDEInfo?)Builder.DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have a date deletion field.");

		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
		{
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does have a readonly date deletion field!");
		}

		var filter = Builders<TDocument>.Filter.Eq(deId.Name, id) & Builders<TDocument>.Filter.Ne(deDeleted.Name, (object?)null);
		var update = Builders<TDocument>.Update.Set(deDeleted.Name, BsonNull.Value);
		updateOptions.IsUpsert = false;

		if (options != null)
		{
			if (options.QueryStringAsyncCallback != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".updateOne(", Environment.NewLine,
					  "\t", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					  "\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					  "\t{\"upsert\": false}", Environment.NewLine,
					");"
				);
				await options.QueryStringAsyncCallback(queryString).ConfigureAwait(false);
			}
			else if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".updateOne(", Environment.NewLine,
					  "\t", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					  "\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					  "\t{\"upsert\": false}", Environment.NewLine,
					");"
				);
				options.QueryStringCallback(queryString);
			}
		}

		UpdateResult result;
		if (clientSessionHandle == null)
		{
			result = await collection.UpdateOneAsync(filter, update, updateOptions, cancellationToken);
		}
		else
		{
			result = await collection.UpdateOneAsync(clientSessionHandle, filter, update, updateOptions, cancellationToken);
		}

		if (result.IsModifiedCountAvailable)
		{
			if (result.ModifiedCount <= 0)
			{
				throw new KeyNotFoundException($"The restore operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}' or it has already been restored.");
			}
		}
		else
		{
			throw new ApplicationException($"The restore operation failed: no acknowledgment to restore record '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
		}
	}
}