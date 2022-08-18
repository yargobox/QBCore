namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBRestoreBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoRestoreBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public QBRestoreBuilder() { }
	public QBRestoreBuilder(QBRestoreBuilder<TDoc, TDto> other) : base(other) { }
}