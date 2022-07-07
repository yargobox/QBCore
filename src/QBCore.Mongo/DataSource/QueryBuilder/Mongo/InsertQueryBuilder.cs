using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class InsertQueryBuilder<TDocument, TCreate> : QueryBuilder<TDocument, TCreate>, IInsertQueryBuilder<TDocument, TCreate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public override Origin Source => new Origin(this.GetType());

	public InsertQueryBuilder(QBBuilder<TDocument, TCreate> building)
		: base(building)
	{
	}

	public Task<TCreate> InsertAsync(
		TCreate document,
		IReadOnlyCollection<QBArgument>? parameters = null,
		DataSourceInsertOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}
}