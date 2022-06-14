namespace QBCore.DataSource;

public interface IDataSourceHost<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>
{
	object SyncRoot { get; }

	ValueTask CreateListenerAsync<T>(bool attachTransient = false) where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;
	ValueTask CreateListenerAsync<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;
	ValueTask<bool> RemoveListenerAsync<T>() where T : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;

	void CreateAction<T>(bool attachTransient = false) where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;
	void CreateAction<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;
	bool RemoveAction<T>() where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;
	Task<bool> RemoveActionAsync<T>() where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>;
}