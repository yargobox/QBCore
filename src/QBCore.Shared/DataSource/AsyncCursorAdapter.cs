namespace QBCore.DataSource;

public class AsyncCursorAdapter<T> : IAsyncDisposable, IDisposable
{
	public IEnumerable<T> Current => _current.Take(_count);

	private readonly T[] _current;
	private IAsyncEnumerable<T>? _asyncEnumerable;
	private int _count;
	private IAsyncEnumerator<T>? _asyncEnumerator;
	private IEnumerator<T>? _enumerator;

	public AsyncCursorAdapter(IAsyncEnumerable<T> asyncEnumerable, int bufferCapacity)
	{
		if (asyncEnumerable == null) throw new ArgumentNullException(nameof(asyncEnumerable));
		if (bufferCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacity));

		_asyncEnumerable = asyncEnumerable;
		_current = new T[bufferCapacity];
	}

	public AsyncCursorAdapter(IAsyncEnumerator<T> asyncEnumerator, int bufferCapacity)
	{
		if (asyncEnumerator == null) throw new ArgumentNullException(nameof(asyncEnumerator));
		if (bufferCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacity));

		_asyncEnumerator = asyncEnumerator;
		_current = new T[bufferCapacity];
	}

	public async ValueTask<bool> MoveNextAsync(CancellationToken cancellation = default(CancellationToken))
	{
		if (_enumerator != null) throw new InvalidOperationException();

		_asyncEnumerator ??= _asyncEnumerable?.GetAsyncEnumerator() ?? throw new InvalidOperationException();

		_count = 0;

		while (_count < _current.Length && await _asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
		{
			_current[_count] = _asyncEnumerator.Current;

			Interlocked.Increment(ref _count);
		}

		return _count > 0;
	}

	public bool MoveNext(CancellationToken cancellation = default(CancellationToken))
	{
		if (_asyncEnumerator != null) throw new InvalidOperationException();

		_enumerator ??= _asyncEnumerable!.ToBlockingEnumerable(cancellation).GetEnumerator();

		_count = 0;

		while (_count < _current.Length && _enumerator.MoveNext())
		{
			_current[_count++] = _enumerator.Current;
		}

		return _count > 0;
	}

	public async ValueTask DisposeAsync()
	{
		_asyncEnumerable = null;

		if (_asyncEnumerator != null)
		{
			var temp = _asyncEnumerator;
			_asyncEnumerator = null;
			temp.ConfigureAwait(false);
			await temp.DisposeAsync().ConfigureAwait(false);
		}
		else if (_enumerator != null)
		{
			var temp = _enumerator;
			_enumerator = null;
			temp.Dispose();
		}
	}

	public void Dispose()
	{
		_asyncEnumerable = null;

		if (_asyncEnumerator != null)
		{
			var temp = _asyncEnumerator;
			_asyncEnumerator = null;
			temp.ConfigureAwait(false);
			Task.Run(async () => await temp.DisposeAsync().ConfigureAwait(false));
		}
		else if (_enumerator != null)
		{
			var temp = _enumerator;
			_enumerator = null;
			temp.Dispose();
		}
	}
}