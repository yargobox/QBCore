using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

internal sealed class InsertQueryBuilder<TDocument, TCreate> : QueryBuilder<TDocument, TCreate>, IInsertQueryBuilder<TDocument, TCreate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public override Origin Source => new Origin(typeof(InsertQueryBuilder<TDocument, TCreate>));

	public InsertQueryBuilder(QBBuilder<TDocument, TCreate> building)
		: base(building)
	{
	}

	public Task<TCreate> InsertAsync(
		TCreate document,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}
}