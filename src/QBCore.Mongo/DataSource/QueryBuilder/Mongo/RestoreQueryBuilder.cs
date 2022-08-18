using QBCore.Configuration;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class RestoreQueryBuilder<TDocument, TRestore> : QueryBuilder<TDocument, TRestore>, IRestoreQueryBuilder<TDocument, TRestore>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public RestoreQueryBuilder(QBRestoreBuilder<TDocument, TRestore> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}
}