using QBCore.Extensions.Threading.Tasks;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource>
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
			if (_colListeners != null)
			{
				AsyncHelper.RunSync(async () => await ClearListenersAsync().ConfigureAwait(false));
			}

			if (_listeners != null)
			{
				foreach (var listener in _listeners)
				{
					AsyncHelper.RunSync(async () => await listener.OnDetachAsync(this).ConfigureAwait(false));
					DisposeObject(_colListeners);
				}
				_listeners = null;
			}

			//_serviceProvider = null!;
		}
	}
	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_colListeners != null)
		{
			await ClearListenersAsync().ConfigureAwait(false);
		}

		if (_listeners != null)
		{
			foreach (var listener in _listeners)
			{
				await listener.OnDetachAsync(this).ConfigureAwait(false);
				await DisposeObjectAsync(listener).ConfigureAwait(false);
			}
			_listeners = null;
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