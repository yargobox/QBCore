using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class UpdateQueryBuilder<TDocument, TUpdate> : QueryBuilder<TDocument, TUpdate>, IUpdateQueryBuilder<TDocument, TUpdate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public UpdateQueryBuilder(QBUpdateBuilder<TDocument, TUpdate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<TDocument?> UpdateAsync(object id, TUpdate document, IReadOnlySet<string>? validFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document == null)
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw new NotSupportedException($"Mongo update query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
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
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field.");
		var deUpdated = (MongoDEInfo?)Builder.DocumentInfo.DateUpdatedField;
		var deModified = (MongoDEInfo?)Builder.DocumentInfo.DateModifiedField;

		var filter = BuildConditionTree(false, Builder.Conditions, GetDBSideName, Builder.Parameters)?.BsonDocument ?? _noneFilter;

		UpdateDefinition<TDocument>? update = null;
		object? value;
		bool isSetValue, isUpdatedSet = false, isModifiedSet = false;
		var dataEntries = (Builder.ProjectionInfo?.DataEntries ?? Builder.DocumentInfo.DataEntries).Values.Cast<MongoDEInfo>();

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
				update = update?.Set(deInfo.Name, value) ?? Builders<TDocument>.Update.Set(deInfo.Name, value);
			}
		}

		if (update == null)
		{
			if (options?.FetchResultDocument == true)
			{
				await CallQueryStringCallback(options, collection, "Find", filter);

				TDocument? foundDocument;
				if (clientSessionHandle == null)
				{
					foundDocument = await (await collection.FindAsync(filter, new FindOptions<TDocument> { Limit = 1 }, cancellationToken).ConfigureAwait(false))
						.FirstOrDefaultAsync().ConfigureAwait(false);
				}
				else
				{
					foundDocument = await (await collection.FindAsync(clientSessionHandle, filter, new FindOptions<TDocument> { Limit = 1 }, cancellationToken).ConfigureAwait(false))
						.FirstOrDefaultAsync().ConfigureAwait(false);
				}

				return foundDocument
					?? throw new KeyNotFoundException($"The update operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
			}
			else
			{
				return default(TDocument?);
			}
		}
		
		if (deUpdated != null && !isUpdatedSet)
		{
			update = update?.CurrentDate(deUpdated.Name) ?? Builders<TDocument>.Update.CurrentDate(deUpdated.Name);
		}
		if (deModified != null && !isModifiedSet)
		{
			update = update?.CurrentDate(deModified.Name) ?? Builders<TDocument>.Update.CurrentDate(deModified.Name);
		}

		if (options?.FetchResultDocument == true)
		{
			await CallQueryStringCallback(options, collection, "FindOneAndUpdate", filter, update).ConfigureAwait(false);

			var findOneAndUpdateOptions = new FindOneAndUpdateOptions<TDocument>
			{
				IsUpsert = false,
				ReturnDocument = ReturnDocument.After
			};
			TDocument? foundDocument;
			if (clientSessionHandle == null)
			{
				foundDocument = await collection.FindOneAndUpdateAsync(filter, update, findOneAndUpdateOptions, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				foundDocument = await collection.FindOneAndUpdateAsync(clientSessionHandle, filter, update, findOneAndUpdateOptions, cancellationToken).ConfigureAwait(false);
			}

			return foundDocument
				?? throw new KeyNotFoundException($"The update operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
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
					throw new KeyNotFoundException($"The update operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
				}
			}
			else
			{
				throw new ApplicationException($"The update operation failed: no acknowledgment to restore record '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
			}

			return default(TDocument?);
		}
	}

	private async ValueTask CallQueryStringCallback(DataSourceOperationOptions? options, IMongoCollection<TDocument> collection, string funcName, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
	{
		if (options?.QueryStringAsyncCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
					"\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					"\t{\"upsert\": false}", Environment.NewLine,
				");"
			);
			await options.QueryStringAsyncCallback(queryString).ConfigureAwait(false);
		}
		else if (options?.QueryStringCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
					"\t", update.Render(BsonSerializer.SerializerRegistry.GetSerializer<TDocument>(), BsonSerializer.SerializerRegistry).ToString(), ",", Environment.NewLine,
					"\t{\"upsert\": false}", Environment.NewLine,
				");"
			);
			options.QueryStringCallback(queryString);
		}
	}

	private async ValueTask CallQueryStringCallback(DataSourceOperationOptions? options, IMongoCollection<TDocument> collection, string funcName, FilterDefinition<TDocument> filter)
	{
		if (options?.QueryStringAsyncCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.FullName, ".", funcName, "(", Environment.NewLine,
					"\t", filter.ToString(), ",", Environment.NewLine,
				");"
			);
			await options.QueryStringAsyncCallback(queryString).ConfigureAwait(false);
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