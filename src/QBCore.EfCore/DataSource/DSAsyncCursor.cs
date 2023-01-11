using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using QBCore.Extensions.Internals;
using QBCore.Extensions.Threading.Tasks;

namespace QBCore.DataSource;

internal class DSAsyncCursor<T> : IDSAsyncCursor<T>
{
	private IQueryable<T>? _queryable;
	private IAsyncEnumerator<T>? _asyncEnumerator;
	private IEnumerator<T>? _syncEnumerator;
	private T _current;
	private readonly CancellationToken _cancellationToken;

	public T Current => _current;
	public CancellationToken CancellationToken => _cancellationToken;

	public bool ObtainsLastPage => false;
	public bool IsLastPageAvailable => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public bool IsLastPage => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public event Action<bool> OnLastPage
	{
		add => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
		remove => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	}

	public bool ObtainsTotalCount => false;
	public bool IsTotalCountAvailable => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public long TotalCount => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public event Action<long> OnTotalCount
	{
		add => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
		remove => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	}

	public DSAsyncCursor(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
	{
		_queryable = queryable;
		_cancellationToken = cancellationToken;
		_current = default(T)!;
	}

	public async ValueTask<bool> MoveNextAsync(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_asyncEnumerator == null)
		{
			if (_queryable == null) throw new ObjectDisposedException(GetType().FullName);
			if (_syncEnumerator != null) throw new InvalidOperationException();

			_asyncEnumerator = _queryable.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
		}

		if (await _asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
		{
			_current = _asyncEnumerator.Current;
			return true;
		}

		await DisposeAsync().ConfigureAwait(false);
		return false;
	}

	public bool MoveNext(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_syncEnumerator == null)
		{
			if (_queryable == null) throw new ObjectDisposedException(GetType().FullName);
			if (_asyncEnumerator != null) throw new InvalidOperationException();

			_syncEnumerator = _queryable.GetEnumerator();
		}

		if (_syncEnumerator.MoveNext())
		{
			_current = _syncEnumerator.Current;
			return true;
		}

		Dispose();
		return false;
	}

	public async ValueTask DisposeAsync()
	{
		if (_queryable != null)
		{
			var queryable = _queryable as IDisposable;
			_queryable = null;

			var asyncEnumerator = _asyncEnumerator;
			_asyncEnumerator = null;

			var syncEnumerator = _syncEnumerator;
			_syncEnumerator = null;

			if (asyncEnumerator != null) await asyncEnumerator.DisposeAsync().ConfigureAwait(false);
			syncEnumerator?.Dispose();
			queryable?.Dispose();
		}
	}

	public void Dispose()
	{
		if (_queryable != null)
		{
			var queryable = _queryable as IDisposable;
			_queryable = null;

			var asyncEnumerator = _asyncEnumerator;
			_asyncEnumerator = null;

			var syncEnumerator = _syncEnumerator;
			_syncEnumerator = null;

			if (asyncEnumerator != null)
			{
				if (asyncEnumerator is IDisposable disposable)
				{
					disposable.Dispose();
				}
				else
				{
					AsyncHelper.RunSync(async () => await asyncEnumerator.DisposeAsync());
				}
			}
			syncEnumerator?.Dispose();
			queryable?.Dispose();
		}
	}
}