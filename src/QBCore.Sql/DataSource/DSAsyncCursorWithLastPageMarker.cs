using QBCore.Extensions.Linq;

namespace QBCore.DataSource;

internal sealed class DSAsyncCursorWithLastPageMarker<T> : IDSAsyncCursor<T>, IAsyncDisposable
{
	private readonly AsyncCursorAdapter<T> _cursor;
	private readonly CancellationToken _cancellationToken;
	private volatile int _lastPageMarker;
	private volatile int _reverseCounter;
	private Action<bool>? _LastPageMarkerCallback;

	public CancellationToken CancellationToken => _cancellationToken;
	public IEnumerable<T> Current => _cursor.Current;

	public bool ObtainsLastPageMarker => true;
	public bool IsLastPageMarkerAvailable => _lastPageMarker >= 0;
	public bool LastPageMarker => _lastPageMarker >= 0 ? _lastPageMarker > 0 : throw new InvalidOperationException("LastPageMarker is not available!");
	public Action<bool>? LastPageMarkerCallback
	{
		get => _LastPageMarkerCallback;
		set => _LastPageMarkerCallback = value;
	}

	public bool ObtainsTotalCount => false;
	public bool IsTotalCountAvailable => false;
	public long TotalCount => throw new InvalidOperationException("TotalCount is not obtained by this cursor!");
	public Action<long>? TotalCountCallback
	{
		get => throw new InvalidOperationException("TotalCount is not obtained by this cursor!");
		set => throw new InvalidOperationException("TotalCount is not obtained by this cursor!");
	}

	public DSAsyncCursorWithLastPageMarker(IAsyncEnumerable<T> cursor, int take, CancellationToken cancellationToken)
	{
		_cursor = new AsyncCursorAdapter<T>(cursor, 20);
		_cancellationToken = cancellationToken;
		_lastPageMarker = -1;
		_reverseCounter = take >= 0 ? take : int.MaxValue;
	}

	public bool MoveNext(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken == default(CancellationToken)) cancellationToken = _cancellationToken;

		if (_cursor.MoveNext(cancellationToken))
		{
			var reverseCounter = _reverseCounter -= _cursor.Current.CountTryGetNonEnumerated();
			if (reverseCounter < 0 && _lastPageMarker < 0)
			{
				_lastPageMarker = 0;
				if (_LastPageMarkerCallback != null) _LastPageMarkerCallback(false);
			}

			return true;
		}
		else if (_lastPageMarker < 0)
		{
			_lastPageMarker = 1;
			if (_LastPageMarkerCallback != null) _LastPageMarkerCallback(true);
		}

		return false;
	}

	public async ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken == default(CancellationToken)) cancellationToken = _cancellationToken;

		if (await _cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
		{
			var reverseCounter = _reverseCounter -= _cursor.Current.CountTryGetNonEnumerated();
			if (reverseCounter < 0 && _lastPageMarker < 0)
			{
				_lastPageMarker = 0;
				if (_LastPageMarkerCallback != null) _LastPageMarkerCallback(false);
			}

			return true;
		}
		else if (_lastPageMarker < 0)
		{
			_lastPageMarker = 1;
			if (_LastPageMarkerCallback != null) _LastPageMarkerCallback(true);
		}

		return false;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		_cursor.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		await _cursor.DisposeAsync().ConfigureAwait(false);
	}
}