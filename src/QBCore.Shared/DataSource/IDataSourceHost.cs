using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDataSourceHost<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	object SyncRoot { get; }

	void AttachListener<T>(T listener, bool attachTransient = false) where T : DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	void RemoveListener<T>(T listener) where T : DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	void RemoveListener<T>(OKeyName okeyName) where T : DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;

	ValueTask CreateListenerAsync<T>(bool attachTransient = false) where T : DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	ValueTask CreateListenerAsync<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	ValueTask<bool> RemoveListenerAsync<T>() where T : DataSourceListener<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;

	void CreateAction<T>(bool attachTransient = false) where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	void CreateAction<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	bool RemoveAction<T>() where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	Task<bool> RemoveActionAsync<T>() where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>;
}