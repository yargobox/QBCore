using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder;

public interface IQueryBuilder
{
	QueryBuilderTypes QueryBuilderType { get; }
	Type DocType { get; }
	DSDocumentInfo DocInfo { get; }
	Type DtoType { get; }
	DSDocumentInfo? DtoInfo { get; }
	Type DataContextInterfaceType { get; }
	IDataContext DataContext { get; }
}

public interface IQueryBuilder<TDoc, TDto> : IQueryBuilder
{
	QBBuilder<TDoc, TDto> Builder { get; }
}

public interface IInsertQueryBuilder<TDoc, TCreate> : IQueryBuilder<TDoc, TCreate>
{
	Task<TDoc> InsertAsync(TDoc document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
}

public interface ISelectQueryBuilder<TDoc, TSelect> : IQueryBuilder<TDoc, TSelect>
{
	IQueryable<TDoc> AsQueryable(DataSourceQueryableOptions? options = null);
	Task<long> CountAsync(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
	long Count(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
	Task<IDSAsyncCursor<TSelect>> SelectAsync(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
	IDSAsyncCursor<TSelect> Select(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
}

public interface IUpdateQueryBuilder<TDoc, TUpdate> : IQueryBuilder<TDoc, TUpdate>
{
	Task<TDoc?> UpdateAsync(object id, TUpdate document, IReadOnlySet<string>? validFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
}

public interface IDeleteQueryBuilder<TDoc, TDelete> : IQueryBuilder<TDoc, TDelete>
{
	Task DeleteAsync(object id, TDelete? document, DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
}

public interface IRestoreQueryBuilder<TDoc, TRestore> : IQueryBuilder<TDoc, TRestore>
{
	Task RestoreAsync(object id, TRestore? document, DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default(CancellationToken));
}