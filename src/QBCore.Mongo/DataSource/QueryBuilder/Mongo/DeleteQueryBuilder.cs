using QBCore.Configuration;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class DeleteQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public DeleteQueryBuilder(QBBuilder<TDocument, TDelete> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}
}