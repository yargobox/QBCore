using QBCore.ObjectFactory;
using QBCore.Configuration;

namespace QBCore.DataSource.QueryBuilder;

internal abstract class QueryBuilder<TDocument, TProjection> : IQueryBuilder<TDocument, TProjection>
{
	public Type DatabaseContextInterface => typeof(IMongoDbContext);
	public abstract QueryBuilderTypes QueryBuilderType { get; }
	public Type DocumentType => typeof(TDocument);
	public Type ProjectionType => typeof(TProjection);

	public QBBuilder<TDocument, TProjection> Building { get; }
	public QBMapper<TDocument, TProjection> Mapping { get; }

	public abstract Origin Source { get; }

	public QueryBuilder(QBBuilder<TDocument, TProjection> building)
	{
		Building = building;
		Mapping = building.Mapping;
	}
}