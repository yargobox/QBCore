using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

internal sealed class DeleteQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public override Origin Source => new Origin(typeof(DeleteQueryBuilder<TDocument, TDelete>));

	public DeleteQueryBuilder(QBBuilder<TDocument, TDelete> building)
		: base(building)
	{
	}
}