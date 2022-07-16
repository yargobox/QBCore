using System.Linq.Expressions;
using QBCore.Extensions.Linq;

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
		if (other._connects != null) _connects = new List<BuilderCondition>(other._connects);
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
	public List<BuilderCondition> Connects => _connects ?? (_connects = new List<BuilderCondition>(8));
	public List<BuilderCondition> Conditions => _conditions ?? (_conditions = new List<BuilderCondition>(8));
	public List<BuilderSortOrder> SortOrders => _sortOrders ?? (_sortOrders = new List<BuilderSortOrder>(3));
	public List<BuilderAggregation> Aggregations => _aggregations ?? (_aggregations = new List<BuilderAggregation>(3));

	private List<BuilderContainer>? _containers;
	private List<BuilderField>? _fields;
	private List<BuilderParameter>? _parameters;
	private List<BuilderCondition>? _connects;
	private List<BuilderCondition>? _conditions;
	private List<BuilderSortOrder>? _sortOrders;
	private List<BuilderAggregation>? _aggregations;

	private int _parentheses;
	private int _sumParentheses;
	private bool? _isByOr;
	private int _autoOpenedParentheses;

	private static readonly List<BuilderContainer> _emptyContainers = new List<BuilderContainer>();
	private static readonly List<BuilderCondition> _emptyConditions = new List<BuilderCondition>();

	public void NormalizeSelect()
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		CompleteAutoOpenedParentheses();
		if (_sumParentheses != 0 || _autoOpenedParentheses != 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		var containers = _containers ?? _emptyContainers;
		var connects = _connects ?? _emptyConditions;
		var conditions = _conditions ?? _emptyConditions;

		var rootIndex = containers.FindIndex(x => x.ContainerOperation == BuilderContainerOperations.Select);
		if (rootIndex < 0)
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if (containers.Any(x => x.ContainerType != BuilderContainerTypes.Table && x.ContainerType != BuilderContainerTypes.View))
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TDto).ToPretty()}'.");
		}

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
		if (connects.Any(x => x.Name == top.Name))
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
				if (!connects.Any(x => x.IsConnectOnField))
				{
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
				}
			}
			else if (temp.ContainerOperation == BuilderContainerOperations.CrossJoin)
			{
				if (connects.Any(x => x.IsConnectOnField))
				{
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}'.");
				}

				containers.RemoveAt(i);
				containers.Add(temp);
				i--;
				j++;
			}
		}

		// Sort containers based on the connect condition, avoid an infinite loop.
		// Start with the penultimate container and go up to the second one.
		//
		for (int i = containers.Count - 2; i > 0; i--)
		{
			// remember a container at this position
			temp = containers[i];

		L_RESCAN:
			top = containers[i];
			foreach (var topDependOn in connects
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

	private QBBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string containerName,
		string dbSideName,
		BuilderContainerTypes containerType,
		BuilderContainerOperations containerOperation)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if ((containerOperation & BuilderContainerOperations.MainMask) != BuilderContainerOperations.None &&
			Containers.Any(x => x.ContainerOperation.HasFlag(BuilderContainerOperations.MainMask)))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': another initial container has already been added before.");
		}
		if (Containers.Any(x => x.Name == containerName))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': initial container '{containerName}' has already been added before.");
		}

		Containers.Add(new BuilderContainer(
			DocumentType: documentType,
			Name: containerName,
			DBSideName: dbSideName,
			ContainerType: containerType,
			ContainerOperation: containerOperation
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
		FO operation)
	{
		var trueContainerType = typeof(TLocal) == typeof(TDto) ? typeof(TDoc) : typeof(TLocal);
		if (name == null)
		{
			name = Containers.Single(x => x.DocumentType == trueContainerType).Name;
		}
		else if (!Containers.Any(x => x.Name == name && x.DocumentType == trueContainerType))
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': referenced container '{name}' of  document '{trueContainerType.ToPretty()}' has not been added yet.");
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
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': referenced container '{refName}' of  document '{typeof(TRef).ToPretty()}' has not been added yet.");
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

		if (flags.HasFlag(BuilderConditionFlags.IsConnect))
		{
			Connects.Add(new BuilderCondition(
				flags: flags,
				parentheses: 0,
				name: name,
				field: field,
				refName: refName,
				refField: refField,
				value: onWhat == BuilderConditionFlags.OnParam ? paramName : onWhat == BuilderConditionFlags.OnConst ? constValue : null,
				operation: operation
			));
		}
		else
		{
			_isByOr = _isByOr.HasValue && _isByOr.Value;

			// Automatically add parentheses if needed
			// Skip conditions like these, they don't need it
			// a		=> a OR ?
			// (a		=> (a OR ?
			// .. (a	=> .. (a OR ?
			//
			if (Conditions.Count > 1 && Conditions[Conditions.Count - 1].Parentheses <= 0)
			{
				BuilderCondition cond;
				int startIndex, lastIndex = Conditions.Count - 1;

				if (_isByOr.Value)
				{
					/*
						a OR b						=> a OR b OR ?
						a AND b						=> (a AND b) OR ?
						(a AND b)					=> (a AND b) OR ?
						(a OR b)					=> (a OR b) OR ?
						((a AND b)					=> ((a AND b) OR ?
						(a AND b) OR c				=> (a AND b) OR c OR ?
						((a OR b) AND (c AND d		=> ((a OR b) AND ((c AND d) OR ?
						((a OR b) AND c				=> (((a OR b) AND c) OR ?
						(a AND (b OR c				=> (a AND (b OR c OR ?
						(a OR (b AND c				=> (a OR ((b AND c) OR ?
						(a OR (b AND c)				=> (a OR (b AND c) OR ?
						(a AND (b OR c)				=> ((a AND (b OR c)) OR ?
						z AND (a AND (b OR c)	 	=> z AND ((a AND (b OR c)) OR ?
						z AND (a AND (b OR c))	 	=> (z AND (a AND (b OR c))) OR ?
					*/
					startIndex = 0;
					bool needsParentheses = false;
					for (int i = lastIndex, sum = 0; i >= 0; i--)
					{
						cond = Conditions[i];

						sum += cond.Parentheses;
						if (sum == 0)
						{
							if (i > 0 && cond.IsByOr)
							{
								startIndex = -1;
								break;
							}
							needsParentheses = true;
						}
						else if (sum > 0)
						{
							startIndex = i;
							break;
						}
					}
					if (startIndex >= 0 && needsParentheses)
					{
						cond = Conditions[startIndex];
						Conditions[startIndex] = cond with { Parentheses = cond.Parentheses + 1 };
						cond = Conditions[lastIndex];
						Conditions[lastIndex] = cond with { Parentheses = cond.Parentheses - 1 };
					}
				}
				else
				{
					/*
						0 | 1 & 2
						0 | (1 & 2
						0 | (1 & 2 & (3
						0 | (1 & 2 & (3 | 4)
						0 | (1 & 2 & (3 | 4) | 5
						0 | (1 & 2 & (3 | 4)) | 5
					*/
					startIndex = -1;
					for (int i = lastIndex, sum = 0; i >= 0; i--)
					{
						cond = Conditions[i];

						sum += cond.Parentheses;
						if (sum == 0)
						{
							if (i > 0 && cond.IsByOr)
							{
								startIndex = i;
								break;
							}
							break;
						}
					}
					if (startIndex >= 0)
					{
						cond = Conditions[startIndex];

						if (_autoOpenedParentheses != 0 || cond.Parentheses < 0)
						{
							throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
						}

						Conditions[startIndex] = cond with { Parentheses = cond.Parentheses + 1 };

						_autoOpenedParentheses = Conditions.Skip(startIndex).Sum(x => x.Parentheses) + _parentheses;
						_sumParentheses++;
					}
				}
			}

			if (_isByOr.Value)
			{
				flags |= BuilderConditionFlags.IsByOr;
			}

			Conditions.Add(new BuilderCondition(
				flags: flags,
				parentheses: _parentheses,
				name: name,
				field: field,
				refName: refName,
				refField: refField,
				value: onWhat == BuilderConditionFlags.OnParam ? paramName : onWhat == BuilderConditionFlags.OnConst ? constValue : null,
				operation: operation
			));

			_isByOr = null;
			_parentheses = 0;
		}

		return this;
	}

	private QBBuilder<TDoc, TDto> AddInclude<TRef>(Expression<Func<TDto, object?>> field, string? refName, Expression<Func<TRef, object?>> refField)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		if (refField == null)
		{
			throw new ArgumentNullException(nameof(refField));
		}

		if (refName == null)
		{
			refName = Containers.Single(x => x.DocumentType == typeof(TRef)).Name;
		}
		else if (!Containers.Any(x => x.Name == refName && x.DocumentType == typeof(TRef)))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TDto).ToPretty()}': referenced container '{refName}' of  document '{typeof(TRef).ToPretty()}' has not been added yet.");
		}

		var fieldPath = new FieldPath(field, false);
		if (Fields.Any(x => x.Field.FullName == fieldPath.FullName))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TDto).ToPretty()}': field {fieldPath.FullName} has already been included/excluded before.");
		}

		var refFieldPath = new FieldPath(refField, true);

		Fields.Add(new BuilderField(
			Field: fieldPath,
			RefName: refName,
			RefField: refFieldPath,
			OptionalExclusion: false
		));

		return this;
	}
	
	private QBBuilder<TDoc, TDto> AddExclude(Expression<Func<TDto, object?>> field, bool optional)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		var fieldPath = new FieldPath(field, false);
		if (Fields.Any(x => x.Field.FullName == fieldPath.FullName))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TDto).ToPretty()}': field '{fieldPath.FullName}' has already been included/excluded before.");
		}

		Fields.Add(new BuilderField(
			Field: fieldPath,
			RefName: null,
			RefField: null,
			OptionalExclusion: optional
		));

		return this;
	}

	public IQBMongoSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, null, refField);
	public IQBMongoSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, refAlias, refField);

	public IQBMongoSelectBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field)
		=> AddExclude(field, false);

	public IQBMongoSelectBuilder<TDoc, TDto> Optional(Expression<Func<TDto, object?>> field)
		=> AddExclude(field, true);

	public IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Select);
	public IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string alias, string tableName)
		=> AddContainer(typeof(TDoc), alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Select);

	public IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.LeftJoin);
	public IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.LeftJoin);

	public IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Join);
	public IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.Join);

	public IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.CrossJoin);
	public IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, BuilderContainerTypes.Table, BuilderContainerOperations.CrossJoin);

	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, null, field, refAlias, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, alias, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.IsConnect | BuilderConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);

	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, null, field, refAlias, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, alias, field, null, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(BuilderConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(BuilderConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(string alias, Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBMongoSelectBuilder<TDoc, TDto> Condition(string alias, Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(BuilderConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);

	public IQBMongoSelectBuilder<TDoc, TDto> Begin()
	{
		_parentheses++;
		_sumParentheses++;
		if (_autoOpenedParentheses > 0) _autoOpenedParentheses++;
		return this;
	}
	public IQBMongoSelectBuilder<TDoc, TDto> End()
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		var lastIndex = Conditions.Count - 1;
		if (lastIndex < 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		var lastCond = Conditions[lastIndex];
		if (lastCond.Parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		int decrement = _autoOpenedParentheses > 0 && --_autoOpenedParentheses == 0 ? 2 : 1;
		if (_sumParentheses < decrement)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		Conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - decrement };
		_sumParentheses -= decrement;

		return this;
	}

	public IQBMongoSelectBuilder<TDoc, TDto> And()
	{
		if (_isByOr != null || _parentheses > 0 || Conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		_isByOr = false;

		return this;
	}
	public IQBMongoSelectBuilder<TDoc, TDto> Or()
	{
		if (_isByOr != null || _parentheses > 0 || Conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if (_autoOpenedParentheses == 1)
		{
			if (_sumParentheses < 1)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}

			var lastIndex = Conditions.Count - 1;
			var lastCond = Conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
			}

			Conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - 1 };
			_sumParentheses--;
			_autoOpenedParentheses = 0;
		}

		_isByOr = true;

		return this;
	}

	private void CompleteAutoOpenedParentheses()
	{
		if (_autoOpenedParentheses > 0)
		{
			if (_sumParentheses < 1)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}
			if (_autoOpenedParentheses > 1)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}
			
			var lastIndex = Conditions.Count - 1;
			var lastCond = Conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
			}

			Conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - 1 };
			_sumParentheses--;
			_autoOpenedParentheses = 0;
		}
	}

	/// <summary>
	/// Trim parentheses around specified expression
	/// </summary>
	/// <param name="conds">Expression</param>
	public static void TrimParentheses(List<BuilderCondition> conds)
	{
		if (conds.Count == 0) return;
		if (conds.Count == 1)
		{
			System.Diagnostics.Debug.Assert(conds[0].Parentheses == 0);
			return;
		}

		var first = conds[0].Parentheses;
		System.Diagnostics.Debug.Assert(first >= 0);
		if (first <= 0) return;

		var last = conds[conds.Count - 1].Parentheses;
		System.Diagnostics.Debug.Assert(last <= 0);
		if (last >= 0) return;

		int sum = 0, min = 0;
		for (int i = 1; i < conds.Count - 1; i++)
		{
			sum += conds[i].Parentheses;
			System.Diagnostics.Debug.Assert(sum + first >= 0);
			if (sum + first <= 0) return;
			if (sum < min) min = sum;
		}
		// (((  (a || b) && c                        )))		4 -1(-1) -3
		// (    (a || b) && (b || c)                 )			2  0(-1) -2
		//      (a || b) && (b || c)
		//      ((a || b) && (b || c)) || (d && e)
		// (    ((a || b) && (b || c)) || (d && e)   )			3 -1(-2) -2

		System.Diagnostics.Debug.Assert(first + sum + last == 0);

		conds[0] = conds[0] with { Parentheses = -min };
		conds[conds.Count - 1] = conds[conds.Count - 1] with { Parentheses = last + (first + min) };

		System.Diagnostics.Debug.Assert(conds.Sum(x => x.Parentheses) == 0);
	}

	/// <summary>
	/// Calc parentheses for specified expression
	/// </summary>
	/// <param name="conds">Expression</param>
	/// <returns>
	/// -1 when the expression is not completed<para />
	/// 0 when the expression is not parenthesized<para />
	/// Number of parentheses around<para />
	/// </returns>
	public static int GetParenthesizedCount(List<BuilderCondition> conds)
	{
		if (conds.Count == 0)
		{
			return 0;
		}
		if (conds.Count == 1)
		{
			return conds[0].Parentheses == 0 ? 0 : -1;
		}

		var first = conds[0].Parentheses;
		if (first <= 0)
		{
			return conds.Sum(x => x.Parentheses) == 0 ? 0 : -1;
		}

		var last = conds[conds.Count - 1].Parentheses;
		if (last >= 0)
		{
			return conds.Sum(x => x.Parentheses) == 0 ? 0 : -1;
		}

		int sum = 0, min = 0;
		for (int i = 1; i < conds.Count - 1; i++)
		{
			sum += conds[i].Parentheses;
			
			if (sum + first <= 0)
			{
				for (; i < conds.Count - 1; i++)
				{
					sum += conds[i].Parentheses;
				}
				sum += first;
				sum += last;
				return sum == 0 ? 0 : -1;
			}

			if (sum < min) min = sum;
		}

		sum += first;
		sum += last;
		return sum == 0 ? first + min : -1;
	}
}