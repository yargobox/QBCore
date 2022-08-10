using System.Linq.Expressions;
using QBCore.Extensions.Linq;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBSelectBuilder<TDoc, TDto> : IQBSelectBuilder<TDoc, TDto>, IQBMongoSelectBuilder<TDoc, TDto>, ICloneable
{
	public QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;
	public bool IsNormalized { get; private set; }

	public IReadOnlyList<QBContainer> Containers => _containers;
	public IReadOnlyList<QBCondition> Connects => _connects ?? BuilderEmptyLists.Conditions;
	public IReadOnlyList<QBCondition> Conditions => _conditions ?? BuilderEmptyLists.Conditions;
	public IReadOnlyList<QBField> Fields => _fields ?? BuilderEmptyLists.Fields;
	public IReadOnlyList<QBParameter> Parameters => _parameters ?? BuilderEmptyLists.Parameters;
	public IReadOnlyList<QBSortOrder> SortOrders => _sortOrders ?? (_sortOrders = new List<QBSortOrder>(3));
	public IReadOnlyList<QBAggregation> Aggregations => _aggregations ?? (_aggregations = new List<QBAggregation>(3));

	private readonly List<QBContainer> _containers = new List<QBContainer>(3);
	private List<QBField>? _fields;
	private List<QBCondition>? _connects;
	private List<QBCondition>? _conditions;
	private List<QBParameter>? _parameters;
	private List<QBSortOrder>? _sortOrders;
	private List<QBAggregation>? _aggregations;

	private int _parentheses;
	private int _sumParentheses;
	private bool? _isByOr;
	private int _autoOpenedParentheses;

	public Expression<Func<TDoc, object?>>? IdField
	{
		get => _idField;
		set
		{
			if (_idField != null)
				throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': option '{nameof(IdField)}' is already set.");
			_idField = value;
		}
	}
	public Expression<Func<TDto, object?>>? DateDeleteField
	{
		get => _dateDeleteSelField;
		set
		{
			if (_dateDeleteSelField != null)
				throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': option '{nameof(IQBSelectBuilder<TDoc, TDto>.DateDeleteField)}' is already set.");
			_dateDeleteSelField = value;
		}
	}
	Expression<Func<TDto, object?>>? IQBMongoSelectBuilder<TDoc, TDto>.DateDeleteField
	{
		get => this.DateDeleteField;
		set => this.DateDeleteField = value;
	}

	private Expression<Func<TDoc, object?>>? _idField;
	private Expression<Func<TDto, object?>>? _dateDeleteSelField;

	public QBSelectBuilder()
	{
		_idField =
			typeof(TDoc).GetProperties().Where(x => x.IsDefined(typeof(DeIdAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>()
			?? typeof(TDoc).GetFields().Where(x => x.IsDefined(typeof(DeIdAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>();

		_dateDeleteSelField =
			typeof(TDto).GetProperties().Where(x => x.IsDefined(typeof(DeDeletedAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDto>()
			?? typeof(TDto).GetFields().Where(x => x.IsDefined(typeof(DeDeletedAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDto>();
	}
	public QBSelectBuilder(QBSelectBuilder<TDoc, TDto> other)
	{
		if (!(IsNormalized = other.IsNormalized))
		{
			other.Normalize();
		}

		_containers = new List<QBContainer>(other._containers);
		if (other._fields != null) _fields = new List<QBField>(other._fields);
		if (other._parameters != null) _parameters = new List<QBParameter>(other._parameters);
		if (other._connects != null) _connects = new List<QBCondition>(other._connects);
		if (other._conditions != null) _conditions = new List<QBCondition>(other._conditions);
		if (other._sortOrders != null) _sortOrders = new List<QBSortOrder>(other._sortOrders);
		if (other._aggregations != null) _aggregations = new List<QBAggregation>(other._aggregations);
		_idField = other._idField;
		_dateDeleteSelField = other._dateDeleteSelField;
	}
	public object Clone() => new QBSelectBuilder<TDoc, TDto>(this);

	public void Normalize()
	{
		if (IsNormalized) return;
		NormalizeInternal();
		IsNormalized = true;
	}

	private void NormalizeInternal()
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

		var containers = _containers ?? BuilderEmptyLists.Containers;
		var connects = _connects ?? BuilderEmptyLists.Conditions;
		var conditions = _conditions ?? BuilderEmptyLists.Conditions;

		var rootIndex = containers.FindIndex(x => x.ContainerOperation == ContainerOperations.Select);
		if (rootIndex < 0)
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if (containers.Any(x => x.ContainerType != ContainerTypes.Table && x.ContainerType != ContainerTypes.View))
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		QBContainer? top, bottom, temp;

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
		if (connects.Any(x => x.Alias == top.Alias))
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

			if (temp.ContainerOperation == ContainerOperations.LeftJoin || temp.ContainerOperation == ContainerOperations.Join)
			{
				if (!connects.Any(x => x.IsConnectOnField && x.Alias == temp.Alias))
				{
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': JOIN (LEFT JOIN) has to have at least one connect condition on a field.");
				}
			}
			else if (temp.ContainerOperation == ContainerOperations.CrossJoin)
			{
				if (connects.Any(x => x.IsConnectOnField && x.Alias == temp.Alias))
				{
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': CROSS JOIN cannot have connection conditions on fields.");
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
				.Where(x => x.IsOnField && x.Alias == top.Alias)
				.Select(x => x.RefAlias))
			{
				for (int j = i + 1; j < containers.Count; j++)
				{
					bottom = containers[j];
					if (bottom.Alias == topDependOn)
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

	private QBSelectBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string alias,
		string dbSideName,
		ContainerTypes containerType,
		ContainerOperations containerOperation)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if ((containerOperation & ContainerOperations.MainMask) != ContainerOperations.None &&
			_containers.Any(x => x.ContainerOperation.HasFlag(ContainerOperations.MainMask)))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': another initial container has already been added before.");
		}
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
		}
		if (_containers.Any(x => x.Alias == alias))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TDto).ToPretty()}': initial container '{alias}' has already been added before.");
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

	private QBSelectBuilder<TDoc, TDto> AddCondition<TLocal, TRef>(
		QBConditionFlags flags,
		string? alias,
		Expression<Func<TLocal, object?>> field,
		string? refAlias,
		Expression<Func<TRef, object?>>? refField,
		object? constValue,
		string? paramName,
		FO operation)
	{
		var trueContainerType = typeof(TLocal) == typeof(TDto) ? typeof(TDoc) : typeof(TLocal);
		if (alias == null)
		{
			alias = _containers.Single(x => x.DocumentType == trueContainerType).Alias;
		}
		else if (!_containers.Any(x => x.Alias == alias && x.DocumentType == trueContainerType))
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': referenced container '{alias}' of  document '{trueContainerType.ToPretty()}' has not been added yet.");
		}

		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}

		var onWhat = flags & (QBConditionFlags.OnField | QBConditionFlags.OnConst | QBConditionFlags.OnParam);
		if (onWhat == QBConditionFlags.OnField)
		{
			if (refAlias == null)
			{
				refAlias = _containers.Single(x => x.DocumentType == typeof(TRef)).Alias;
			}
			else if (!_containers.Any(x => x.Alias == refAlias && x.DocumentType == typeof(TRef)))
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': referenced container '{refAlias}' of  document '{typeof(TRef).ToPretty()}' has not been added yet.");
			}

			if (refField == null)
			{
				throw new ArgumentNullException(nameof(refField));
			}
		}
		else if (onWhat == QBConditionFlags.OnParam)
		{
			if (string.IsNullOrWhiteSpace(paramName))
			{
				throw new ArgumentNullException(nameof(paramName));
			}
		}

		if (flags.HasFlag(QBConditionFlags.IsConnect))
		{
			if (_connects == null)
			{
				_connects = new List<QBCondition>(4);
			}

			IsNormalized = false;
			_connects.Add(new QBCondition(
				flags: flags,
				alias: alias,
				field: new MongoFieldPath(field, false),
				refAlias: refAlias,
				refField: refField != null ? new MongoFieldPath(refField, true) : null,
				value: onWhat == QBConditionFlags.OnParam ? paramName : onWhat == QBConditionFlags.OnConst ? constValue : null,
				operation: operation
			));
		}
		else
		{
			var isByOr = _isByOr.HasValue && _isByOr.Value;

			if (_conditions == null)
			{
				_conditions = new List<QBCondition>(4);
			}

			// Automatically add parentheses if needed
			// Skip conditions like these, they don't need it
			// a		=> a OR ?
			// (a		=> (a OR ?
			// .. (a	=> .. (a OR ?
			//
			if (_conditions.Count > 1 && _conditions[_conditions.Count - 1].Parentheses <= 0)
			{
				QBCondition cond;
				int startIndex, lastIndex = _conditions.Count - 1;

				if (isByOr)
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
						cond = _conditions[i];

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
						cond = _conditions[startIndex];
						_conditions[startIndex] = cond with { Parentheses = cond.Parentheses + 1 };
						cond = _conditions[lastIndex];
						_conditions[lastIndex] = cond with { Parentheses = cond.Parentheses - 1 };
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
						cond = _conditions[i];

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
						cond = _conditions[startIndex];

						if (_autoOpenedParentheses != 0 || cond.Parentheses < 0)
						{
							throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
						}

						_conditions[startIndex] = cond with { Parentheses = cond.Parentheses + 1 };

						_autoOpenedParentheses = _conditions.Skip(startIndex).Sum(x => x.Parentheses) + _parentheses;
						_sumParentheses++;
					}
				}
			}

			if (isByOr)
			{
				flags |= QBConditionFlags.IsByOr;
			}

			IsNormalized = false;
			_conditions.Add(new QBCondition(
				flags: flags,
				alias: alias,
				field: new MongoFieldPath(field, false),
				refAlias: refAlias,
				refField: refField != null ? new MongoFieldPath(refField, true) : null,
				value: onWhat == QBConditionFlags.OnParam ? paramName : onWhat == QBConditionFlags.OnConst ? constValue : null,
				operation: operation
			) { Parentheses = _parentheses });

			_isByOr = null;
			_parentheses = 0;
		}

		if (flags.HasFlag(QBConditionFlags.OnParam))
		{
			var fieldPath = (flags.HasFlag(QBConditionFlags.IsConnect)
					? _connects![_connects.Count - 1]
					: _conditions![_conditions.Count - 1])
				.Field;

			AddParameter(paramName!, fieldPath.FieldType, fieldPath.IsNullable, System.Data.ParameterDirection.Input);
		}

		return this;
	}

	private QBSelectBuilder<TDoc, TDto> AddParameter(string name, Type underlyingType, bool isNullable, System.Data.ParameterDirection direction)
	{
		if (_parameters == null)
		{
			_parameters = new List<QBParameter>(8);
		}

		var param = _parameters.FirstOrDefault(x => x.Name == name);
		if (param != null)
		{
			if (param.UnderlyingType != underlyingType || param.IsNullable != isNullable || param.Direction != direction)
			{
				throw new InvalidOperationException($"Incorrect parameter definition of select query builder '{typeof(TDto).ToPretty()}': parameter '{name}' has already been added before with different properties");
			}
		}
		else
		{
			IsNormalized = false;
			_parameters.Add(new QBParameter(name, underlyingType, isNullable, direction));
		}

		return this;
	}

	private QBSelectBuilder<TDoc, TDto> AddInclude<TRef>(Expression<Func<TDto, object?>> field, string? refAlias, Expression<Func<TRef, object?>> refField)
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

		if (refAlias == null)
		{
			refAlias = _containers.Single(x => x.DocumentType == typeof(TRef)).Alias;
		}
		else if (!_containers.Any(x => x.Alias == refAlias && x.DocumentType == typeof(TRef)))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TDto).ToPretty()}': referenced container '{refAlias}' of  document '{typeof(TRef).ToPretty()}' has not been added yet.");
		}


		if (_fields == null)
		{
			_fields = new List<QBField>(8);
		}

		var fieldPath = new MongoFieldPath(field, false);
		if (_fields.Any(x => x.Field.FullName == fieldPath.FullName))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TDto).ToPretty()}': field {fieldPath.FullName} has already been included/excluded before.");
		}

		var refFieldPath = new MongoFieldPath(refField, true);

		IsNormalized = false;
		_fields.Add(new QBField(
			Field: fieldPath,
			RefAlias: refAlias,
			RefField: refFieldPath,
			OptionalExclusion: false
		));

		return this;
	}
	
	private QBSelectBuilder<TDoc, TDto> AddExclude(Expression<Func<TDto, object?>> field, bool optional)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		
		if (_fields == null)
		{
			_fields = new List<QBField>(8);
		}

		var fieldPath = new MongoFieldPath(field, false);
		if (_fields.Any(x => x.Field.FullName == fieldPath.FullName))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TDto).ToPretty()}': field '{fieldPath.FullName}' has already been included/excluded before.");
		}

		IsNormalized = false;
		_fields.Add(new QBField(
			Field: fieldPath,
			RefAlias: null,
			RefField: null,
			OptionalExclusion: optional
		));

		return this;
	}

	public IQBSelectBuilder<TDoc, TDto> SelectFrom(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Select);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.SelectFrom(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Select);
	public IQBSelectBuilder<TDoc, TDto> SelectFrom(string alias, string tableName)
		=> AddContainer(typeof(TDoc), alias, tableName, ContainerTypes.Table, ContainerOperations.Select);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.SelectFrom(string alias, string tableName)
		=> AddContainer(typeof(TDoc), alias, tableName, ContainerTypes.Table, ContainerOperations.Select);

	public IQBSelectBuilder<TDoc, TDto> LeftJoin<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, ContainerTypes.Table, ContainerOperations.LeftJoin);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.LeftJoin<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, ContainerTypes.Table, ContainerOperations.LeftJoin);
	public IQBSelectBuilder<TDoc, TDto> LeftJoin<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.LeftJoin);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.LeftJoin<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.LeftJoin);

	public IQBSelectBuilder<TDoc, TDto> Join<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, ContainerTypes.Table, ContainerOperations.Join);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Join<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, ContainerTypes.Table, ContainerOperations.Join);
	public IQBSelectBuilder<TDoc, TDto> Join<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.Join);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Join<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.Join);

	public IQBSelectBuilder<TDoc, TDto> CrossJoin<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, ContainerTypes.Table, ContainerOperations.CrossJoin);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.CrossJoin<TRef>(string tableName)
		=> AddContainer(typeof(TRef), tableName, tableName, ContainerTypes.Table, ContainerOperations.CrossJoin);
	public IQBSelectBuilder<TDoc, TDto> CrossJoin<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.CrossJoin);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.CrossJoin<TRef>(string alias, string tableName)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.CrossJoin);

	public IQBSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBSelectBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);

	public IQBSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public IQBSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);

	public IQBSelectBuilder<TDoc, TDto> Begin()
	{
		((IQBMongoSelectBuilder<TDoc, TDto>)this).Begin();
		return this;
	}
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Begin()
	{
		IsNormalized = false;
		_parentheses++;
		_sumParentheses++;
		if (_autoOpenedParentheses > 0) _autoOpenedParentheses++;
		return this;
	}
	public IQBSelectBuilder<TDoc, TDto> End()
	{
		((IQBMongoSelectBuilder<TDoc, TDto>)this).End();
		return this;
	}
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.End()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		var lastIndex = _conditions.Count - 1;
		var lastCond = _conditions[lastIndex];
		if (lastCond.Parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		int decrement = _autoOpenedParentheses > 0 && --_autoOpenedParentheses == 0 ? 2 : 1;
		if (_sumParentheses < decrement)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		IsNormalized = false;
		_conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - decrement };
		_sumParentheses -= decrement;

		return this;
	}

	public IQBSelectBuilder<TDoc, TDto> And()
	{
		((IQBMongoSelectBuilder<TDoc, TDto>)this).And();
		return this;
	}
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.And()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		IsNormalized = false;
		_isByOr = false;

		return this;
	}
	public IQBSelectBuilder<TDoc, TDto> Or()
	{
		((IQBMongoSelectBuilder<TDoc, TDto>)this).Or();
		return this;
	}
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Or()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
		}

		if (_autoOpenedParentheses == 1)
		{
			if (_sumParentheses < 1)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}

			var lastIndex = _conditions.Count - 1;
			var lastCond = _conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
			}

			IsNormalized = false;
			_conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - 1 };
			_sumParentheses--;
			_autoOpenedParentheses = 0;
			_isByOr = true;
		}
		else
		{
			IsNormalized = false;
			_isByOr = true;
		}

		return this;
	}

	public IQBSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, null, refField);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, null, refField);
	public IQBSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, refAlias, refField);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Include<TRef>(Expression<Func<TDto, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, refAlias, refField);

	public IQBSelectBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field)
		=> AddExclude(field, false);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Exclude(Expression<Func<TDto, object?>> field)
		=> AddExclude(field, false);

	public IQBSelectBuilder<TDoc, TDto> Optional(Expression<Func<TDto, object?>> field)
		=> AddExclude(field, true);
	IQBMongoSelectBuilder<TDoc, TDto> IQBMongoSelectBuilder<TDoc, TDto>.Optional(Expression<Func<TDto, object?>> field)
		=> AddExclude(field, true);

	private void CompleteAutoOpenedParentheses()
	{
		if (_autoOpenedParentheses > 0)
		{
			if (_sumParentheses < 1 || _autoOpenedParentheses > 1 || _conditions == null || _conditions.Count == 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}'.");
			}
			
			var lastIndex = _conditions.Count - 1;
			var lastCond = _conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
			}

			_conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - 1 };
			_sumParentheses--;
			_autoOpenedParentheses = 0;
		}
	}

	/// <summary>
	/// Trim parentheses around specified expression
	/// </summary>
	/// <param name="conds">Expression</param>
	public static void TrimParentheses(List<QBCondition> conds)
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
}

internal static class BuilderEmptyLists
{
	public static readonly List<QBContainer> Containers = new List<QBContainer>(0);
	public static readonly List<QBCondition> Conditions = new List<QBCondition>(0);
	public static readonly List<QBField> Fields = new List<QBField>(0);
	public static readonly List<QBParameter> Parameters = new List<QBParameter>(0);
	public static readonly List<QBSortOrder> SortOrders = new List<QBSortOrder>(0);
	public static readonly List<QBAggregation> Aggregations = new List<QBAggregation>(0);
}