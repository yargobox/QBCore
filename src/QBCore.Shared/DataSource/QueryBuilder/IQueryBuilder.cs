using System.Linq.Expressions;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilder
{
	QueryBuilderTypes QueryBuilderType { get; }
	Type DocumentType { get; }
	DSDocumentInfo DocumentInfo { get; }
	Type ProjectionType { get; }
	DSDocumentInfo? ProjectionInfo { get; }
	Type DatabaseContextInterfaceType { get; }
	IDataContext DataContext { get; }
}

public interface IQueryBuilder<TDocument, TProjection> : IQueryBuilder
{
	QBBuilder<TDocument, TProjection> Builder { get; }
}

public interface IInsertQueryBuilder<TDocument, TCreate> : IQueryBuilder<TDocument, TCreate>
{
	IQBInsertBuilder<TDocument, TCreate> InsertBuilder { get; }
	Task<TDocument> InsertAsync(
		TDocument document,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	);
}

public interface ISelectQueryBuilder<TDocument, TSelect> : IQueryBuilder<TDocument, TSelect>
{
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