namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class InsertQBBuilder<TDoc, TCreate> : QBBuilder<TDoc, TCreate>, IMongoInsertQBBuilder<TDoc, TCreate>
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
			{
				throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TCreate).ToPretty()}': option '{nameof(IdGenerator)}' is already set.");
			}
			_idGenerator = value;
		}
	}
	Func<IDSIdGenerator>? IMongoInsertQBBuilder<TDoc, TCreate>.IdGenerator
	{
		get => IdGenerator;
		set => IdGenerator = value;
	}

	private List<QBContainer>? _containers;
	private Func<IDSIdGenerator>? _idGenerator;

	public InsertQBBuilder() { }
	public InsertQBBuilder(InsertQBBuilder<TDoc, TCreate> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(1) { other._containers.First() };
		_idGenerator = other._idGenerator;
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

	public override QBBuilder<TDoc, TCreate> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Insert query builder '{typeof(TCreate).ToPretty()}' has already been initialized.");
		}

		Insert(tableName);

		return this;
	}
	IMongoInsertQBBuilder<TDoc, TCreate> IMongoInsertQBBuilder<TDoc, TCreate>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TCreate> Insert(string? tableName = null)
		=> AddContainer(tableName);
	IMongoInsertQBBuilder<TDoc, TCreate> IMongoInsertQBBuilder<TDoc, TCreate>.Insert(string? tableName)
		=> AddContainer(tableName);
}