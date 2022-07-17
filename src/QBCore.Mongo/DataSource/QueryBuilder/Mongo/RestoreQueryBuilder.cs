using QBCore.Configuration;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class RestoreQueryBuilder<TDocument, TRestore> : QueryBuilder<TDocument, TRestore>, IRestoreQueryBuilder<TDocument, TRestore>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public override Origin Source => new Origin(this.GetType());

	public RestoreQueryBuilder(QBBuilder<TDocument, TRestore> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}
}