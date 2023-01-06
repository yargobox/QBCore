using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SoftDelQueryBuilder<TDoc, TDelete> : QueryBuilder<TDoc, TDelete>, IDeleteQueryBuilder<TDoc, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public SoftDelQueryBuilder(SoftDelQBBuilder<TDoc, TDelete> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null && typeof(TDelete) != typeof(EmptyDto)) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

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

		var collection = _mongoDbContext.AsMongoDatabase().GetCollection<TDoc>(top.DBSideName);

		var updateOptions = (UpdateOptions?)options?.NativeOptions ?? new UpdateOptions();
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;

		var deId = (MongoDEInfo?)Builder.DocInfo.IdField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocInfo.DocumentType.ToPretty());
		var deDeleted = (MongoDEInfo?)Builder.DocInfo.DateDeletedField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveDeletedDataEntry(Builder.DocInfo.DocumentType.ToPretty());
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{Builder.DocInfo.DocumentType.ToPretty()}' has a readonly date deletion field!");

		var filter = BuildConditionTree(false, Builder.Conditions, GetDBSideName, Builder.Parameters)?.BsonDocument ?? _noneFilter;
		UpdateDefinition<TDoc>? update = null;

		if (document is not null)
		{
			var getDateDelFromDto = Builder.DtoInfo?.DateDeletedField?.Getter ?? Builder.DtoInfo?.DataEntries.GetValueOrDefault(deDeleted.Name)?.Getter;
			if (getDateDelFromDto != null)
			{
				var dateDel = getDateDelFromDto(document);
				if (dateDel is not null && dateDel != deDeleted.UnderlyingType.GetDefaultValue())
				{
					update = Builders<TDoc>.Update.Set(deDeleted.Name, dateDel);
				}
			}
		}
		update ??= Builders<TDoc>.Update.CurrentDate(deDeleted.Name);
		
		updateOptions.IsUpsert = false;

		if (options != null)
		{
			if (options.QueryStringCallbackAsync != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".updateOne(", Environment.NewLine,
					  "\t", filter.ToString(), ",", Environment.NewLine,
					  "\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDoc>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
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
					  "\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDoc>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
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
				throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
			}
		}
		else
		{
			throw EX.QueryBuilder.Make.OperationFailedNoAcknowledgment(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
		}
	}
}