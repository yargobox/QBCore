namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilderFactory
{
	Type DataSourceConcrete { get; }
	Type DatabaseContextInterface { get; }
	QueryBuilderTypes SupportedQueryBuilders { get; }

	IInsertQueryBuilder<TDocument, TCreate> CreateQBInsert<TDocument, TCreate>();
	ISelectQueryBuilder<TDocument, TSelect> CreateQBSelect<TDocument, TSelect>();
	IUpdateQueryBuilder<TDocument, TUpdate> CreateQBUpdate<TDocument, TUpdate>();
	IDeleteQueryBuilder<TDocument, TDelete> CreateQBDelete<TDocument, TDelete>();
	IDeleteQueryBuilder<TDocument, TDelete> CreateQBSoftDel<TDocument, TDelete>();
	IRestoreQueryBuilder<TDocument, TRestore> CreateQBRestore<TDocument, TRestore>();
}