using MongoDB.Bson;
using MongoDB.Driver;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed partial class SelectQueryBuilder<TDocument, TSelect>
{
	private sealed class DSAsyncEnumerable : IDSAsyncEnumerable<TSelect>
	{
		private const uint _fSkipIsGreaterThanZero = 1;
		private const uint _fIsObtainEOF = 2;
		private const uint _fIsObtainTotalCount = 4;
		private const uint _fIsEOFAvailable = 16;
		private const uint _fIsEOF = 32;
		private const uint _fIsTotalCountAvailable = 64;
		private const uint _fIsGetAsyncEnumeratorCalled = 512;

		public bool IsObtainEOF => (_flags & _fIsObtainEOF) == _fIsObtainEOF;
		public bool IsEOFAvailable => (_flags & _fIsEOFAvailable) == _fIsEOFAvailable;
		public bool EOF
		{
			get
			{
				var flags = _flags;
				return (flags & _fIsEOFAvailable) == _fIsEOFAvailable
					? (flags & _fIsEOF) == _fIsEOF
					: throw new InvalidOperationException($"'{nameof(EOF)}' is not available!");
			}
		}

		public bool IsObtainTotalCount => (_flags & _fIsObtainTotalCount) == _fIsObtainTotalCount;
		public bool IsTotalCountAvailable => (_flags & _fIsTotalCountAvailable) == _fIsTotalCountAvailable;

		public long TotalCount
		{
			get => _totalCount;
			private set => _totalCount = value;
		}

		readonly IMongoCollection<TDocument> _collection;
		readonly List<BsonDocument> _query;
		readonly int _take;
		readonly AggregateOptions? _aggregateOptions;
		readonly IClientSessionHandle? _clientSessionHandle;
		readonly CancellationToken _cancellationToken;
		volatile uint _flags;
		long _totalCount;

		public DSAsyncEnumerable(IMongoCollection<TDocument> collection, List<BsonDocument> query, int take, bool skipIsGreaterThanZero, bool obtainEOF, bool obtainTotalCount, AggregateOptions? aggregateOptions, IClientSessionHandle? clientSessionHandle, CancellationToken cancellationToken)
		{
			_collection = collection;
			_query = query;
			_take = take;
			_aggregateOptions = aggregateOptions;
			_clientSessionHandle = clientSessionHandle;
			_cancellationToken = cancellationToken;
			
			uint flags = 0;
			if (skipIsGreaterThanZero) flags |= _fSkipIsGreaterThanZero;
			if (obtainEOF)
			{
				if (take < 0)
				{
					throw new ArgumentException(nameof(obtainEOF));
				}
				flags |= _fIsObtainEOF;
			}
			if (obtainTotalCount) flags |= _fIsObtainTotalCount;

			_flags = flags;
		}

		public async IAsyncEnumerator<TSelect> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
		{
			var flags = _flags;
			if ((flags & _fIsGetAsyncEnumeratorCalled) == _fIsGetAsyncEnumeratorCalled)
			{
				throw new InvalidOperationException($"Object of type '{nameof(IDSAsyncEnumerable<TSelect>)}' cannot be called twice!");
			}

			if (cancellationToken == default(CancellationToken))
			{
				cancellationToken = _cancellationToken;
			}

			_flags |= _fIsGetAsyncEnumeratorCalled;

			if ((flags & _fIsObtainEOF) == _fIsObtainEOF)
			{
				using (var cursor = _clientSessionHandle == null
					? await _collection.AggregateAsync<TSelect>(_query, _aggregateOptions, cancellationToken)
					: await _collection.AggregateAsync<TSelect>(_clientSessionHandle, _query, _aggregateOptions, cancellationToken))
				{
					var reverseCounter = _take;
					while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
					{
						foreach (var doc in cursor.Current)
						{
							if (--reverseCounter >= 0)
							{
								yield return doc;
							}
							else
							{
								_flags |= _fIsEOFAvailable;
								yield break;
							}
						}
						cancellationToken.ThrowIfCancellationRequested();
					}

					_flags |= _fIsEOF | _fIsEOFAvailable;
				}
			}
			else if ((flags & _fIsObtainTotalCount) == _fIsObtainTotalCount)
			{
				if ((flags & _fSkipIsGreaterThanZero) == _fSkipIsGreaterThanZero)
				{
					using (var cursor = _clientSessionHandle == null
						? await _collection.AggregateAsync<TSelect>(_query, _aggregateOptions, cancellationToken)
						: await _collection.AggregateAsync<TSelect>(_clientSessionHandle, _query, _aggregateOptions, cancellationToken))
					{
						while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
						{
							foreach (var doc in cursor.Current)
							{
								yield return doc;
							}
							cancellationToken.ThrowIfCancellationRequested();
						}
					}

					TotalCount = await GetTotalCountAsync(cancellationToken);
					_flags |= _fIsTotalCountAvailable;
				}
				else
				{
					uint counter = 0;

					using (var cursor = _clientSessionHandle == null
						? await _collection.AggregateAsync<TSelect>(_query, _aggregateOptions, cancellationToken)
						: await _collection.AggregateAsync<TSelect>(_clientSessionHandle, _query, _aggregateOptions, cancellationToken))
					{
						while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
						{
							foreach (var doc in cursor.Current)
							{
								counter++;
								yield return doc;
							}
							cancellationToken.ThrowIfCancellationRequested();
						}
					}

					if (_take < 0 || counter < (uint)_take)
					{
						_totalCount = counter;
						_flags |= _fIsEOFAvailable | _fIsEOF | _fIsTotalCountAvailable;
					}
					else
					{
						TotalCount = await GetTotalCountAsync(cancellationToken);
						_flags |= _fIsTotalCountAvailable;
					}
				}
			}
			else
			{
				using (var cursor = _clientSessionHandle == null
					? await _collection.AggregateAsync<TSelect>(_query, _aggregateOptions, cancellationToken)
					: await _collection.AggregateAsync<TSelect>(_clientSessionHandle, _query, _aggregateOptions, cancellationToken))
				{
					while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
					{
						foreach (var doc in cursor.Current)
						{
							yield return doc;
						}
						cancellationToken.ThrowIfCancellationRequested();
					}
				}
			}
		}

		private async ValueTask<long> GetTotalCountAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var index = _query.FindLastIndex(x => x.Contains("$limit"));
			if (index >= 0)
			{
				_query.RemoveAt(index);
			}
			index = _query.FindLastIndex(x => x.Contains("$skip"));
			if (index >= 0)
			{
				_query.RemoveAt(index);
			}
			index = _query.FindLastIndex(x => x.Contains("$sort"));
			if (index >= 0)
			{
				_query.RemoveAt(index);
			}

			_query.Add(new BsonDocument {
							{ "$group", new BsonDocument {
									{ "_id", BsonNull.Value },
									{ "n", new BsonDocument {
											{ "$sum", 1 }
										}
									}
							} } });

			using (var cursor = _clientSessionHandle == null
				? await _collection.AggregateAsync<TSelect>(_query, _aggregateOptions, cancellationToken)
				: await _collection.AggregateAsync<TSelect>(_clientSessionHandle, _query, _aggregateOptions, cancellationToken))
			{
				var bsonCount = (await cursor.FirstAsync(cancellationToken)).ToBsonDocument();
				return bsonCount["n"].ToInt64();
			}
		}
	}
}