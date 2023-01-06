namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class InsertQBBuilder<TDoc, TCreate> : QBBuilder<TDoc, TCreate>, IEfCoreInsertQBBuilder<TDoc, TCreate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;
	public override IDataLayerInfo DataLayer => EfCoreDataLayer.Default;
	public override IReadOnlyList<QBContainer> Containers => _containers ?? EmptyLists.Containers;

	private List<QBContainer>? _containers;

	public InsertQBBuilder() { }
	public InsertQBBuilder(InsertQBBuilder<TDoc, TCreate> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(1) { other._containers.First() };
	}
	public InsertQBBuilder(IQBBuilder other)
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make insert query builder '{typeof(TDoc).ToPretty()}, {typeof(TCreate).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		other.Prepare();

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || top.ContainerType != ContainerTypes.Table)
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

	private InsertQBBuilder<TDoc, TCreate> AddContainer(string? dbSideName)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TCreate).ToPretty()}': initial container has already been added before.");
		}

		dbSideName ??= EfCoreDataLayer.Default.GetDefaultDBSideContainerName(typeof(TDoc));
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

	public override QBBuilder<TDoc, TCreate> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Insert query builder '{typeof(TCreate).ToPretty()}' has already been initialized.");
		}

		Insert(tableName);

		return this;
	}
	IEfCoreInsertQBBuilder<TDoc, TCreate> IEfCoreInsertQBBuilder<TDoc, TCreate>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TCreate> Insert(string? tableName = null)
		=> AddContainer(tableName);
	IEfCoreInsertQBBuilder<TDoc, TCreate> IEfCoreInsertQBBuilder<TDoc, TCreate>.Insert(string? tableName)
		=> AddContainer(tableName);
}