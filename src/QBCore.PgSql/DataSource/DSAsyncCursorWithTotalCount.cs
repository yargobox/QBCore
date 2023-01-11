using System.Data;
using Dapper;
using Npgsql;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource;

internal class DSAsyncCursorWithTotalCount<T> : IDSAsyncCursor<T>
{
	public const string TotalCountFieldName = "____total_count";

	private NpgsqlCommand? _command;
	private NpgsqlDataReader? _dataReader;
	private Func<IDataReader, T>? _rowParser;
	private T _current;
	private readonly bool _disposeConnection;
	private readonly CancellationToken _cancellationToken;
	private long _offset;
	private long _totalCount;
	private Action<long>? _callback;

	public T Current => _current;
	public CancellationToken CancellationToken => _cancellationToken;

	public bool ObtainsLastPage => true;
	public bool IsLastPageAvailable => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public bool IsLastPage => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	public event Action<bool> OnLastPage
	{
		add => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
		remove => throw EX.DataSource.Make.PropertyOrMethodNotSupportedByThisCursor();
	}

	public bool ObtainsTotalCount => true;
	public bool IsTotalCountAvailable => _totalCount >= 0;
	public long TotalCount => _totalCount >= 0 ? _totalCount : throw EX.DataSource.Make.PropertyOrMethodIsNotAvailableYet();
	public event Action<long> OnTotalCount
	{
		add => _callback += value;
		remove => _callback -= value;
	}

	public DSAsyncCursorWithTotalCount(NpgsqlCommand command, bool disposeConnection, long offset, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

		_command = command;
		_disposeConnection = disposeConnection;
		_cancellationToken = cancellationToken;
		_current = default(T)!;
		_offset = offset;
	}

	public async ValueTask<bool> MoveNextAsync(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		for (; ; )
		{
			if (_dataReader == null)
			{
				if (_command == null) throw new ObjectDisposedException(GetType().FullName);

				_dataReader = await _command.ExecuteReaderAsync(commandBehavior, cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false);

				if (!await _dataReader.ReadAsync(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false))
				{
					_totalCount = 0;
					if (_callback != null)
					{
						_callback(_totalCount);
					}

					break;
				}

				var totalCountIndex = _dataReader.GetOrdinal(TotalCountFieldName);
				_totalCount = _dataReader.GetInt64(totalCountIndex);
				if (_callback != null)
				{
					_callback(_totalCount);
				}

				if (_offset >= _totalCount)
				{
					break;
				}

				var howManyFieldsProcess = totalCountIndex == _dataReader.VisibleFieldCount - 1 ? totalCountIndex : -1;
				_rowParser = _dataReader.GetRowParser<T>(null, 0, howManyFieldsProcess);

				_current = _rowParser(_dataReader);
				return true;
			}

			if (await _dataReader.ReadAsync(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false))
			{
				_current = _rowParser!(_dataReader);
				return true;
			}

			break;
		}

		while (await _dataReader.NextResultAsync(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false)) { }

		await DisposeAsync().ConfigureAwait(false);
		return false;
	}

	public bool MoveNext(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		for (; ; )
		{
			if (_dataReader == null)
			{
				if (_command == null) throw new ObjectDisposedException(GetType().FullName);

				_dataReader = _command.ExecuteReader(commandBehavior);

				if (!_dataReader.Read())
				{
					_totalCount = 0;
					if (_callback != null)
					{
						_callback(_totalCount);
					}

					break;
				}

				var totalCountIndex = _dataReader.GetOrdinal(TotalCountFieldName);
				_totalCount = _dataReader.GetInt64(totalCountIndex);
				if (_callback != null)
				{
					_callback(_totalCount);
				}

				if (_offset >= _totalCount)
				{
					break;
				}

				var howManyFieldsProcess = totalCountIndex == _dataReader.VisibleFieldCount - 1 ? totalCountIndex : -1;
				_rowParser = _dataReader.GetRowParser<T>(null, 0, howManyFieldsProcess);

				_current = _rowParser(_dataReader);
				return true;
			}

			if (_dataReader.Read())
			{
				_current = _rowParser!(_dataReader);
				return true;
			}

			break;
		}

		while (_dataReader.NextResult()) { }

		Dispose();
		return false;
	}

	public async ValueTask DisposeAsync()
	{
		if (_command != null)
		{
			var connection = _disposeConnection ? _command.Connection : null;

			var command = _command;
			_command = null;

			var dataReader = _dataReader;
			_dataReader = null;

			_rowParser = null;
			_callback = null;

			if (dataReader != null) await dataReader.DisposeAsync().ConfigureAwait(false);
			await command.DisposeAsync().ConfigureAwait(false);
			if (connection != null) await connection.DisposeAsync().ConfigureAwait(false);
		}
	}

	public void Dispose()
	{
		if (_command != null)
		{
			var connection = _disposeConnection ? _command.Connection : null;

			var command = _command;
			_command = null;

			var dataReader = _dataReader;
			_dataReader = null;

			_rowParser = null;
			_callback = null;

			dataReader?.Close();
			command.Dispose();
			connection?.Dispose();
		}
	}
}