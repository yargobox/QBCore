namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource>
{
	private KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>[]? _listeners;

	/// <summary>
	/// Gets and adds a listener from the DI container to the data source.
	/// </summary>
	/// <typeparam name="T">Listener type to get and add.</typeparam>
	/// <param name="attachTransient">Dispose the listener when calling <c>RemoveListener()</c> or disposing the entire data source.</param>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>
	/// The method itself is thread safe. However, calling <c>OnAttach()</c> on shared listeners will not.
	/// </remarks>
	public async ValueTask CreateListenerAsync<T>(bool attachTransient = false) where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		var listener = _serviceProvider.GetRequiredInstance<T>();
		await AddListenerInternalAsync(listener, attachTransient);
	}

	/// <summary>
	/// Gets and adds a listener from the DI container to the data source.
	/// </summary>
	/// <typeparam name="T">Listener type to get and add.</typeparam>
	/// <param name="implementationFactory"></param>
	/// <param name="attachTransient">Dispose the listener when calling <c>RemoveListener()</c> or disposing the entire data source.</param>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>
	/// The method itself is thread safe. However, calling <c>OnAttach()</c> on shared listeners will not.
	/// </remarks>
	public async ValueTask CreateListenerAsync<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		var listener = implementationFactory(_serviceProvider);
		if (listener == null)
		{
			throw new InvalidOperationException($"Unable to resolve service for type '{typeof(T).ToPretty()}'.");
		}
		await AddListenerInternalAsync(listener, attachTransient);
	}

	/// <summary>
	/// Removes a listener from the data source.
	/// </summary>
	/// <typeparam name="T">Listener type to remove.</typeparam>
	/// <returns>true when such a listener is found and removed.</returns>
	/// <remarks>
	/// The method itself is thread safe. However, calling <c>OnDetach()</c> and <c>DisposeAsync()</c> on the listener will not.
	/// </remarks>
	public async ValueTask<bool> RemoveListenerAsync<T>() where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		return await RemoveListenerInternalAsync(typeof(T));
	}

	private async ValueTask AddListenerInternalAsync(DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> listener, bool attachTransient)
	{
		var entry = KeyValuePair.Create(listener, attachTransient);
		await entry.Key.OnAttachAsync(this);
		AddListenerToArray(entry);
	}
	private async Task<bool> RemoveListenerInternalAsync(Type listenerType)
	{
		var entry = RemoveListenerFromArray(listenerType);
		if (entry.HasValue)
		{
			await entry.Value.Key.OnDetachAsync(this);
			if (entry.Value.Value)
			{
				await DisposeObjectAsync(entry.Value.Key);
			}
			return true;
		}
		return false;
	}
	private void AddListenerToArray(KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool> entry)
	{
		KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>[]? newOne, oldOne;
		do
		{
			oldOne = _listeners;

			if (oldOne == null)
			{
				newOne = new KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>[1];
				newOne[0] = entry;
			}
			else
			{
				newOne = new KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>[oldOne.Length + 1];
				Array.Copy(oldOne, newOne, oldOne.Length);
				newOne[oldOne.Length] = entry;
			}
		}
		while (Interlocked.CompareExchange(ref _listeners, newOne, oldOne) != oldOne);
	}
	private KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>? RemoveListenerFromArray(Type listenerType)
	{
		KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>? entry;
		KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>[]? newOne = null, oldOne;

		do
		{
			entry = null;
			oldOne = _listeners;

			if (oldOne != null)
			{
				for (var i = oldOne.Length - 1; i >= 0; i--)
				{
					if (oldOne[i].Key.GetType() == listenerType)
					{
						entry = oldOne[i];

						if (oldOne.Length > 1)
						{
							newOne = new KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool>[oldOne.Length - 1];
							Array.Copy(oldOne, newOne, i);
							if (i + 1 < oldOne.Length)
							{
								Array.Copy(oldOne, i + 1, newOne, i, newOne.Length - i);
							}
						}
						else
						{
							newOne = null;
						}
						break;
					}
				}
			}
		}
		while (entry.HasValue && Interlocked.CompareExchange(ref _listeners, newOne, oldOne) != oldOne);

		return entry;
	}
	private async ValueTask ClearListenersAsync()
	{
		var oldOne = _listeners;
		if (oldOne != null && Interlocked.CompareExchange(ref _listeners, null, oldOne) == oldOne)
		{
			KeyValuePair<DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>, bool> entry;
			for (var i = oldOne.Length - 1; i >= 0; i--)
			{
				entry = oldOne[i];
				await entry.Key.OnDetachAsync(this);
			}
			for (var i = oldOne.Length - 1; i >= 0; i--)
			{
				entry = oldOne[i];
				if (entry.Value) await DisposeObjectAsync(entry.Key);
			}
		}
	}
}