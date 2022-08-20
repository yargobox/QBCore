namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBSoftDelBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoSoftDelBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public QBSoftDelBuilder() { }
	public QBSoftDelBuilder(QBSoftDelBuilder<TDoc, TDto> other) : base(other) { }

	protected override void OnNormalize()
	{
		if (_containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of soft delete query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	private QBSoftDelBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string alias,
		string dbSideName,
		ContainerTypes containerType,
		ContainerOperations containerOperation)
	{
		if (_containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of soft delete query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
		}
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of soft delete query builder '{typeof(TDto).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
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

	public override QBBuilder<TDoc, TDto> Update(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Update);
	IQBMongoSoftDelBuilder<TDoc, TDto> IQBMongoSoftDelBuilder<TDoc, TDto>.Update(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Update);
}