using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using QBCore.Extensions.Threading.Tasks;

namespace QBCore.DataSource;

internal class DSAsyncCursorWithLastPageMark<T> : IDSAsyncCursor<T>
{
	private IQueryable<T>? _queryable;
	private IAsyncEnumerator<T>? _asyncEnumerator;
	private IEnumerator<T>? _syncEnumerator;
	private T _current;
	private readonly CancellationToken _cancellationToken;
	private int _take;
	private Action<bool>? _callback;

	public T Current => _current;
	public CancellationToken CancellationToken => _cancellationToken;

	public bool ObtainsLastPage => true;
	public bool IsLastPageAvailable => _queryable == null;
	public bool IsLastPage => _queryable == null ? _take >= 0 : throw NotAvailableYet();
	public event Action<bool> OnLastPage
	{
		add => _callback += value ?? throw new ArgumentNullException(nameof(value));
		remove => _callback -= value ?? throw new ArgumentNullException(nameof(value));
	}

	public bool ObtainsTotalCount => false;
	public bool IsTotalCountAvailable => throw NotSupportedByThisCursor();
	public long TotalCount => throw NotSupportedByThisCursor();
	public event Action<long> OnTotalCount
	{
		add => throw NotSupportedByThisCursor();
		remove => throw NotSupportedByThisCursor();
	}

	public DSAsyncCursorWithLastPageMark(IQueryable<T> queryable, int take, CancellationToken cancellationToken = default(CancellationToken))
	{
		_queryable = queryable;
		_cancellationToken = cancellationToken;
		_current = default(T)!;
		_take = take < 0 ? int.MaxValue : take;
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
			if (--_take < 0)
			{
				if (_callback != null)
				{
					_callback(false);
				}

				await DisposeAsync().ConfigureAwait(false);
				return false;
			}

			_current = _asyncEnumerator.Current;
			return true;
		}

		if (_take >= 0 && _callback != null)
		{
			_callback(true);
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
			if (--_take < 0)
			{
				if (_callback != null)
				{
					_callback(false);
				}

				Dispose();
				return false;
			}

			_current = _syncEnumerator.Current;
			return true;
		}

		if (_take >= 0 && _callback != null)
		{
			_callback(true);
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

			_callback = null;

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

			_callback = null;

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

	static NotSupportedException NotSupportedByThisCursor([CallerMemberName] string memberName = "")
		=> new NotSupportedException($"Property or method '{memberName}' is not supported by this cursor!");
	static InvalidOperationException NotAvailableYet([CallerMemberName] string memberName = "")
		=> new InvalidOperationException($"{nameof(IsLastPage)} is not available yet!");
}