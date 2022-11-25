using QBCore.Configuration;

namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilderFactory
{
	IDataLayerInfo DataLayer { get; }
	DSTypeInfo DSTypeInfo { get; }
	QueryBuilderTypes SupportedQueryBuilders { get; }

	IQBBuilder? DefaultInsertBuilder { get; }
	IQBBuilder? DefaultSelectBuilder { get; }
	IQBBuilder? DefaultUpdateBuilder { get; }
	IQBBuilder? DefaultDeleteBuilder { get; }
	IQBBuilder? DefaultSoftDelBuilder { get; }
	IQBBuilder? DefaultRestoreBuilder { get; }

	IInsertQueryBuilder<TDocument, TCreate> CreateQBInsert<TDocument, TCreate>(IDataContext dataContext) where TDocument : class;
	ISelectQueryBuilder<TDocument, TSelect> CreateQBSelect<TDocument, TSelect>(IDataContext dataContext) where TDocument : class;
	IUpdateQueryBuilder<TDocument, TUpdate> CreateQBUpdate<TDocument, TUpdate>(IDataContext dataContext) where TDocument : class;
	IDeleteQueryBuilder<TDocument, TDelete> CreateQBDelete<TDocument, TDelete>(IDataContext dataContext) where TDocument : class;
	IDeleteQueryBuilder<TDocument, TDelete> CreateQBSoftDel<TDocument, TDelete>(IDataContext dataContext) where TDocument : class;
	IRestoreQueryBuilder<TDocument, TRestore> CreateQBRestore<TDocument, TRestore>(IDataContext dataContext) where TDocument : class;
}