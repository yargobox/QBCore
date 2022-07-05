using System.Linq.Expressions;
using QBCore.Extensions.Linq;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal class QBBuilder<TDoc, TDto> :
	IQBInsertBuilder<TDoc, TDto>,
	IQBMongoSelectBuilder<TDoc, TDto>,
	IQBUpdateBuilder<TDoc, TDto>,
	IQBDeleteBuilder<TDoc, TDto>,
	IQBSoftDelBuilder<TDoc, TDto>,
	IQBRestoreBuilder<TDoc, TDto>,
	ICloneable
{
	public QBBuilder() { }
	public QBBuilder(QBBuilder<TDoc, TDto> other)
	{
		if (other._containers != null) _containers = new List<BuilderContainer>(other._containers);
		if (other._fields != null) _fields = new List<BuilderField>(other._fields);
		if (other._parameters != null) _parameters = new List<BuilderParameter>(other._parameters);
		if (other._conditions != null) _conditions = new List<BuilderCondition>(other._conditions);
		if (other._sortOrders != null) _sortOrders = new List<BuilderSortOrder>(other._sortOrders);
		if (other._aggregations != null) _aggregations = new List<BuilderAggregation>(other._aggregations);
	}
	public object Clone()
	{
		return new QBBuilder<TDoc, TDto>(this);
	}

	public List<BuilderContainer> Containers => _containers ?? (_containers = new List<BuilderContainer>(3));
	public List<BuilderField> Fields => _fields ?? (_fields = new List<BuilderField>(8));
	public List<BuilderParameter> Parameters => _parameters ?? (_parameters = new List<BuilderParameter>(3));
	public List<BuilderCondition> Conditions => _conditions ?? (_conditions = new List<BuilderCondition>(3));
	public List<BuilderSortOrder> SortOrders => _sortOrders ?? (_sortOrders = new List<BuilderSortOrder>(3));
	public List<BuilderAggregation> Aggregations => _aggregations ?? (_aggregations = new List<BuilderAggregation>(3));

	private List<BuilderContainer>? _containers;
	private List<BuilderField>? _fields;
	private List<BuilderParameter>? _parameters;
	private List<BuilderCondition>? _conditions;
	private List<BuilderSortOrder>? _sortOrders;
	private List<BuilderAggregation>? _aggregations;

	private int _parentheses;
	private bool? _isByOr;

	private static readonly List<BuilderContainer> _emptyContainers = new List<BuilderContainer>();
	private static readonly List<BuilderCondition> _emptyConditions = new List<BuilderCondition>();

	public void NormalizeSelect()
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var containers = _containers ?? _emptyContainers;
		var conditions = _conditions ?? _emptyConditions;

		var rootIndex = containers.FindIndex(x => x.ContainerOperation == BuilderContainerOperations.Select);
		if (rootIndex < 0)
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if (containers.All(x => x.ContainerType == BuilderContainerTypes.Table || x.ContainerType == BuilderContainerTypes.View))
		{
			BuilderContainer? top, bottom, temp;

			// Move a root container to the beginning
			//
			top = containers[rootIndex];
			if (rootIndex != 0)
			{
				containers.RemoveAt(rootIndex);
				containers.Insert(0, top);
			}

			// The root container cannot have connect conditions (depends on others)
			//
			if (conditions.Any(x => x.IsOnField && x.Name == top.Name))
			{
				throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}

			// Check for connect conditions.
			// Move all containers that do not have connect conditions down (cross joins).
			// If some of them are referenced by others, then the next sort will raise them to the desired position up.
			// Respect the initial order of these containers.
			//
			for (int i = 1, j = 0; i < containers.Count - j; i++)
			{
				temp = containers[i];

				if (temp.ContainerOperation == BuilderContainerOperations.LeftJoin || temp.ContainerOperation == BuilderContainerOperations.Join)
				{
					if (!conditions.Any(x => x.IsConnectOnField))
					{
						throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
					}
				}
				else if (temp.ContainerOperation == BuilderContainerOperations.CrossJoin)
				{
					if (conditions.Any(x => x.IsConnectOnField))
					{
						throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
					}

					containers.RemoveAt(i);
					containers.Add(temp);
					i--;
					j++;
				}
			}

			// Sort containers in expression tree order, avoid an infinite loop.
			// Start with the penultimate container and go up to the second one.
			// This sort does not take into account the "connect" and "non-connect" conditions,
			// because in MongoDB the conditions for the fields of the joined collections should be
			// applied in the lookup pipeline, especially if there are no such fields in the result subset.
			//
			for (int i = containers.Count - 2; i > 0; i--)
			{
				// remember a container at this position
				temp = containers[i];

			L_RESCAN:
				top = containers[i];
				foreach (var topDependOn in conditions
					.Where(x => x.IsOnField && x.Name == top.Name)
					.Select(x => x.RefName))
				{
					for (int j = i + 1; j < containers.Count; j++)
					{
						bottom = containers[j];
						if (bottom.Name == topDependOn)
						{
							// The signal for an infinite loop is the return of the container to its previous position
							if (bottom == temp)
							{
								throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
							}

							containers.Swap(i, j);
							goto L_RESCAN;
						}
					}
				}
			}
		}
		else
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		// Verify parentheses in conditions
		//
		int parentheses;
		foreach (var cont in containers)
		{
			parentheses = 0;
			foreach (var cond in conditions.Where(x => x.IsConnect == false && x.Name == cont.Name))
			{
				parentheses += cond.Parentheses;
				if (parentheses < 0)
				{
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
				}
			}
			if (parentheses != 0)
			{
				throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}
		}
	}

	private QBBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string name,
		string container,
		BuilderContainerTypes containerType,
		BuilderContainerOperations containerOperation,
		string? connectTemplate = null)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		if ((containerOperation & BuilderContainerOperations.MainMask) != BuilderContainerOperations.None)
		{
			if (Containers.Any(x => x.ContainerOperation.HasFlag(BuilderContainerOperations.MainMask) || x.Name == name))
				throw new InvalidOperationException($"QB: '{name}' has already been added or another initial container has been added before.");
		}
		else if (Containers.Any(x => x.Name == name))
		{
			throw new InvalidOperationException($"QB: '{name}' has been added before.");
		}

		Containers.Add(new BuilderContainer(
			DocumentType: documentType,
			Name: name,
			DBSideName: container,
			ContainerType: containerType,
			ContainerOperation: containerOperation,
			ConnectTemplate: connectTemplate
		));

		return this;
	}

	private QBBuilder<TDoc, TDto> AddCondition<TLocal, TRef>(
		BuilderConditionFlags flags,
		string? name,
		Expression<Func<TLocal, object?>> field,
		string? refName,
		Expression<Func<TRef, object?>>? refField,
		object? constValue,
		string? paramName,
		ConditionOperations operation)
	{
		if (_isByOr != null && (flags.HasFlag(BuilderConditionFlags.IsConnect) || (Conditions.Count <= 0 || Conditions[Conditions.Count - 1].IsConnect)))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var trueContainerType = typeof(TLocal) == typeof(TDto) ? typeof(TDoc) : typeof(TLocal);
		if (name == null)
		{
			name = Containers.Single(x => x.DocumentType == trueContainerType).Name;
		}
		else if (!Containers.Any(x => x.Name == name && x.DocumentType == trueContainerType))
		{
			throw new InvalidOperationException($"QB: '{name}' for document '{trueContainerType.ToPretty()}' has not been added yet.");
		}

		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		var onWhat = flags & (BuilderConditionFlags.OnField | BuilderConditionFlags.OnConst | BuilderConditionFlags.OnParam);
		if (onWhat == BuilderConditionFlags.OnField)
		{
			if (refName == null)
			{
				refName = Containers.Single(x => x.DocumentType == typeof(TRef)).Name;
			}
			else if (!Containers.Any(x => x.Name == refName && x.DocumentType == typeof(TRef)))
			{
				throw new InvalidOperationException($"QB: '{refName}' for document '{typeof(TRef).ToPretty()}' has not been added yet.");
			}

			if (refField == null)
			{
				throw new ArgumentNullException(nameof(refField));
			}
		}
		else if (onWhat == BuilderConditionFlags.OnParam)
		{
			if (string.IsNullOrWhiteSpace(paramName))
			{
				throw new ArgumentNullException(nameof(paramName));
			}
		}

		if (_isByOr == true)
		{
			flags |= BuilderConditionFlags.IsByOr;
		}

		Conditions.Add(new BuilderCondition(
			Flags: flags,
			Parentheses: _parentheses,
			Name: name,
			Field: field,
			RefName: refName,
			RefField: refField,
			Value: onWhat == BuilderConditionFlags.OnParam ? paramName : onWhat == BuilderConditionFlags.OnConst ? constValue : null,
			Operation: operation
		));

		_isByOr = null;
		_parentheses = 0;

		return this;
	}

	private QBBuilder<TDoc, TDto> AddInclude<TOther>(string? otherName, Expression<Func<TOther, object?>> docNavPath, Expression<Func<TDto, object?>>? dtoNavPath)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		if (otherName == null)
		{
			otherName = Containers.Single(x => x.DocumentType == typeof(TOther)).Name;
		}
		else if (!Containers.Any(x => x.Name == otherName && x.DocumentType == typeof(TOther)))
		{
			throw new InvalidOperationException($"QB: '{otherName}' for document '{typeof(TOther).ToPretty()}' has not been added yet.");
		}

		if (docNavPath == null)
		{
			throw new ArgumentNullException(nameof(docNavPath));
		}

		Fields.Add(new BuilderField(
			Name: otherName,
			DocNavPath: docNavPath.GetPropertyOrFieldPath(),
			DtoNavPath: dtoNavPath?.GetPropertyOrFieldPath()!
		));

		return this;
	}

	public IQBMongoSelectBuilder<TDoc, TDto> Include(Expression<Func<TDoc, object?>> docNavPath)
		=> AddInclude<TDoc>(null, docNavPath, null);
	public IQBMongoSelectBuilder<TDoc, TDto> Include(Expression<Func<TDoc, object?>> docNavPath, Expression<Func<TDto, object?>> dtoNavPath)
		=> AddInclude<TDoc>(null, docNavPath, dtoNavPath);
	public IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(Expression<Func<TOther, object?>> docNavPath)
		=> AddInclude<TOther>(null, docNavPath, null);
	public IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(string otherName, Expression<Func<TOther, object?>> docNavPath)
		=> AddInclude<TOther>(otherName, docNavPath, null);
	public IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(Expression<Func<TOther, object?>> docNavPath, Expression<Func<TDto, object?>> dtoNavPath)
		=> AddInclude<TOther>(null, docNavPath, dtoNavPath);
	public IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(string otherName, Expression<Func<TOther, object?>> docNavPath, Expression<Func<TDto, object?>> dtoNavPath)
		=> AddInclude<TOther>(otherName, docNavPath, dtoNavPath);

	public IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Select);
	public IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string name, string tableName, string? conditionTemplate = null)
		=> AddContainer(typeof(TDoc), name, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Select, conditionTemplate);

	public IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TOther>(string tableName)
		=> AddContainer(typeof(TOther), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.LeftJoin);
	public IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TOther>(string name, string tableName)
		=> AddContainer(typeof(TOther), name, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.LeftJoin);

	public IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TOther>(string tableName)
		=> AddContainer(typeof(TOther), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Join);
	public IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TOther>(string name, string tableName)
		=> AddContainer(typeof(TOther), name, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Join);

	public IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TOther>(string tableName)
		=> AddContainer(typeof(TOther), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.CrossJoin);
	public IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TOther>(string name, string tableName)
		=> AddContainer(typeof(TOther), name, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.CrossJoin);

	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, null, field, refName, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, name, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, name, field, refName, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string name, Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnConst, name, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string name, Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnParam, name, field, null, null, null, paramName, operation);

	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, null, field, refName, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, name, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, name, field, refName, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string name, Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnConst, name, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string name, Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnParam, name, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, ConditionOperations operation)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(string name, Expression<Func<TDoc, object?>> field, object? constValue, ConditionOperations operation)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnConst, name, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, ConditionOperations operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(string name, Expression<Func<TDoc, object?>> field, ConditionOperations operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnParam, name, field, null, null, null, paramName, operation);

	public IQBMongoSelectBuilder<TDoc, TDto> Begin()
	{
		_parentheses++;
		return this;
	}
	public IQBMongoSelectBuilder<TDoc, TDto> End()
	{
		var lastIndex = Conditions.Count - 1;
		if (lastIndex < 0 || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var lastCond = Conditions[lastIndex];
		if (lastCond.IsConnect)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		Conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - 1 };

		return this;
	}

	public IQBMongoSelectBuilder<TDoc, TDto> And()
	{
		if (_isByOr != null)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var lastIndex = Conditions.Count - 1;
		if (lastIndex < 0)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var lastCond = Conditions[lastIndex];
		if (lastCond.IsConnect)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		_isByOr = false;

		return this;
	}
	public IQBMongoSelectBuilder<TDoc, TDto> Or()
	{
		if (_isByOr != null)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var lastIndex = Conditions.Count - 1;
		if (lastIndex < 0)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		var lastCond = Conditions[lastIndex];
		if (lastCond.IsConnect)
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder condition '{typeof(TDto).ToPretty()}'.");
		}

		_isByOr = true;

		return this;
	}
}