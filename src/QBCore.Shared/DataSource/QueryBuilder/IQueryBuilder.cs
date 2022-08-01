using System.Linq.Expressions;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilder : IOriginal
{
	QueryBuilderTypes QueryBuilderType { get; }
	Type DocumentType { get; }
	Type ProjectionType { get; }
	
	Type DatabaseContextInterface { get; }
	IDataContext DataContext { get; }
}

public interface IQueryBuilder<TDocument, TProjection> : IQueryBuilder
{
}

public interface IInsertQueryBuilder<TDocument, TCreate> : IQueryBuilder<TDocument, TCreate>
{
	IQBInsertBuilder<TDocument, TCreate> InsertBuilder { get; }
	Task<object> InsertAsync(
		TCreate document,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface ISelectQueryBuilder<TDocument, TSelect> : IQueryBuilder<TDocument, TSelect>
{
	IQBSelectBuilder<TDocument, TSelect> SelectBuilder { get; }
	IQueryable<TDocument> AsQueryable(DataSourceQueryableOptions? options = null);
	Task<long> CountAsync(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
	Task<IDSAsyncCursor<TSelect>> SelectAsync(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
}

public interface IUpdateQueryBuilder<TDocument, TUpdate> : IQueryBuilder<TDocument, TUpdate>
{
	Task<TUpdate> UpdateAsync(
		TUpdate document,
		IReadOnlyCollection<QBCondition> conditions,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		IReadOnlyCollection<QBArgument>? parameters = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface IDeleteQueryBuilder<TDocument, TDelete> : IQueryBuilder<TDocument, TDelete>
{
}

public interface IRestoreQueryBuilder<TDocument, TRestore> : IQueryBuilder<TDocument, TRestore>
{
}