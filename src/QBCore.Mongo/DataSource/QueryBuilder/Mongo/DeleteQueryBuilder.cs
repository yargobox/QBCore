using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

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
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null && typeof(TDelete) != typeof(EmptyDto)) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Delete)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
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

		var collection = _mongoDbContext.AsMongoDatabase().GetCollection<TDocument>(top.DBSideName);

		var deleteOptions = (DeleteOptions?)options?.NativeOptions ?? new DeleteOptions();
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocumentInfo.IdField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());

		var filter = Builders<TDocument>.Filter.Eq(deId.Name, id);

		if (options != null)
		{
			if (options.QueryStringCallbackAsync != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".deleteOne(",
					filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ");"
				);
				await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
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
				throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocumentInfo.DocumentType.ToPretty());
			}
		}
		else
		{
			throw EX.QueryBuilder.Make.OperationFailedNoAcknowledgment(QueryBuilderType.ToString(), id.ToString(), Builder.DocumentInfo.DocumentType.ToPretty());
		}
	}
}