namespace QBCore.DataSource.QueryBuilder;

internal class QBBuilder<TDocument, TProjection> :
	IQBInsertBuilder<TDocument, TProjection>,
	IQBSelectBuilder<TDocument, TProjection>,
	IQBUpdateBuilder<TDocument, TProjection>,
	IQBDeleteBuilder<TDocument, TProjection>,
	IQBSoftDelBuilder<TDocument, TProjection>,
	IQBRestoreBuilder<TDocument, TProjection>,
	ICloneable
{
	public QBBuilder() { }
	public QBBuilder(QBBuilder<TDocument, TProjection> other)
	{
		if (_containers != null) other._containers = new List<BuilderContainer>(_containers);
		if (_parameters != null) other._parameters = new List<BuilderParameter>(_parameters);
		if (_conditions != null) other._conditions = new List<BuilderCondition>(_conditions);
		if (_sortOrders != null) other._sortOrders = new List<BuilderSortOrder>(_sortOrders);
		if (_aggregations != null) other._aggregations = new List<BuilderAggregation>(_aggregations);
		if (_mapping != null) other._mapping = new QBMapper<TDocument, TProjection>(_mapping);
	}
	public object Clone()
	{
		return new QBBuilder<TDocument, TProjection>(this);
	}

	public void Map(Action<IQBMapper<TDocument, TProjection>> mapper)
	{
		_mapping = new QBMapper<TDocument, TProjection>();
		mapper(_mapping);
	}

	public QBMapper<TDocument, TProjection> Mapping
	{
		get
		{
			if (_mapping == null)
			{
				_mapping = new QBMapper<TDocument, TProjection>();
				_mapping.AutoMap();
			}
			return _mapping;
		}
	}

	public List<BuilderContainer> Containers => _containers ?? (_containers = new List<BuilderContainer>(3));
	public List<BuilderParameter> Parameters => _parameters ?? (_parameters = new List<BuilderParameter>(3));
	public List<BuilderCondition> Conditions => _conditions ?? (_conditions = new List<BuilderCondition>(3));
	public List<BuilderSortOrder> SortOrders => _sortOrders ?? (_sortOrders = new List<BuilderSortOrder>(3));
	public List<BuilderAggregation> Aggregations => _aggregations ?? (_aggregations = new List<BuilderAggregation>(3));

	public List<BuilderContainer>? _containers;
	public List<BuilderParameter>? _parameters;
	public List<BuilderCondition>? _conditions;
	public List<BuilderSortOrder>? _sortOrders;
	public List<BuilderAggregation>? _aggregations;
	public QBMapper<TDocument, TProjection>? _mapping;
}