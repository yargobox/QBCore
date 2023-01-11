using System.Data;
using System.Runtime.CompilerServices;
using MongoDB.Driver;

namespace QBCore.DataSource;

internal class DSAsyncCursor<T> : IDSAsyncCursor<T>
{
	private IAsyncCursor<T>? _cursor;
	private IEnumerator<T>? _current;
	private readonly CancellationToken _cancellationToken;

	public T Current => _current is not null ? _current.Current : default(T)!;
	public CancellationToken CancellationToken => _cancellationToken;

	public bool ObtainsLastPage => false;
	public bool IsLastPageAvailable => throw NotSupportedByThisCursor();
	public bool IsLastPage => throw NotSupportedByThisCursor();
	public event Action<bool> OnLastPage
	{
		add => throw NotSupportedByThisCursor();
		remove => throw NotSupportedByThisCursor();
	}

	public bool ObtainsTotalCount => false;
	public bool IsTotalCountAvailable => throw NotSupportedByThisCursor();
	public long TotalCount => throw NotSupportedByThisCursor();
	public event Action<long> OnTotalCount
	{
		add => throw NotSupportedByThisCursor();
		remove => throw NotSupportedByThisCursor();
	}

	public DSAsyncCursor(IAsyncCursor<T> cursor, CancellationToken cancellationToken = default(CancellationToken))
	{
		_cursor = cursor;
		_cancellationToken = cancellationToken;
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
					Dispose();
					return false;
				}
			}
			
			if (_current.MoveNext())
			{
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
                    Dispose();
					return false;
				}
			}
			
			if (_current.MoveNext())
			{
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

			current?.Dispose();
			cursor?.Dispose();
		}
	}

	static NotSupportedException NotSupportedByThisCursor([CallerMemberName] string memberName = "")
		=> new NotSupportedException($"Property or method '{memberName}' is not supported by this cursor!");
}