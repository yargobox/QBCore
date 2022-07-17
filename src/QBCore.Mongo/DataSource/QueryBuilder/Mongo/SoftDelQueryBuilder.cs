using QBCore.Configuration;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SoftDelQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public override Origin Source => new Origin(this.GetType());

	public SoftDelQueryBuilder(QBBuilder<TDocument, TDelete> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}
}