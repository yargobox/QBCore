namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBSoftDelBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoSoftDelBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public QBSoftDelBuilder() { }
	public QBSoftDelBuilder(QBSoftDelBuilder<TDoc, TDto> other) : base(other) { }
}