namespace QBCore.DataSource.QueryBuilder;

internal abstract class SqlInsertQBBuilder<TDoc, TCreate> : QBBuilder<TDoc, TCreate>, ISqlInsertQBBuilder<TDoc, TCreate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;
	public override IReadOnlyList<QBContainer> Containers => _containers ?? EmptyLists.Containers;

	private List<QBContainer>? _containers;

	public SqlInsertQBBuilder() { }
	public SqlInsertQBBuilder(SqlInsertQBBuilder<TDoc, TCreate> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(1) { other._containers.First() };
	}
	public SqlInsertQBBuilder(IQBBuilder other)
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TCreate).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		other.Prepare();

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || (top.ContainerType != ContainerTypes.Table && top.ContainerType != ContainerTypes.View))
		{
			throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TCreate).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		AutoBuild(top.DBSideName);
	}

	protected override void OnPrepare()
	{
		if (Containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TCreate).ToPretty()}'.");
		}
	}

	private SqlInsertQBBuilder<TDoc, TCreate> AddContainer(string? dbSideName)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TCreate).ToPretty()}': initial container has already been added before.");
		}

		dbSideName ??= DataLayer.GetDefaultDBSideContainerName(typeof(TDoc));
		if (string.IsNullOrEmpty(dbSideName))
		{
			throw new ArgumentException(nameof(dbSideName));
		}

		var alias = ExtensionsForSql.ParseDbObjectName(dbSideName).Object;
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TCreate).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
		}

		if (_containers == null)
		{
			_containers = new List<QBContainer>(1);
		}

		IsNormalized = false;
		_containers.Add(new QBContainer(
			DocumentType: typeof(TDoc),
			Alias: alias,
			DBSideName: dbSideName,
			ContainerType: ContainerTypes.Table,
			ContainerOperation: ContainerOperations.Insert
		));

		return this;
	}

	public override QBBuilder<TDoc, TCreate> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Insert query builder '{typeof(TCreate).ToPretty()}' has already been initialized.");
		}

		Insert(tableName);

		return this;
	}
	ISqlInsertQBBuilder<TDoc, TCreate> ISqlInsertQBBuilder<TDoc, TCreate>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TCreate> Insert(string? tableName = null)
	{
		return AddContainer(tableName);
	}
	ISqlInsertQBBuilder<TDoc, TCreate> ISqlInsertQBBuilder<TDoc, TCreate>.Insert(string? tableName)
	{
		return AddContainer(tableName);
	}
}