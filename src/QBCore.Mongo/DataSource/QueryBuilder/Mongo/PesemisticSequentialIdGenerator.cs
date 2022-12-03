namespace QBCore.DataSource.QueryBuilder.Mongo;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using QBCore.DataSource.Options;

public class PesemisticSequentialIdGenerator<TDocument> : IDSIdGenerator
{
	public int MaxAttempts { get; }
	public readonly int StartAt;
	public readonly int Step;

	private static readonly Func<object, object>? _getDocumentId = BsonClassMap.LookupClassMap(typeof(TDocument)).IdMemberMap?.Getter;

	public PesemisticSequentialIdGenerator(int startAt = 1, int step = 1, int maxAttempts = 8)
	{
		if (step == 0) throw new ArgumentException(nameof(step) + " cannot be zero.", nameof(step));
		if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

		StartAt = startAt;
		Step = step;
		MaxAttempts = maxAttempts;
	}

	public bool IsEmpty(object? id) => id == null || (Step > 0 ? ((int)id) >= StartAt : ((int)id) <= StartAt);

	public object GenerateId(object container, object document, DataSourceIdGeneratorOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (container == null)
		{
			throw new ArgumentNullException(nameof(container));
		}
		if (container is not IMongoCollection<TDocument> collection)
		{
			throw new ArgumentException(nameof(container));
		}
		if (document == null)
		{
			throw new ArgumentNullException(nameof(document));
		}
		if (document is not TDocument typedDocument)
		{
			throw new ArgumentException(nameof(document));
		}
		if (options != null)
		{
			if (options.NativeOptions != null && options.NativeOptions is not AggregateOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
		}

		var aggregateOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		var query = Step > 0 ? SequentialIdGeneratorBase.MaxIDQuery : SequentialIdGeneratorBase.MinIDQuery;

		if (options?.QueryStringCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.CollectionName, ".aggregate(",
					Step > 0 ? SequentialIdGeneratorBase.MaxIDQueryString : SequentialIdGeneratorBase.MinIDQueryString,
				");");
			options.QueryStringCallback(queryString);
		}

		var lastId =
			(
				clientSessionHandle == null
					? collection.Aggregate<SequentialIdGeneratorBase.DocumentId>(query, aggregateOptions, cancellationToken)
					: collection.Aggregate<SequentialIdGeneratorBase.DocumentId>(clientSessionHandle, query, aggregateOptions, cancellationToken)
			)
			.FirstOrDefault(cancellationToken)?.Id;

		if (lastId == null)
		{
			return StartAt;
		}
		else
		{
			var newId = lastId.Value + Step;

			if (_getDocumentId != null && (int?)_getDocumentId(document!) == newId)
			{
				Thread.Sleep(40);
			}

			return newId;
		}
	}

	public async Task<object> GenerateIdAsync(object container, object document, DataSourceIdGeneratorOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (container == null)
		{
			throw new ArgumentNullException(nameof(container));
		}
		if (container is not IMongoCollection<TDocument> collection)
		{
			throw new ArgumentException(nameof(container));
		}
		if (document == null)
		{
			throw new ArgumentNullException(nameof(document));
		}
		if (document is not TDocument typedDocument)
		{
			throw new ArgumentException(nameof(document));
		}
		if (options != null)
		{
			if (options.NativeOptions != null && options.NativeOptions is not AggregateOptions)
			{
				throw new ArgumentException(nameof(options.NativeOptions));
			}
			if (options.NativeClientSession != null && options.NativeClientSession is not IClientSessionHandle)
			{
				throw new ArgumentException(nameof(options.NativeClientSession));
			}
		}

		var aggregateOptions = (AggregateOptions?)options?.NativeOptions;
		var clientSessionHandle = (IClientSessionHandle?)options?.NativeClientSession;
		var query = Step > 0 ? SequentialIdGeneratorBase.MaxIDQuery : SequentialIdGeneratorBase.MinIDQuery;

		if (options?.QueryStringCallbackAsync != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.CollectionName, ".aggregate(",
					Step > 0 ? SequentialIdGeneratorBase.MaxIDQueryString : SequentialIdGeneratorBase.MinIDQueryString,
				");");
			await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
		}
		else if (options?.QueryStringCallback != null)
		{
			var queryString = string.Concat(
				"db.", collection.CollectionNamespace.CollectionName, ".aggregate(",
					Step > 0 ? SequentialIdGeneratorBase.MaxIDQueryString : SequentialIdGeneratorBase.MinIDQueryString,
				");");
			options.QueryStringCallback(queryString);
		}

		var lastId = (await
			(
				clientSessionHandle == null
					? await collection.AggregateAsync<SequentialIdGeneratorBase.DocumentId>(query, aggregateOptions, cancellationToken).ConfigureAwait(false)
					: await collection.AggregateAsync<SequentialIdGeneratorBase.DocumentId>(clientSessionHandle, query, aggregateOptions, cancellationToken).ConfigureAwait(false)
			)
			.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false))?.Id;

		if (lastId == null)
		{
			return StartAt;
		}
		else
		{
			var newId = lastId.Value + Step;

			if (_getDocumentId != null && (int?)_getDocumentId(document!) == newId)
			{
				await Task.Delay(20).ConfigureAwait(false);
			}

			return newId;
		}
	}
}

internal static class SequentialIdGeneratorBase
{
	public sealed class DocumentId
	{
#pragma warning disable CS0649
		[BsonElement, BsonId] public int? Id;
#pragma warning restore CS0649
	}

	private static string? _maxIDQueryString;
	private static string? _minIDQueryString;

	public static readonly BsonDocument[] MaxIDQuery = new BsonDocument[]
	{
		new BsonDocument { { "$sort", new BsonDocument { { "_id", -1 } } } },
		new BsonDocument { { "$limit", 1 } },
		new BsonDocument { { "$project", new BsonDocument { { "_id", 1 } } } }
	};

	public static readonly BsonDocument[] MinIDQuery = new BsonDocument[]
	{
		new BsonDocument { { "$sort", new BsonDocument { { "_id", 1 } } } },
		MaxIDQuery[1],
		MaxIDQuery[2]
	};

	public static string MaxIDQueryString => _maxIDQueryString ?? (_maxIDQueryString = new BsonArray(MaxIDQuery).ToString());
	public static string MinIDQueryString => _minIDQueryString ?? (_minIDQueryString = new BsonArray(MinIDQuery).ToString());
}