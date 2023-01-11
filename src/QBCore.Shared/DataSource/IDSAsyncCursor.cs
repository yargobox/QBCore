using System.Data;

namespace QBCore.DataSource;

public interface IDSAsyncCursor<out T> : IAsyncDisposable, IDisposable
{
	T Current { get; }
	CancellationToken CancellationToken { get; }

	bool ObtainsLastPage { get; }
	bool IsLastPageAvailable { get; }
	bool IsLastPage { get; }
	event Action<bool> OnLastPage;

	bool ObtainsTotalCount { get; }
	bool IsTotalCountAvailable { get; }
	long TotalCount { get; }
	event Action<long> OnTotalCount;

	ValueTask<bool> MoveNextAsync(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken));
	bool MoveNext(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken));
}