using System.Collections;

namespace QBCore.DataSource;

public static class ExtensionsForIDSAsyncCursor
{
	/// <summary>
	/// Determines whether the cursor contains any documents.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>True if the cursor contains any documents.</returns>
	public static bool Any<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		using (cursor)
		{
			var batch = GetFirstBatch(cursor, cancellationToken);
			return batch.Any();
		}
	}

	/// <summary>
	/// Determines whether the cursor contains any documents.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose result is true if the cursor contains any documents.</returns>
	public static async Task<bool> AnyAsync<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		using (cursor)
		{
			var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
			return batch.Any();
		}
	}

	/// <summary>
	/// Returns the first document of a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The first document.</returns>
	public static T First<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		using (cursor)
		{
			var batch = GetFirstBatch(cursor, cancellationToken);
			return batch.First();
		}
	}

	/// <summary>
	/// Returns the first document of a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose result is the first document.</returns>
	public static async Task<T> FirstAsync<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		using (cursor)
		{
			var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
			return batch.First();
		}
	}

	/// <summary>
	/// Returns the first document of a cursor, or a default value if the cursor contains no documents.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The first document of the cursor, or a default value if the cursor contains no documents.</returns>
	public static T? FirstOrDefault<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		using (cursor)
		{
			var batch = GetFirstBatch(cursor, cancellationToken);
			return batch.FirstOrDefault();
		}
	}

	/// <summary>
	/// Returns the first document of the cursor, or a default value if the cursor contains no documents.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task whose result is the first document of the cursor, or a default value if the cursor contains no documents.</returns>
	public static async Task<T?> FirstOrDefaultAsync<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		using (cursor)
		{
			var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
			return batch.FirstOrDefault();
		}
	}

	/// <summary>
	/// Calls a delegate for each document returned by the cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="source">The source.</param>
	/// <param name="processor">The processor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task that completes when all the documents have been processed.</returns>
	public static Task ForEachAsync<T>(this IDSAsyncCursor<T> source, Func<T, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ForEachAsync(source, (doc, _) => processor(doc), cancellationToken);
	}

	/// <summary>
	/// Calls a delegate for each document returned by the cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The source.</param>
	/// <param name="processor">The processor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task that completes when all the documents have been processed.</returns>
	public static async Task ForEachAsync<T>(this IDSAsyncCursor<T> cursor, Func<T, int, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));
		if (processor == null) throw new ArgumentNullException(nameof(processor));

		using (cursor)
		{
			var index = 0;
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				foreach (var document in cursor.Current)
				{
					await processor(document, index++).ConfigureAwait(false);
					cancellationToken.ThrowIfCancellationRequested();
				}
			}
		}
	}

	/// <summary>
	/// Calls a delegate for each document returned by the cursor.
	/// </summary>
	/// <remarks>
	/// If your delegate is going to take a long time to execute or is going to block
	/// consider using a different overload of ForEachAsync that uses a delegate that
	/// returns a Task instead.
	/// </remarks>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="source">The source.</param>
	/// <param name="processor">The processor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task that completes when all the documents have been processed.</returns>
	public static Task ForEachAsync<T>(this IDSAsyncCursor<T> source, Action<T> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ForEachAsync(source, (doc, _) => processor(doc), cancellationToken);
	}

	/// <summary>
	/// Calls a delegate for each document returned by the cursor.
	/// </summary>
	/// <remarks>
	/// If your delegate is going to take a long time to execute or is going to block
	/// consider using a different overload of ForEachAsync that uses a delegate that
	/// returns a Task instead.
	/// </remarks>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The source.</param>
	/// <param name="processor">The processor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task that completes when all the documents have been processed.</returns>
	public static async Task ForEachAsync<T>(this IDSAsyncCursor<T> cursor, Action<T, int> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));
		if (processor == null) throw new ArgumentNullException(nameof(processor));

		using (cursor)
		{
			var index = 0;
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				foreach (var document in cursor.Current)
				{
					processor(document, index++);
					cancellationToken.ThrowIfCancellationRequested();
				}
			}
		}
	}

	/// <summary>
	/// Returns the only document of a cursor. This method throws an exception if the cursor does not contain exactly one document.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The only document of a cursor.</returns>
	public static T Single<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));		

		using (cursor)
		{
			var batch = GetFirstBatch(cursor, cancellationToken);
			return batch.Single();
		}
	}

	/// <summary>
	/// Returns the only document of a cursor. This method throws an exception if the cursor does not contain exactly one document.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose result is the only document of a cursor.</returns>
	public static async Task<T> SingleAsync<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));	

		using (cursor)
		{
			var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
			return batch.Single();
		}
	}

	/// <summary>
	/// Returns the only document of a cursor, or a default value if the cursor contains no documents.
	/// This method throws an exception if the cursor contains more than one document.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The only document of a cursor, or a default value if the cursor contains no documents.</returns>
	public static T? SingleOrDefault<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));		

		using (cursor)
		{
			var batch = GetFirstBatch(cursor, cancellationToken);
			return batch.SingleOrDefault();
		}
	}

	/// <summary>
	/// Returns the only document of a cursor, or a default value if the cursor contains no documents.
	/// This method throws an exception if the cursor contains more than one document.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose result is the only document of a cursor, or a default value if the cursor contains no documents.</returns>
	public static async Task<T?> SingleOrDefaultAsync<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));	

		using (cursor)
		{
			var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
			return batch.SingleOrDefault();
		}
	}

	/// <summary>
	/// Wraps a cursor in an IEnumerable that can be enumerated one time.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The cursor.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>An IEnumerable</returns>
	public static IEnumerable<T> ToEnumerable<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		return new AsyncCursorEnumerableOneTimeAdapter<T>(cursor, cancellationToken);
	}

	/// <summary>
	/// Returns a list containing all the documents returned by a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The source.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The list of documents.</returns>
	public static List<T> ToList<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));	

		var list = new List<T>();
		using (cursor)
		{
			while (cursor.MoveNext(cancellationToken))
			{
				list.AddRange(cursor.Current);
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
		return list;
	}

	/// <summary>
	/// Returns a list containing all the documents returned by a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The source.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose value is the list of documents.</returns>
	public static async Task<List<T>> ToListAsync<T>(this IDSAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));

		var list = new List<T>();
		using (cursor)
		{
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				list.AddRange(cursor.Current);
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		return list;
	}

	/// <summary>
	/// Returns a list containing all the documents returned by a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The source.</param>
	/// <param name="lastPageMarkerCallback">The callback to detect the last page.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose value is the list of documents.</returns>
	public static async Task<List<T>> ToListAsync<T>(this IDSAsyncCursor<T> cursor, Action<bool> lastPageMarkerCallback, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));
		if (lastPageMarkerCallback == null) throw new ArgumentNullException(nameof(lastPageMarkerCallback));

		var list = new List<T>();
		using (cursor)
		{
			cursor.LastPageMarkerCallback = lastPageMarkerCallback;
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				list.AddRange(cursor.Current);
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		return list;
	}

	/// <summary>
	/// Returns a list containing all the documents returned by a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the document.</typeparam>
	/// <param name="cursor">The source.</param>
	/// <param name="totalCountCallback">The callback to get a total row count.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A Task whose value is the list of documents.</returns>
	public static async Task<List<T>> ToListAsync<T>(this IDSAsyncCursor<T> cursor, Action<long> totalCountCallback, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cursor == null) throw new ArgumentNullException(nameof(cursor));
		if (totalCountCallback == null) throw new ArgumentNullException(nameof(totalCountCallback));

		var list = new List<T>();
		using (cursor)
		{
			cursor.TotalCountCallback = totalCountCallback;
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				list.AddRange(cursor.Current);
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		return list;
	}

	private static IEnumerable<T> GetFirstBatch<T>(IDSAsyncCursor<T> cursor, CancellationToken cancellationToken)
	{
		if (cursor.MoveNext(cancellationToken))
		{
			return cursor.Current;
		}
		else
		{
			return Enumerable.Empty<T>();
		}
	}

	private static async Task<IEnumerable<T>> GetFirstBatchAsync<T>(IDSAsyncCursor<T> cursor, CancellationToken cancellationToken)
	{
		if (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
		{
			return cursor.Current;
		}
		else
		{
			return Enumerable.Empty<T>();
		}
	}

	private class AsyncCursorEnumerableOneTimeAdapter<T> : IEnumerable<T>
	{
		private readonly IDSAsyncCursor<T> _cursor;
		private readonly CancellationToken _cancellationToken;
		private bool _hasBeenEnumerated;

		public AsyncCursorEnumerableOneTimeAdapter(IDSAsyncCursor<T> cursor, CancellationToken cancellationToken)
		{
			if (cursor == null) throw new ArgumentNullException(nameof(cursor));

			_cursor = cursor;
			_cancellationToken = cancellationToken;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (_hasBeenEnumerated) throw new InvalidOperationException("An IDSAsyncCursor can only be enumerated once.");

			_hasBeenEnumerated = true;
			return new AsyncCursorEnumerator<T>(_cursor, _cancellationToken);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class AsyncCursorEnumerator<T> : IEnumerator<T>
	{
		private readonly IDSAsyncCursor<T> _cursor;
		private readonly CancellationToken _cancellationToken;
		private IEnumerator<T>? _batchEnumerator;
		private bool _disposed;
		private bool _finished;
		private bool _started;

		public AsyncCursorEnumerator(IDSAsyncCursor<T> cursor, CancellationToken cancellationToken)
		{
			if (cursor == null) throw new ArgumentNullException(nameof(cursor));

			_cursor = cursor;
			_cancellationToken = cancellationToken;
		}

		public T Current
		{
			get
			{
				ThrowIfDisposed();
				if (!_started)
				{
					throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
				}
				if (_finished)
				{
					throw new InvalidOperationException("Enumeration already finished.");
				}
				return _batchEnumerator!.Current;
			}
		}

		object IEnumerator.Current => Current!;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_batchEnumerator?.Dispose();
				_cursor.Dispose();
			}
		}

		public bool MoveNext()
		{
			ThrowIfDisposed();
			_started = true;

			if (_batchEnumerator != null && _batchEnumerator.MoveNext())
			{
				return true;
			}

			while (true)
			{
				if (_cursor.MoveNext(_cancellationToken))
				{
					_batchEnumerator?.Dispose();
					_batchEnumerator = _cursor.Current.GetEnumerator();
					if (_batchEnumerator.MoveNext())
					{
						return true;
					}
				}
				else
				{
					_batchEnumerator = null;
					_finished = true;
					return false;
				}
			}
		}

		public void Reset()
		{
			ThrowIfDisposed();
			throw new NotSupportedException();
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}