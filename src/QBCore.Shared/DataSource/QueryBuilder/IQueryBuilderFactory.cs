using QBCore.Configuration;

namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilderFactory
{
	IDataLayerInfo DataLayer { get; }
	Type DataSourceConcrete { get; }
	QueryBuilderTypes SupportedQueryBuilders { get; }

	IQBBuilder? DefaultInsertBuilder { get; }
	IQBBuilder? DefaultSelectBuilder { get; }
	IQBBuilder? DefaultUpdateBuilder { get; }
	IQBBuilder? DefaultDeleteBuilder { get; }
	IQBBuilder? DefaultSoftDelBuilder { get; }
	IQBBuilder? DefaultRestoreBuilder { get; }

	IInsertQueryBuilder<TDocument, TCreate> CreateQBInsert<TDocument, TCreate>(IDataContext dataContext);
	ISelectQueryBuilder<TDocument, TSelect> CreateQBSelect<TDocument, TSelect>(IDataContext dataContext);
	IUpdateQueryBuilder<TDocument, TUpdate> CreateQBUpdate<TDocument, TUpdate>(IDataContext dataContext);
	IDeleteQueryBuilder<TDocument, TDelete> CreateQBDelete<TDocument, TDelete>(IDataContext dataContext);
	IDeleteQueryBuilder<TDocument, TDelete> CreateQBSoftDel<TDocument, TDelete>(IDataContext dataContext);
	IRestoreQueryBuilder<TDocument, TRestore> CreateQBRestore<TDocument, TRestore>(IDataContext dataContext);
}