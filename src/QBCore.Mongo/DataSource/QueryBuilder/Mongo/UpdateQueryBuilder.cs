using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class UpdateQueryBuilder<TDocument, TUpdate> : QueryBuilder<TDocument, TUpdate>, IUpdateQueryBuilder<TDocument, TUpdate>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public UpdateQueryBuilder(QBUpdateBuilder<TDocument, TUpdate> building, IDataContext dataContext)
		: base(building, dataContext)
	{
	}

	public Task<TUpdate> UpdateAsync(
		TUpdate document,
		IReadOnlyCollection<QueryBuilder.QBCondition> conditions,
		IReadOnlyCollection<string>? modifiedFieldNames = null,
		IReadOnlyCollection<QBArgument>? parameters = null,
		DataSourceUpdateOptions? options = null,
		CancellationToken cancellationToken = default(CancellationToken)
	)
	{
		throw new NotImplementedException();
	}
}