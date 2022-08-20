using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class DeleteQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public DeleteQueryBuilder(QBDeleteBuilder<TDocument, TDelete> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default)
	{
		if (id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document == null && typeof(TDelete) != typeof(EmptyDto))
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Delete)
		{
			throw new NotSupportedException($"Mongo delete query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		if (options != null)
		{
			if (options.NativeOptions != null && options.NativeOptions is not DeleteOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
		}

		var collection = _mongoDbContext.DB.GetCollection<TDocument>(top.DBSideName);

		var deleteOptions = (DeleteOptions?)options?.NativeOptions ?? new DeleteOptions();
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field.");

		var filter = Builders<TDocument>.Filter.Eq(deId.Name, id);

		if (options != null)
		{
			if (options.QueryStringAsyncCallback != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".deleteOne(",
					filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ");"
				);
				await options.QueryStringAsyncCallback(queryString).ConfigureAwait(false);
			}
			else if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".deleteOne(",
					filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ");"
				);
				options.QueryStringCallback(queryString);
			}
		}

		DeleteResult result;
		if (clientSessionHandle == null)
		{
			result = await collection.DeleteOneAsync(filter, deleteOptions, cancellationToken);
		}
		else
		{
			result = await collection.DeleteOneAsync(clientSessionHandle, filter, deleteOptions, cancellationToken);
		}

		if (result.IsAcknowledged)
		{
			if (result.DeletedCount <= 0)
			{
				throw new KeyNotFoundException($"The delete operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
			}
		}
		else
		{
			throw new ApplicationException($"The delete operation failed: no acknowledgment to delete record '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
		}
	}
}