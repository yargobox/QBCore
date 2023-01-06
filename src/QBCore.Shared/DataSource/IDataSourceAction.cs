namespace QBCore.DataSource;

public interface IDataSourceAction<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	void OnAttach(IDataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore> ds);
	void OnDetach(IDataSource<TKey, TDoc, TCreate, TSelect, TUpdate, TDelete, TRestore> ds);
}