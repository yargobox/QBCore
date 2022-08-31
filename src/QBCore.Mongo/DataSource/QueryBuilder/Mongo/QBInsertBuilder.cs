namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBInsertBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, IQBMongoInsertBuilder<TDoc, TDto>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;
	public override IDataLayerInfo DataLayer => MongoDataLayer.Default;
	public override IReadOnlyList<QBContainer> Containers => _containers ?? EmptyLists.Containers;
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
		get => IdGenerator;
		set => IdGenerator = value;
	}

	private List<QBContainer>? _containers;
	private Func<IDSIdGenerator>? _idGenerator;

	public QBInsertBuilder() { }
	public QBInsertBuilder(QBInsertBuilder<TDoc, TDto> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(1) { other._containers.First() };
		_idGenerator = other._idGenerator;
	}
	public QBInsertBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		if (other.Containers.Count > 0)
		{
			var c = other.Containers.First();
			if (c.DocumentType != typeof(TDoc) || c.ContainerType != ContainerTypes.Table)
			{
				throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
			}

			Insert(c.DBSideName);
		}
	}
	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Insert query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		return Insert();
	}

	protected override void OnNormalize()
	{
		if (Containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	private QBInsertBuilder<TDoc, TDto> AddContainer(string? dbSideName)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
		}

		dbSideName ??= MongoDataLayer.Default.GetDefaultDBSideContainerName(typeof(TDoc));
		if (string.IsNullOrEmpty(dbSideName))
		{
			throw new ArgumentException(nameof(dbSideName));
		}

		if (_containers == null)
		{
			_containers = new List<QBContainer>(1);
		}

		IsNormalized = false;
		_containers.Add(new QBContainer(
			DocumentType: typeof(TDoc),
			Alias: dbSideName,
			DBSideName: dbSideName,
			ContainerType: ContainerTypes.Table,
			ContainerOperation: ContainerOperations.Insert
		));

		return this;
	}

	public override QBBuilder<TDoc, TDto> Insert(string? tableName = null)
		=> AddContainer(tableName);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.Insert(string? tableName)
		=> AddContainer(tableName);
}