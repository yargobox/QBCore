namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBInsertBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoInsertBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	private Func<IDSIdGenerator>? _idGenerator;

	public override Func<IDSIdGenerator>? IdGenerator
	{
		get => _idGenerator;
		set
		{
			if (_idGenerator != null)
				throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': option '{nameof(IdGenerator)}' is already set.");
			_idGenerator = value;
		}
	}
	Func<IDSIdGenerator>? IQBMongoInsertBuilder<TDoc, TDto>.IdGenerator
	{
		get => this.IdGenerator;
		set => this.IdGenerator = value;
	}

	public QBInsertBuilder() { }
	public QBInsertBuilder(QBInsertBuilder<TDoc, TDto> other) : base(other)
	{
		_idGenerator = other._idGenerator;
	}

	protected override void OnNormalize()
	{
		if (_containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	private QBInsertBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string alias,
		string dbSideName,
		ContainerTypes containerType,
		ContainerOperations containerOperation)
	{
		if (_containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
		}
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
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

	public override QBBuilder<TDoc, TDto> InsertTo(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Insert);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.InsertTo(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Insert);
}