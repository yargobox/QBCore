namespace QBCore.DataSource;

public interface IDataSourceAction<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>
{
	void OnAttach(IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete> ds);
	void OnDetach(IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete> ds);
}