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

	public async Task<TDocument> UpdateAsync(object id, TDocument document, IReadOnlySet<string>? modifiedFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
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

		var collection = _mongoDbContext.DB.GetCollection<TDocument>(top.DBSideName);

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

		foreach (var deInfo in Builder.DocumentInfo.DataEntries
			.Where(x => modifiedFieldNames == null ? x.Key != deId.Name : x.Key != deId.Name && modifiedFieldNames.Contains(x.Key))
			.Select(x => x.Value)
			.Cast<MongoDEInfo>()
			.Where(x => x.MemberMap != null))
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
				update = update != null ? update.Set(deInfo.Name, value) : Builders<TDocument>.Update.Set(deInfo.Name, value);
			}
		}
		if (update == null)
		{
			return document;
		}
		
		if (deUpdated != null && !isUpdatedSet)
		{
			update = update != null ? update.CurrentDate(deUpdated.Name) : Builders<TDocument>.Update.CurrentDate(deUpdated.Name);
		}
		if (deModified != null && !isModifiedSet)
		{
			update = update != null ? update.CurrentDate(deModified.Name) : Builders<TDocument>.Update.CurrentDate(deModified.Name);
		}
		updateOptions.IsUpsert = false;

		if (options != null)
		{
			if (options.QueryStringAsyncCallback != null)
			{
				var queryString = string.Concat(
					"db.", top.DBSideName, ".updateOne(", Environment.NewLine,
					  "\t", filter.ToString(), ",", Environment.NewLine,
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
				throw new KeyNotFoundException($"The update operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
			}
		}
		else
		{
			throw new ApplicationException($"The update operation failed: no acknowledgment to restore record '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
		}

		return document;
	}
}