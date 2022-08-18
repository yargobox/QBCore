namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBUpdateBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoUpdateBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public QBUpdateBuilder() { }
	public QBUpdateBuilder(QBUpdateBuilder<TDoc, TDto> other) : base(other) { }
}