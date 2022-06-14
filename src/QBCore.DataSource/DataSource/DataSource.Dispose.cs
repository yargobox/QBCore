using QBCore.Threading.Tasks;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TDataSource>
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

			if (_nativeListener != null)
			{
				AsyncHelper.RunSync(async () => await _nativeListener.OnDetachAsync(this).ConfigureAwait(false));
				DisposeObject(_nativeListener);
				_nativeListener = null;
			}

			_serviceProvider = null!;
		}
	}
	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_listeners != null)
		{
			await ClearListenersAsync().ConfigureAwait(false);
		}

		if (_nativeListener != null)
		{
			await _nativeListener.OnDetachAsync(this).ConfigureAwait(false);
			await DisposeObjectAsync(_nativeListener).ConfigureAwait(false);
			_nativeListener = null;
		}

		_serviceProvider = null!;
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