using MongoDB.Driver;

namespace QBCore.DataSource;

internal sealed class DSAsyncCursor<T> : IDSAsyncCursor<T>
{
	private readonly IAsyncCursor<T> _cursor;
	private readonly CancellationToken _cancellationToken;

	public CancellationToken CancellationToken => _cancellationToken;
	public IEnumerable<T> Current => _cursor.Current;

	public bool ObtainsLastPageMarker => false;
	public bool IsLastPageMarkerAvailable => false;
	public bool LastPageMarker => throw new InvalidOperationException("LastPageMarker is not acknowledged by this cursor!");
	public Action<bool>? LastPageMarkerCallback
	{
		get => throw new InvalidOperationException("LastPageMarker is not acknowledged by this cursor!");
		set => throw new InvalidOperationException("LastPageMarker is not acknowledged by this cursor!");
	}

	public bool ObtainsTotalCount => false;
	public bool IsTotalCountAvailable => false;
	public long TotalCount => throw new InvalidOperationException("TotalCount is not obtained by this cursor!");
	public Action<long>? TotalCountCallback
	{
		get => throw new InvalidOperationException("TotalCount is not obtained by this cursor!");
		set => throw new InvalidOperationException("TotalCount is not obtained by this cursor!");
	}

	public DSAsyncCursor(IAsyncCursor<T> cursor, CancellationToken cancellationToken)
	{
		_cursor = cursor;
		_cancellationToken = cancellationToken;
	}

	public bool MoveNext(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken == default(CancellationToken)) cancellationToken = _cancellationToken;

		return _cursor.MoveNext(cancellationToken);
	}

	public async ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken == default(CancellationToken)) cancellationToken = _cancellationToken;

		return await _cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		_cursor.Dispose();
	}
}