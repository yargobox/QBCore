namespace QBCore.DataSource;

public interface IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	void OnAttach(IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> ds);
	void OnDetach(IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> ds);
}