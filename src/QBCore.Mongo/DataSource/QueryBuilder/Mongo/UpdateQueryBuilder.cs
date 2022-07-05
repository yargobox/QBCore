using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class UpdateQueryBuilder<TDocument, TUpdate> : QueryBuilder<TDocument, TUpdate>, IUpdateQueryBuilder<TDocument, TUpdate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public override Origin Source => new Origin(this.GetType());

	public UpdateQueryBuilder(QBBuilder<TDocument, TUpdate> building)
		: base(building)
	{
	}

	public Task<TUpdate> UpdateAsync(
		TUpdate document,
		IReadOnlyCollection<QBCondition> conditions,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}
}