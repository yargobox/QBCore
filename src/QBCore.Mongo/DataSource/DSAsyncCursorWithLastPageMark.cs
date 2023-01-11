using System.Data;
using MongoDB.Driver;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource;

internal class DSAsyncCursorWithLastPageMark<T> : IDSAsyncCursor<T>
{
	private IAsyncCursor<T>? _cursor;
	private IEnumerator<T>? _current;
	private readonly CancellationToken _cancellationToken;
	private int _take;
	private Action<bool>? _callback;

	public T Current => _current is not null ? _current.Current : default(T)!;
	public CancellationToken CancellationToken => _cancellationToken;

	public bool ObtainsLastPage => true;
	public bool IsLastPageAvailable => _cursor == null;
	public bool IsLastPage => _cursor == null ? _take >= 0 : throw EX.DataSource.Make.PropertyOrMethodIsNotAvailableYet();
	public event Action<bool> OnLastPage
	{
		add => _callback += value;
		remove => _callback -= value;
	}

	public bool ObtainsTotalCount => false;
	public bool IsTotalCountAvailable => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public long TotalCount => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public event Action<long> OnTotalCount
	{
		add => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
		remove => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	}

	public DSAsyncCursorWithLastPageMark(IAsyncCursor<T> cursor, int take, CancellationToken cancellationToken = default(CancellationToken))
	{
		_cursor = cursor;
		_cancellationToken = cancellationToken;
		_take = take < 0 ? int.MaxValue : take;
	}

	public async ValueTask<bool> MoveNextAsync(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		for (; ; )
		{
			if (_current == null)
			{
				if (_cursor == null) throw new ObjectDisposedException(GetType().FullName);

				if (await _cursor.MoveNextAsync((cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken)).ConfigureAwait(false))
				{
					_current = _cursor.Current.GetEnumerator();
				}
				else
				{
					if (_take >= 0 && _callback != null)
					{
						_callback(true);
					}

					Dispose();
					return false;
				}
			}
			
			if (_current.MoveNext())
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

				return true;
			}
			else
			{
				_current.Dispose();
				_current = null;

				(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ThrowIfCancellationRequested();
			}
		}
	}

	public bool MoveNext(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		for (; ; )
		{
			if (_current == null)
			{
				if (_cursor == null) throw new ObjectDisposedException(GetType().FullName);

				if (_cursor.MoveNext((cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken)))
				{
					_current = _cursor.Current.GetEnumerator();
				}
				else
				{
					if (_take >= 0 && _callback != null)
					{
						_callback(true);
					}

					Dispose();
					return false;
				}
			}
			
			if (_current.MoveNext())
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

				return true;
			}
			else
			{
				_current.Dispose();
				_current = null;

				(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ThrowIfCancellationRequested();
			}
		}
	}

	public async ValueTask DisposeAsync()
	{
		Dispose();

		await Task.CompletedTask;
	}

	public void Dispose()
	{
		if (_cursor != null)
		{
			var current = _current;
			_current = null;

			var cursor = _cursor;
			_cursor = null;

			_callback = null;

			current?.Dispose();
			cursor?.Dispose();
		}
	}
}