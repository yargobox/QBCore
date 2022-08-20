namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBDeleteBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoDeleteBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public QBDeleteBuilder() { }
	public QBDeleteBuilder(QBDeleteBuilder<TDoc, TDto> other) : base(other) { }

	protected override void OnNormalize()
	{
		if (_containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of delete query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	private QBDeleteBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string alias,
		string dbSideName,
		ContainerTypes containerType,
		ContainerOperations containerOperation)
	{
		if (_containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of delete query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
		}
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of delete query builder '{typeof(TDto).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
		}

		IsNormalized = false;
		_containers.Add(new QBContainer(
			DocumentType: documentType,
			Alias: alias,
			DBSideName: dbSideName,
			ContainerType: containerType,
			ContainerOperation: containerOperation
		));

		return this;
	}

	public override QBBuilder<TDoc, TDto> DeleteFrom(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Delete);
	IQBMongoDeleteBuilder<TDoc, TDto> IQBMongoDeleteBuilder<TDoc, TDto>.DeleteFrom(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Delete);
}