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

	IInsertQueryBuilder<TDoc, TCreate> CreateQBInsert<TDoc, TCreate>(IDataContext dataContext) where TDoc : class;
	ISelectQueryBuilder<TDoc, TSelect> CreateQBSelect<TDoc, TSelect>(IDataContext dataContext) where TDoc : class;
	IUpdateQueryBuilder<TDoc, TUpdate> CreateQBUpdate<TDoc, TUpdate>(IDataContext dataContext) where TDoc : class;
	IDeleteQueryBuilder<TDoc, TDelete> CreateQBDelete<TDoc, TDelete>(IDataContext dataContext) where TDoc : class;
	IDeleteQueryBuilder<TDoc, TDelete> CreateQBSoftDel<TDoc, TDelete>(IDataContext dataContext) where TDoc : class;
	IRestoreQueryBuilder<TDoc, TRestore> CreateQBRestore<TDoc, TRestore>(IDataContext dataContext) where TDoc : class;
}