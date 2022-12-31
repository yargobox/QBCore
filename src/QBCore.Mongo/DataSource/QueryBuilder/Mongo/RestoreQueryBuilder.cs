using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

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
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null && typeof(TRestore) != typeof(EmptyDto)) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
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

		var collection = _mongoDbContext.AsMongoDatabase().GetCollection<TDocument>(top.DBSideName);

		var updateOptions = (UpdateOptions?)options?.NativeOptions ?? new UpdateOptions();
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocumentInfo.IdField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());
		var deDeleted = (MongoDEInfo?)Builder.DocumentInfo.DateDeletedField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveDeletedDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' has a readonly date deletion field!");

		var filter = BuildConditionTree(false, Builder.Conditions, GetDBSideName, Builder.Parameters)?.BsonDocument ?? _noneFilter;
		UpdateDefinition<TDocument>? update = null;

		if (document is not null)
		{
			var getDateDelFromDto = Builder.ProjectionInfo?.DateDeletedField?.Getter ?? Builder.ProjectionInfo?.DataEntries.GetValueOrDefault(deDeleted.Name)?.Getter;
			if (getDateDelFromDto != null)
			{
				var dateDel = getDateDelFromDto(document);
				if (dateDel is not null && dateDel != deDeleted.UnderlyingType.GetDefaultValue())
				{
					update = Builders<TDocument>.Update.Set(deDeleted.Name, dateDel);
				}
			}
		}
		update ??= deDeleted.IsNullable
			? Builders<TDocument>.Update.Set(deDeleted.Name, BsonNull.Value)
			: Builders<TDocument>.Update.Set(deDeleted.Name, deDeleted.UnderlyingType.GetDefaultValue());

		updateOptions.IsUpsert = false;

		if (options != null)
		{
			if (options.QueryStringCallbackAsync != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".updateOne(", Environment.NewLine,
					  "\t", filter.ToString(), ",", Environment.NewLine,
					  "\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					  "\t{\"upsert\": false}", Environment.NewLine,
					");"
				);
				await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
			}
			else if (options.QueryStringCallback != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".updateOne(", Environment.NewLine,
					  "\t", filter.ToString(), ",", Environment.NewLine,
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
				throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocumentInfo.DocumentType.ToPretty());
			}
		}
		else
		{
			throw EX.QueryBuilder.Make.OperationFailedNoAcknowledgment(QueryBuilderType.ToString(), id.ToString(), Builder.DocumentInfo.DocumentType.ToPretty());
		}
	}
}