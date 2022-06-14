namespace QBCore.DataSource.QueryBuilder;

public interface IQBBuilder<TDocument, TProjection>
{
	void Map(Action<IQBMapper<TDocument, TProjection>> mapper);
}