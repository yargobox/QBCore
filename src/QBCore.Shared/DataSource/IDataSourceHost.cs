namespace QBCore.DataSource;

public interface IDataSourceHost<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	object SyncRoot { get; }

	ValueTask CreateListenerAsync<T>(bool attachTransient = false) where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	ValueTask CreateListenerAsync<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	ValueTask<bool> RemoveListenerAsync<T>() where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;

	void CreateAction<T>(bool attachTransient = false) where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	void CreateAction<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	bool RemoveAction<T>() where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;
	Task<bool> RemoveActionAsync<T>() where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>;
}