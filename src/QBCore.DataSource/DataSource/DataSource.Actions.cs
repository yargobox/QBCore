namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource>
{
	public void CreateAction<T>(bool attachTransient = false) where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		throw new NotImplementedException();
	}
	public void CreateAction<T>(Func<IServiceProvider, T> implementationFactory, bool attachTransient = false) where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		throw new NotImplementedException();
	}

	public bool RemoveAction<T>() where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		throw new NotImplementedException();
	}
	public Task<bool> RemoveActionAsync<T>() where T : IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
	{
		throw new NotImplementedException();
	}
}