using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal abstract class CommonQBBuilder<TDoc, TDto> : QBBuilder<TDoc, TDto>
{
	public override IDataLayerInfo DataLayer => EfCoreDataLayer.Default;
	public override IReadOnlyList<QBContainer> Containers => _containers ?? EmptyLists.Containers;
	public override IReadOnlyList<QBCondition> Conditions => _conditions ?? EmptyLists.Conditions;
	public override IReadOnlyList<QBParameter> Parameters => _parameters ?? EmptyLists.Parameters;

	private List<QBContainer>? _containers;
	private List<QBCondition>? _conditions;
	private List<QBParameter>? _parameters;

	private int _parentheses;
	private int _sumParentheses;
	private bool? _isByOr;
	private int _autoOpenedParentheses;

	public CommonQBBuilder() { }
	public CommonQBBuilder(CommonQBBuilder<TDoc, TDto> other) : base(other)
	{
		if (other._containers != null) _containers = new List<QBContainer>(1) { other._containers.First() };
		if (other._conditions != null) _conditions = new List<QBCondition>(other._conditions);
		if (other._parameters != null) _parameters = new List<QBParameter>(other._parameters);
	}

	protected override void OnNormalize()
	{
		if (_isByOr != null || _parentheses > 0 || Containers.Count != 1)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}

		CompleteAutoOpenedParentheses();
		if (_sumParentheses != 0 || _autoOpenedParentheses != 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}
	}

	protected CommonQBBuilder<TDoc, TDto> AddContainer(string? dbSideName, ContainerTypes containerType, ContainerOperations containerOperation)
	{
		if (Containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
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
			ContainerType: containerType,
			ContainerOperation: containerOperation
		));

		return this;
	}

	protected CommonQBBuilder<TDoc, TDto> AddCondition(
		QBConditionFlags flags,
		DEPathDefinition<TDoc> field,
		object? constValue,
		string? paramName,
		FO operation)
	{
		return AddCondition(
			flags,
			field.ToDataEntryPath(EfCoreDataLayer.Default),
			constValue,
			paramName,
			operation
		);
	}
	protected CommonQBBuilder<TDoc, TDto> AddCondition(
		QBConditionFlags flags,
		Expression<Func<TDoc, object?>> field,
		object? constValue,
		string? paramName,
		FO operation)
	{
		return AddCondition(
			flags,
			new DEPath(field, false, EfCoreDataLayer.Default),
			constValue,
			paramName,
			operation
		);
	}
	protected CommonQBBuilder<TDoc, TDto> AddCondition(
		QBConditionFlags flags,
		DEPath field,
		object? constValue,
		string? paramName,
		FO operation)
	{
		var alias = Containers.FirstOrDefault()?.Alias;
		if (alias == null)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}': referenced container of document '{typeof(TDoc).ToPretty()}' has not been added yet.");
		}

		if (field == null)
		{
			throw new ArgumentNullException(nameof(field));
		}
		if (field.Count == 0 || field.DocumentType != typeof(TDoc))
		{
			throw new ArgumentException(nameof(field));
		}

		var onWhat = flags & (QBConditionFlags.OnField | QBConditionFlags.OnConst | QBConditionFlags.OnParam);
		if (onWhat == QBConditionFlags.OnField)
		{
			throw new ArgumentException(nameof(flags));
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
			throw new ArgumentException(nameof(flags));
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
							throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
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
				refAlias: null,
				refField: null,
				value: onWhat == QBConditionFlags.OnParam ? paramName : onWhat == QBConditionFlags.OnConst ? constValue : null,
				operation: operation
			) { Parentheses = _parentheses });

			_isByOr = null;
			_parentheses = 0;
		}

		if (flags.HasFlag(QBConditionFlags.OnParam))
		{
			var fieldPath = _conditions![_conditions.Count - 1].Field;

			AddParameter(paramName!, fieldPath.DataEntryType, fieldPath.IsNullable, System.Data.ParameterDirection.Input);
		}

		return this;
	}

	protected QBBuilder<TDoc, TDto> AddParameter(string name, Type underlyingType, bool isNullable, System.Data.ParameterDirection direction)
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
				throw new InvalidOperationException($"Incorrect parameter definition of query builder '{typeof(TDto).ToPretty()}': parameter '{name}' has already been added before with different properties");
			}
		}
		else
		{
			IsNormalized = false;
			_parameters.Add(new QBParameter(name, underlyingType, isNullable, direction));
		}

		return this;
	}

	public override QBBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation)
		=> AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
	public override QBBuilder<TDoc, TDto> Condition(DEPathDefinition<TDoc> field, object? constValue, FO operation)
		=> AddCondition(QBConditionFlags.OnConst, field, constValue, null, operation);
	public override QBBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName)
		=> AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);
	public override QBBuilder<TDoc, TDto> Condition(DEPathDefinition<TDoc> field, FO operation, string paramName)
		=> AddCondition(QBConditionFlags.OnParam, field, null, paramName, operation);

	public override QBBuilder<TDoc, TDto> Begin()
	{
		IsNormalized = false;
		_parentheses++;
		_sumParentheses++;
		if (_autoOpenedParentheses > 0) _autoOpenedParentheses++;
		return this;
	}
	public override QBBuilder<TDoc, TDto> End()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}

		var lastIndex = _conditions.Count - 1;
		var lastCond = _conditions[lastIndex];
		if (lastCond.Parentheses > 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}

		int decrement = _autoOpenedParentheses > 0 && --_autoOpenedParentheses == 0 ? 2 : 1;
		if (_sumParentheses < decrement)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}

		IsNormalized = false;
		_conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - decrement };
		_sumParentheses -= decrement;

		return this;
	}

	public override QBBuilder<TDoc, TDto> And()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}

		IsNormalized = false;
		_isByOr = false;

		return this;
	}
	public override QBBuilder<TDoc, TDto> Or()
	{
		if (_isByOr != null || _parentheses > 0 || _conditions == null || _conditions.Count == 0)
		{
			throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
		}

		if (_autoOpenedParentheses == 1)
		{
			if (_sumParentheses < 1)
			{
				throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
			}

			var lastIndex = _conditions.Count - 1;
			var lastCond = _conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
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

	private void CompleteAutoOpenedParentheses()
	{
		if (_autoOpenedParentheses > 0)
		{
			if (_sumParentheses < 1 || _autoOpenedParentheses > 1 || _conditions == null || _conditions.Count == 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}'.");
			}
			
			var lastIndex = _conditions.Count - 1;
			var lastCond = _conditions[lastIndex];
			if (lastCond.Parentheses > 0)
			{
				throw new InvalidOperationException($"Incorrect condition definition of query builder '{typeof(TDto).ToPretty()}': messed up with automatically opening parentheses.");
			}

			_conditions[lastIndex] = lastCond with { Parentheses = lastCond.Parentheses - 1 };
			_sumParentheses--;
			_autoOpenedParentheses = 0;
		}
	}
}