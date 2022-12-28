namespace QBCore.DataSource.QueryBuilder;

internal abstract class SqlInsertQBBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>, ISqlInsertQBBuilder<TDoc, TDto> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;
	public override IReadOnlyList<QBContainer> Containers => _containers ?? EmptyLists.Containers;

	private List<QBContainer>? _containers;

	public SqlInsertQBBuilder() { }
	public SqlInsertQBBuilder(SqlInsertQBBuilder<TDoc, TDto> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(1) { other._containers.First() };
	}
	public SqlInsertQBBuilder(IQBBuilder other)
	{
		if (other.DocumentType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		var container = other.Containers.FirstOrDefault();
		if (container?.DocumentType == null || container.DocumentType != typeof(TDoc) || container.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TDto).ToPretty()}' from '{other.DocumentType.ToPretty()}, {other.ProjectionType.ToPretty()}'.");
		}

		Insert(container.DBSideName);
	}

	public override QBBuilder<TDoc, TDto> AutoBuild()
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Insert query builder '{typeof(TDto).ToPretty()}' has already been initialized.");
		}

		Insert();
		return this;
	}

	protected override void OnNormalize()
	{
		if (Containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	private SqlInsertQBBuilder<TDoc, TDto> AddContainer(string? dbSideName)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
		}

		dbSideName ??= DataLayer.GetDefaultDBSideContainerName(typeof(TDoc));
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
	{
		return AddContainer(tableName);
	}
	ISqlInsertQBBuilder<TDoc, TDto> ISqlInsertQBBuilder<TDoc, TDto>.Insert(string? tableName)
	{
		return AddContainer(tableName);
	}
}