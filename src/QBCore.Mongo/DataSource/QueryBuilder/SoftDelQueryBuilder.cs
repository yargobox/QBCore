using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

internal sealed class SoftDelQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public override Origin Source => new Origin(this.GetType());

	public SoftDelQueryBuilder(QBBuilder<TDocument, TDelete> building)
		: base(building)
	{
	}
}