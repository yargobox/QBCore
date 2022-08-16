namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBDeleteBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoDeleteBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public QBDeleteBuilder() { }
	public QBDeleteBuilder(QBDeleteBuilder<TDoc, TDto> other) : base(other) { }
}