using System.Data;
using Dapper;
using Npgsql;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource;

internal class DSAsyncCursorWithLastPageMark<T> : IDSAsyncCursor<T>
{
	private NpgsqlCommand? _command;
	private NpgsqlDataReader? _dataReader;
	private Func<IDataReader, T>? _rowParser;
	private T _current;
	private readonly bool _disposeConnection;
	private readonly CancellationToken _cancellationToken;
	private int _take;
	private Action<bool>? _callback;

	public T Current => _current;
	public CancellationToken CancellationToken => _cancellationToken;

	public bool ObtainsLastPage => true;
	public bool IsLastPageAvailable => _command == null;
	public bool IsLastPage => _command == null ? _take >= 0 : throw EX.DataSource.Make.PropertyOrMethodIsNotAvailableYet();
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

	public DSAsyncCursorWithLastPageMark(NpgsqlCommand command, bool disposeConnection, int take, CancellationToken cancellationToken = default(CancellationToken))
	{
		_command = command;
		_disposeConnection = disposeConnection;
		_cancellationToken = cancellationToken;
		_current = default(T)!;
		_take = take < 0 ? int.MaxValue : take;
	}

	public async ValueTask<bool> MoveNextAsync(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_dataReader == null)
		{
			if (_command == null) throw new ObjectDisposedException(GetType().FullName);

			_dataReader = await _command.ExecuteReaderAsync(commandBehavior, cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false);
			_rowParser = _dataReader.GetRowParser<T>();
		}

		if (await _dataReader.ReadAsync(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false))
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

			_current = _rowParser!(_dataReader);
			return true;
		}

		while (await _dataReader.NextResultAsync(cancellationToken == default(CancellationToken) ? _cancellationToken : cancellationToken).ConfigureAwait(false))
		{ }

		if (_take >= 0 && _callback != null)
		{
			_callback(true);
		}

		await DisposeAsync().ConfigureAwait(false);
		return false;
	}

	public bool MoveNext(CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_dataReader == null)
		{
			if (_command == null) throw new ObjectDisposedException(GetType().FullName);

			_dataReader = _command.ExecuteReader(commandBehavior);
			_rowParser = _dataReader.GetRowParser<T>();
		}

		if (_dataReader.Read())
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

			_current = _rowParser!(_dataReader);
			return true;
		}

		while (_dataReader.NextResult())
		{ }

		if (_take >= 0 && _callback != null)
		{
			_callback(true);
		}

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