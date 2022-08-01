using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class InsertQueryBuilder<TDocument, TCreate> : QueryBuilder<TDocument, TCreate>, IInsertQueryBuilder<TDocument, TCreate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public IQBInsertBuilder<TDocument, TCreate> InsertBuilder => Builder;

	public InsertQueryBuilder(QBBuilder<TDocument, TCreate> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}

	public Task<object> InsertAsync(
		TCreate document,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}
}