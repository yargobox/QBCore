using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class UpdateQueryBuilder<TDoc, TUpdate> : QueryBuilder<TDoc, TUpdate>, IUpdateQueryBuilder<TDoc, TUpdate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public UpdateQueryBuilder(UpdateQBBuilder<TDoc, TUpdate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<TDoc?> UpdateAsync(object id, TUpdate document, IReadOnlySet<string>? validFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

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
		var deUpdated = (MongoDEInfo?)Builder.DocInfo.DateUpdatedField;
		var deModified = (MongoDEInfo?)Builder.DocInfo.DateModifiedField;

		var filter = BuildConditionTree(false, Builder.Conditions, GetDBSideName, Builder.Parameters)?.BsonDocument ?? _noneFilter;

		UpdateDefinition<TDoc>? update = null;
		object? value;
		bool isSetValue, isUpdatedSet = false, isModifiedSet = false;
		var dataEntries = (Builder.DtoInfo?.DataEntries ?? Builder.DocInfo.DataEntries).Values.Cast<MongoDEInfo>();

		foreach (var deInfo in dataEntries.Where(x => x.MemberMap != null && x.Name != deId.Name && (validFieldNames == null || validFieldNames.Contains(x.Name))))
		{
			isSetValue = true;
			value = deInfo.Getter(document);

			if (deInfo == deUpdated)
			{
				isUpdatedSet = isSetValue = value is not null && value != deInfo.UnderlyingType.GetDefaultValue();
			}
			else if (deInfo == deModified)
			{
				isModifiedSet = isSetValue = value is not null && value != deInfo.UnderlyingType.GetDefaultValue();
			}

			if (isSetValue)
			{
				update = update?.Set(deInfo.Name, value) ?? Builders<TDoc>.Update.Set(deInfo.Name, value);
			}
		}

		if (update == null)
		{
			if (options?.FetchResultDocument == true)
			{
				await CallQueryStringCallback(options, collection, "Find", filter);

				TDoc? foundDocument;
				if (clientSessionHandle == null)
				{
					foundDocument = await (await collection.FindAsync(filter, new FindOptions<TDoc> { Limit = 1 }, cancellationToken).ConfigureAwait(false))
						.FirstOrDefaultAsync().ConfigureAwait(false);
				}
				else
				{
					foundDocument = await (await collection.FindAsync(clientSessionHandle, filter, new FindOptions<TDoc> { Limit = 1 }, cancellationToken).ConfigureAwait(false))
						.FirstOrDefaultAsync().ConfigureAwait(false);
				}

				return foundDocument
					?? throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
			}
			else
			{
				return default(TDoc?);
			}
		}
		
		if (deUpdated != null && !isUpdatedSet)
		{
			update = update?.CurrentDate(deUpdated.Name) ?? Builders<TDoc>.Update.CurrentDate(deUpdated.Name);
		}
		if (deModified != null && !isModifiedSet)
		{
			update = update?.CurrentDate(deModified.Name) ?? Builders<TDoc>.Update.CurrentDate(deModified.Name);
		}

		if (options?.FetchResultDocument == true)
		{
			await CallQueryStringCallback(options, collection, "FindOneAndUpdate", filter, update).ConfigureAwait(false);

			var findOneAndUpdateOptions = new FindOneAndUpdateOptions<TDoc>
			{
				IsUpsert = false,
				ReturnDocument = ReturnDocument.After
			};
			TDoc? foundDocument;
			if (clientSessionHandle == null)
			{
				foundDocument = await collection.FindOneAndUpdateAsync(filter, update, findOneAndUpdateOptions, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				foundDocument = await collection.FindOneAndUpdateAsync(clientSessionHandle, filter, update, findOneAndUpdateOptions, cancellationToken).ConfigureAwait(false);
			}

			return foundDocument
				?? throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
		}
		else
		{
			await CallQueryStringCallback(options, collection, "UpdateOne", filter, update).ConfigureAwait(false);

			updateOptions.IsUpsert = false;
			UpdateResult result;
			if (clientSessionHandle == null)
			{
				result = await collection.UpdateOneAsync(filter, update, updateOptions, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				result = await collection.UpdateOneAsync(clientSessionHandle, filter, update, updateOptions, cancellationToken).ConfigureAwait(false);
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

			return default(TDoc?);
		}
	}

	private async ValueTask CallQueryStringCallback(DataSourceOperationOptions? options, IMongoCollection<TDoc> collection, string funcName, FilterDefinition<TDoc> filter, UpdateDefinition<TDoc> update)
	{
		if (options?.QueryStringCallbackAsync != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
					"\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDoc>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					"\t{\"upsert\": false}", Environment.NewLine,
				");"
			);
			await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
		}
		else if (options?.QueryStringCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
					"\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDoc>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					"\t{\"upsert\": false}", Environment.NewLine,
				");"
			);
			options.QueryStringCallback(queryString);
		}
	}

	private async ValueTask CallQueryStringCallback(DataSourceOperationOptions? options, IMongoCollection<TDoc> collection, string funcName, FilterDefinition<TDoc> filter)
	{
		if (options?.QueryStringCallbackAsync != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
				");"
			);
			await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
		}
		else if (options?.QueryStringCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
				");"
			);
			options.QueryStringCallback(queryString);
		}
	}
}