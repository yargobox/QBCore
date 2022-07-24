namespace QBCore.DataSource;

public interface IDSAsyncCursor<out T> : IDisposable
{
	IEnumerable<T> Current { get; }
	bool MoveNext(CancellationToken cancellationToken = default(CancellationToken));
	ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken));

	CancellationToken CancellationToken { get; }

	bool ObtainsLastPageMarker { get; }
	bool IsLastPageMarkerAvailable { get; }
	bool LastPageMarker { get; }
	Action<bool>? LastPageMarkerCallback { get; set; }

	bool ObtainsTotalCount { get; }
	bool IsTotalCountAvailable { get; }
	long TotalCount { get; }
	Action<long>? TotalCountCallback { get; set; }
}