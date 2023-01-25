using System.Linq.Expressions;
using QBCore.Extensions.Linq;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class SelectQBBuilder<TDoc, TSelect> : QBBuilder<TDoc, TSelect>, IMongoSelectQBBuilder<TDoc, TSelect>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;
	public override IDataLayerInfo DataLayer => MongoDataLayer.Default;
	public override IReadOnlyList<QBContainer> Containers => _containers ?? EmptyLists.Containers;
	public override IReadOnlyList<QBField> Fields => _fields ?? EmptyLists.Fields;
	public override IReadOnlyList<QBCondition> Connects => _connects ?? EmptyLists.Conditions;
	public override IReadOnlyList<QBCondition> Conditions => _conditions ?? EmptyLists.Conditions;
	public override IReadOnlyList<QBParameter> Parameters => _parameters ?? EmptyLists.Parameters;
	public override IReadOnlyList<QBSortOrder> SortOrders => _sortOrders ?? EmptyLists.SortOrders;
	public override IReadOnlyList<QBAggregation> Aggregations => _aggregations ?? EmptyLists.Aggregations;

	private List<QBContainer>? _containers;
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

	public SelectQBBuilder() { }
	public SelectQBBuilder(SelectQBBuilder<TDoc, TSelect> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(other._containers);
		if (other._fields != null) _fields = new List<QBField>(other._fields);
		if (other._parameters != null) _parameters = new List<QBParameter>(other._parameters);
		if (other._connects != null) _connects = new List<QBCondition>(other._connects);
		if (other._conditions != null) _conditions = new List<QBCondition>(other._conditions);
		if (other._sortOrders != null) _sortOrders = new List<QBSortOrder>(other._sortOrders);
		if (other._aggregations != null) _aggregations = new List<QBAggregation>(other._aggregations);
	}
	public SelectQBBuilder(IQBBuilder other)
	{
		if (other is null) throw new ArgumentNullException(nameof(other));

		if (other.DocType != typeof(TDoc))
		{
			throw new InvalidOperationException($"Could not make select query builder '{typeof(TDoc).ToPretty()}, {typeof(TSelect).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		var top = other.Containers.FirstOrDefault();
		if (top?.DocumentType != typeof(TDoc) || top.ContainerType != ContainerTypes.Table)
		{
			throw new InvalidOperationException($"Could not make select query builder '{typeof(TDoc).ToPretty()}, {typeof(TSelect).ToPretty()}' from '{other.DocType.ToPretty()}, {other.DtoType.ToPretty()}'.");
		}

		AutoBuild(top.DBSideName);
	}

	protected override void OnNormalize()
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		CompleteAutoOpenedParentheses();
		if (_sumParentheses != 0 || _autoOpenedParentheses != 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		if (_containers == null || _containers.Count == 0)
		{
			return;
		}

		var containers = _containers ?? EmptyLists.Containers;
		var connects = _connects ?? EmptyLists.Conditions;

		var rootIndex = containers.FindIndex(x => x.ContainerOperation == ContainerOperations.Select);
		if (rootIndex < 0)
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		if (containers.Any(x => x.ContainerType != ContainerTypes.Table && x.ContainerType != ContainerTypes.View))
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TSelect).ToPretty()}'.");
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
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}'.");
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
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': JOIN (LEFT JOIN) has to have at least one connect condition on a field.");
				}
			}
			else if (temp.ContainerOperation == ContainerOperations.CrossJoin)
			{
				if (connects.Any(x => x.IsConnectOnField && x.Alias == temp.Alias))
				{
					throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': CROSS JOIN cannot have connection conditions on fields.");
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
							throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}'.");
						}

						containers.Swap(i, j);
						goto L_RESCAN;
					}
				}
			}
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();

		if (Containers.Count == 0)
		{
			throw new InvalidOperationException($"Incompatible configuration of select query builder '{typeof(TSelect).ToPretty()}'.");
		}
	}

	private SelectQBBuilder<TDoc, TSelect> AddContainer(
		Type documentType,
		string? alias,
		string? dbSideName,
		ContainerTypes containerType,
		ContainerOperations containerOperation)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		dbSideName ??= MongoDataLayer.Default.GetDefaultDBSideContainerName(documentType);
		alias ??= dbSideName;

		if ((containerOperation & ContainerOperations.MainMask) != ContainerOperations.None && Containers.Any(x => x.ContainerOperation.HasFlag(ContainerOperations.MainMask)))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': another initial container has already been added before.");
		}
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
		}
		if (Containers.Any(x => x.Alias == alias))
		{
			throw new InvalidOperationException($"Incorrect definition of select query builder '{typeof(TSelect).ToPretty()}': initial container '{alias}' has already been added before.");
		}

		if (_containers == null)
		{
			_containers = new List<QBContainer>(3);
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

	private SelectQBBuilder<TDoc, TSelect> AddCondition<TLocal, TRef>(
		QBConditionFlags flags,
		string? alias,
		DEPathDefinition<TLocal> field,
		string? refAlias,
		DEPathDefinition<TRef>? refField,
		object? constValue,
		string? paramName,
		FO operation)
	{
		return AddCondition<TLocal, TRef>(
			flags,
			alias,
			field.ToDataEntryPath(MongoDataLayer.Default),
			refAlias,
			refField?.ToDataEntryPath(MongoDataLayer.Default),
			constValue,
			paramName,
			operation
		);
	}
	private SelectQBBuilder<TDoc, TSelect> AddCondition<TLocal, TRef>(
		QBConditionFlags flags,
		string? alias,
		Expression<Func<TLocal, object?>> field,
		string? refAlias,
		Expression<Func<TRef, object?>>? refField,
		object? constValue,
		string? paramName,
		FO operation)
	{
		return AddCondition<TLocal, TRef>(
			flags,
			alias,
			new DEPath(field, false, MongoDataLayer.Default),
			refAlias,
			refField != null ? new DEPath(refField, true, MongoDataLayer.Default) : null,
			constValue,
			paramName,
			operation
		);
	}
	private SelectQBBuilder<TDoc, TSelect> AddCondition<TLocal, TRef>(
		QBConditionFlags flags,
		string? alias,
		DEPath field,
		string? refAlias,
		DEPath? refField,
		object? constValue,
		string? paramName,
		FO operation)
	{
		var trueContainerType = typeof(TLocal) == typeof(TSelect) ? typeof(TDoc) : typeof(TLocal);
		if (alias == null)
		{
			alias = Containers.Single(x => x.DocumentType == trueContainerType).Alias;
		}
		else if (!Containers.Any(x => x.Alias == alias && x.DocumentType == trueContainerType))
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}': referenced container '{alias}' of  document '{trueContainerType.ToPretty()}' has not been added yet.");
		}

		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		if (field.Count == 0 || field.DocumentType != typeof(TLocal))
		{
			throw new ArgumentException(nameof(field));
		}

		if (refField?.Count > 0 && refField.DocumentType != typeof(TRef))
		{
			throw new ArgumentException(nameof(refField));
		}

		var onWhat = flags & (QBConditionFlags.OnField | QBConditionFlags.OnConst | QBConditionFlags.OnParam);
		if (onWhat == QBConditionFlags.OnField)
		{
			if (refAlias == null)
			{
				refAlias = Containers.Single(x => x.DocumentType == typeof(TRef)).Alias;
			}
			else if (!Containers.Any(x => x.Alias == refAlias && x.DocumentType == typeof(TRef)))
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}': referenced container '{refAlias}' of  document '{typeof(TRef).ToPretty()}' has not been added yet.");
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
				field: field,
				refAlias: refAlias,
				refField: refField,
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
							throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}': messed up with automatically opening parentheses.");
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
				field: field,
				refAlias: refAlias,
				refField: refField,
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

			AddParameter(paramName!, fieldPath.DataEntryType, fieldPath.IsNullable, System.Data.ParameterDirection.Input);
		}

		return this;
	}

	private SelectQBBuilder<TDoc, TSelect> AddParameter(string name, Type underlyingType, bool isNullable, System.Data.ParameterDirection direction)
	{
		if (_parameters == null)
		{
			_parameters = new List<QBParameter>(8);
		}

		var param = _parameters.FirstOrDefault(x => x.ParameterName == name);
		if (param != null)
		{
			if (param.ClrType != underlyingType || param.IsNullable != isNullable || param.Direction != direction)
			{
				throw new InvalidOperationException($"Incorrect parameter definition of select query builder '{typeof(TSelect).ToPretty()}': parameter '{name}' has already been added before with different properties");
			}
		}
		else
		{
			IsNormalized = false;
			_parameters.Add(new QBParameter(name, underlyingType, isNullable, direction));
		}

		return this;
	}

	private SelectQBBuilder<TDoc, TSelect> AddInclude<TRef>(DEPathDefinition<TSelect> field, string? refAlias, DEPathDefinition<TRef>? refField)
	{
		return AddInclude<TRef>(
			field.ToDataEntryPath(MongoDataLayer.Default),
			refAlias,
			refField?.ToDataEntryPath(MongoDataLayer.Default)
		);
	}
	private SelectQBBuilder<TDoc, TSelect> AddInclude<TRef>(Expression<Func<TSelect, object?>> field, string? refAlias, Expression<Func<TRef, object?>> refField)
	{
		return AddInclude<TRef>(
			new DEPath(field, false, MongoDataLayer.Default),
			refAlias, refField != null ? new DEPath(refField, true, MongoDataLayer.Default) : null
		);
	}
	private SelectQBBuilder<TDoc, TSelect> AddInclude<TRef>(DEPath field, string? refAlias, DEPath? refField)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		if (field.Count == 0 || field.DocumentType != typeof(TSelect))
		{
			throw new ArgumentException(nameof(field));
		}
		if (refField == null)
		{
			throw new ArgumentNullException(nameof(refField));
		}
		if (refField.Count > 0 && refField.DocumentType != typeof(TRef))
		{
			throw new ArgumentException(nameof(refField));
		}

		if (refAlias == null)
		{
			refAlias = Containers.Single(x => x.DocumentType == typeof(TRef)).Alias;
		}
		else if (!Containers.Any(x => x.Alias == refAlias && x.DocumentType == typeof(TRef)))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TSelect).ToPretty()}': referenced container '{refAlias}' of  document '{typeof(TRef).ToPretty()}' has not been added yet.");
		}


		if (_fields == null)
		{
			_fields = new List<QBField>(8);
		}

		if (_fields.Any(x => x.Field.Path == field.Path))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TSelect).ToPretty()}': field {field.Path} has already been included/excluded before.");
		}

		IsNormalized = false;
		_fields.Add(new QBField(
			Field: field,
			RefAlias: refAlias,
			RefField: refField,
			IsOptional: false,
			IsExcluded: false
		));

		return this;
	}
	
	private SelectQBBuilder<TDoc, TSelect> AddExclude(DEPathDefinition<TSelect> field, bool optional)
	{
		return AddExclude(field.ToDataEntryPath(MongoDataLayer.Default), optional);
	}
	private SelectQBBuilder<TDoc, TSelect> AddExclude(Expression<Func<TSelect, object?>> field, bool optional)
	{
		return AddExclude(new DEPath(field, false, MongoDataLayer.Default), optional);
	}
	private SelectQBBuilder<TDoc, TSelect> AddExclude(DEPath field, bool optional)
	{
		if (_isByOr != null || _parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		
		if (_fields == null)
		{
			_fields = new List<QBField>(8);
		}

		if (_fields.Any(x => x.Field.Path == field.Path))
		{
			throw new InvalidOperationException($"Incorrect field definition of select query builder '{typeof(TSelect).ToPretty()}': field '{field.Path}' has already been included/excluded before.");
		}

		IsNormalized = false;
		_fields.Add(new QBField(
			Field: field,
			RefAlias: null,
			RefField: null,
			IsOptional: optional,
			IsExcluded: optional
		));

		return this;
	}

	private SelectQBBuilder<TDoc, TSelect> AddSortBy<TLocal>(string? alias, DEPathDefinition<TLocal> field, SO sortOrder)
	{
		return AddSortBy<TLocal>(alias, field.ToDataEntryPath(MongoDataLayer.Default), sortOrder);
	}
	private SelectQBBuilder<TDoc, TSelect> AddSortBy<TLocal>(string? alias, Expression<Func<TLocal, object?>> field, SO sortOrder)
	{
		return AddSortBy<TLocal>(alias, new DEPath(field, false, MongoDataLayer.Default), sortOrder);
	}
	private SelectQBBuilder<TDoc, TSelect> AddSortBy<TLocal>(string? alias, DEPath field, SO sortOrder)
	{
		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		if (string.IsNullOrEmpty(alias))
		{
			if (typeof(TLocal) != typeof(TSelect))
			{
				throw new ArgumentNullException(nameof(alias));
			}

			alias = string.Empty;
		}
		else if (!Containers.Any(x => x.Alias == alias && x.DocumentType == typeof(TLocal)))
		{
			throw new InvalidOperationException($"Incorrect sort order definition of select query builder '{typeof(TSelect).ToPretty()}': referenced container '{alias}' of  document '{typeof(TLocal).ToPretty()}' has not been added yet.");
		}
		
		if (_sortOrders == null)
		{
			_sortOrders = new List<QBSortOrder>(4);
		}

		if (_sortOrders.Any(x => x.Alias == alias && x.Field.Path == field.Path))
		{
			throw new InvalidOperationException($"Incorrect sort order definition of select query builder '{typeof(TSelect).ToPretty()}': field '{field.Path}' already has a sort order.");
		}

		IsNormalized = false;
		_sortOrders.Add(new QBSortOrder(alias, field, sortOrder));

		return this;
	}

	public override QBBuilder<TDoc, TSelect> AutoBuild(string? tableName = null)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Select query builder '{typeof(TSelect).ToPretty()}' has already been initialized.");
		}

		Select(tableName);

		return this;
	}
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.AutoBuild(string? tableName)
	{
		AutoBuild(tableName);
		return this;
	}

	public override QBBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null)
		=> AddContainer(typeof(TDoc), alias, tableName, ContainerTypes.Table, ContainerOperations.Select);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Select(string? tableName, string? alias)
		=> AddContainer(typeof(TDoc), alias, tableName, ContainerTypes.Table, ContainerOperations.Select);

	public override QBBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.LeftJoin);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.LeftJoin<TRef>(string? tableName, string? alias)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.LeftJoin);

	public override QBBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.Join);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Join<TRef>(string? tableName, string? alias)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.Join);

	public override QBBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.CrossJoin);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.CrossJoin<TRef>(string? tableName, string? alias)
		=> AddContainer(typeof(TRef), alias, tableName, ContainerTypes.Table, ContainerOperations.CrossJoin);

	public override QBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(DEPathDefinition<TLocal> field, DEPathDefinition<TRef> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, DEPathDefinition<TLocal> field, string refAlias, DEPathDefinition<TRef> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.IsConnect | QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(DEPathDefinition<TLocal> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, DEPathDefinition<TLocal> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(DEPathDefinition<TLocal> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, DEPathDefinition<TLocal> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.IsConnect | QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);

	public override QBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(DEPathDefinition<TLocal> field, DEPathDefinition<TRef> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, null, field, null, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, DEPathDefinition<TLocal> field, string refAlias, DEPathDefinition<TRef> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation)
		=> AddCondition<TLocal, TRef>(QBConditionFlags.OnField, alias, field, refAlias, refField, null, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(DEPathDefinition<TLocal> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, DEPathDefinition<TLocal> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnConst, alias, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(DEPathDefinition<TLocal> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, DEPathDefinition<TLocal> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName)
		=> AddCondition<TLocal, NotSupported>(QBConditionFlags.OnParam, alias, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition(DEPathDefinition<TDoc> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnConst, null, field, null, null, constValue, null, operation);
	public override QBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	public override QBBuilder<TDoc, TSelect> Condition(DEPathDefinition<TDoc> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition<TDoc, NotSupported>(QBConditionFlags.OnParam, null, field, null, null, null, paramName, operation);

	public override QBBuilder<TDoc, TSelect> Begin()
	{
		((IMongoSelectQBBuilder<TDoc, TSelect>)this).Begin();
		return this;
	}
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Begin()
	{
		IsNormalized = false;
		_parentheses++;
		_sumParentheses++;
		if (_autoOpenedParentheses > 0) _autoOpenedParentheses++;
		return this;
	}
	public override QBBuilder<TDoc, TSelect> End()
	{
		((IMongoSelectQBBuilder<TDoc, TSelect>)this).End();
		return this;
	}
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.End()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		var lastIndex = _conditions.Count - 1;
		var lastCond = _conditions[lastIndex];
		if (lastCond.Parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		int decrement = _autoOpenedParentheses > 0 && --_autoOpenedParentheses == 0 ? 2 : 1;
		if (_sumParentheses < decrement)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		IsNormalized = false;
		_conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - decrement };
		_sumParentheses -= decrement;

		return this;
	}

	public override QBBuilder<TDoc, TSelect> And()
	{
		((IMongoSelectQBBuilder<TDoc, TSelect>)this).And();
		return this;
	}
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.And()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		IsNormalized = false;
		_isByOr = false;

		return this;
	}
	public override QBBuilder<TDoc, TSelect> Or()
	{
		((IMongoSelectQBBuilder<TDoc, TSelect>)this).Or();
		return this;
	}
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Or()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
		}

		if (_autoOpenedParentheses == 1)
		{
			if (_sumParentheses < 1)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
			}

			var lastIndex = _conditions.Count - 1;
			var lastCond = _conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}': messed up with automatically opening parentheses.");
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

	public override QBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, null, refField);
	public override QBBuilder<TDoc, TSelect> Include<TRef>(DEPathDefinition<TSelect> field, DEPathDefinition<TRef> refField)
		=> AddInclude<TRef>(field, null, refField);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, null, refField);
	public override QBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, refAlias, refField);
	public override QBBuilder<TDoc, TSelect> Include<TRef>(DEPathDefinition<TSelect> field, string refAlias, DEPathDefinition<TRef> refField)
		=> AddInclude<TRef>(field, refAlias, refField);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField)
		=> AddInclude<TRef>(field, refAlias, refField);

	public override QBBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field)
		=> AddExclude(field, false);
	public override QBBuilder<TDoc, TSelect> Exclude(DEPathDefinition<TSelect> field)
		=> AddExclude(field, false);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Exclude(Expression<Func<TSelect, object?>> field)
		=> AddExclude(field, false);

	public override QBBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field)
		=> AddExclude(field, true);
	public override QBBuilder<TDoc, TSelect> Optional(DEPathDefinition<TSelect> field)
		=> AddExclude(field, true);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.Optional(Expression<Func<TSelect, object?>> field)
		=> AddExclude(field, true);

	public override QBBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending)
		=> AddSortBy(null, field, sortOrder);
	public override QBBuilder<TDoc, TSelect> SortBy(DEPathDefinition<TSelect> field, SO sortOrder = SO.Ascending)
		=> AddSortBy(null, field, sortOrder);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder)
		=> AddSortBy(null, field, sortOrder);
	public override QBBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending)
		=> AddSortBy(alias, field, sortOrder);
	public override QBBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, DEPathDefinition<TLocal> field, SO sortOrder = SO.Ascending)
		=> AddSortBy(alias, field, sortOrder);
	IMongoSelectQBBuilder<TDoc, TSelect> IMongoSelectQBBuilder<TDoc, TSelect>.SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder)
		=> AddSortBy(alias, field, sortOrder);


	private void CompleteAutoOpenedParentheses()
	{
		if (_autoOpenedParentheses > 0)
		{
			if (_sumParentheses < 1 || _autoOpenedParentheses > 1 || _conditions == null || _conditions.Count == 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}'.");
			}
			
			var lastIndex = _conditions.Count - 1;
			var lastCond = _conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of select query builder '{typeof(TSelect).ToPretty()}': messed up with automatically opening parentheses.");
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