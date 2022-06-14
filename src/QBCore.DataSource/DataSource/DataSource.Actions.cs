namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TDataSource>
{
	public void CreateAction<T>(bool attachTransient = false) where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>
	{
		throw new NotImplementedException();
	}
	public void CreateAction<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>
	{
		throw new NotImplementedException();
	}

	public bool RemoveAction<T>() where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>
	{
		throw new NotImplementedException();
	}
	public Task<bool> RemoveActionAsync<T>() where T : IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>
	{
		throw new NotImplementedException();
	}
}