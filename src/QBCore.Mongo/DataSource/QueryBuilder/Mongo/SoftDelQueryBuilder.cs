using QBCore.Configuration;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SoftDelQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public SoftDelQueryBuilder(QBSoftDelBuilder<TDocument, TDelete> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}
}