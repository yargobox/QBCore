using QBCore.Extensions.Threading.Tasks;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource>
{
	~DataSource()
	{
		Dispose(false);
	}
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_listeners != null)
			{
				AsyncHelper.RunSync(async () => await ClearListenersAsync().ConfigureAwait(false));
			}

			if (_listener != null)
			{
				AsyncHelper.RunSync(async () => await _listener.OnDetachAsync(this).ConfigureAwait(false));
				DisposeObject(_listener);
				_listener = null;
			}

			//_serviceProvider = null!;
		}
	}
	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_listeners != null)
		{
			await ClearListenersAsync().ConfigureAwait(false);
		}

		if (_listener != null)
		{
			await _listener.OnDetachAsync(this).ConfigureAwait(false);
			await DisposeObjectAsync(_listener).ConfigureAwait(false);
			_listener = null;
		}

		//_serviceProvider = null!;
	}

	protected static void DisposeObject(object? @ref)
	{
		if (@ref != null)
		{
			if (@ref is IDisposable dispose)
			{
				dispose?.Dispose();
			}
			else if (@ref is IAsyncDisposable asyncDispose)
			{
				if (asyncDispose != null)
				{
					AsyncHelper.RunSync(async () => await asyncDispose.DisposeAsync().ConfigureAwait(false));
				}
			}
		}
	}
	protected static async ValueTask DisposeObjectAsync(object? @ref)
	{
		if (@ref != null)
		{
			if (@ref is IAsyncDisposable asyncDispose)
			{
				if (asyncDispose != null)
				{
					await asyncDispose.DisposeAsync().ConfigureAwait(false);
				}
			}
			else if (@ref is IDisposable dispose)
			{
				dispose?.Dispose();
			}
		}
	}
}