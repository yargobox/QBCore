using System.Data;
using System.Linq.Expressions;
using QBCore.Extensions.Linq;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBInsertBuilder<TDoc, TDto> : IQBInsertBuilder<TDoc, TDto>, IQBMongoInsertBuilder<TDoc, TDto>, ICloneable
{
	public QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;
	public bool IsNormalized { get; private set; }

	public IReadOnlyList<QBContainer> Containers => _containers;
	public IReadOnlyList<QBCondition> Connects => BuilderEmptyLists.Conditions;
	public IReadOnlyList<QBCondition> Conditions => BuilderEmptyLists.Conditions;
	public IReadOnlyList<QBField> Fields => BuilderEmptyLists.Fields;
	public IReadOnlyList<QBParameter> Parameters => _parameters ?? BuilderEmptyLists.Parameters;
	public IReadOnlyList<QBSortOrder> SortOrders => BuilderEmptyLists.SortOrders;
	public IReadOnlyList<QBAggregation> Aggregations => BuilderEmptyLists.Aggregations;

	private readonly List<QBContainer> _containers = new List<QBContainer>(1);
	private List<QBParameter>? _parameters;

	public Expression<Func<TDoc, object?>>? IdField
	{
		get => _idField;
		set
		{
			if (_idField != null)
			{
				throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': option '{nameof(IdField)}' is already set.");
			}
			if (value != null)
			{
				IdGetter = _idField?.Compile();
				
				_idField = value;
			}
		}
	}
	public Func<TDoc, object?>? IdGetter { get; private set; }
	public Action<TDoc, object?>? IdSetter { get; private set; }
	public Expression<Func<TDoc, object?>>? DateCreateField
	{
		get => _dateCreateField;
		set
		{
			if (_dateCreateField != null)
				throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': option '{nameof(DateCreateField)}' is already set.");
			_dateCreateField = value;
		}
	}
	public Expression<Func<TDoc, object?>>? DateModifyField
	{
		get => _dateModifyField;
		set
		{
			if (_dateModifyField != null)
				throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': option '{nameof(DateModifyField)}' is already set.");
			_dateModifyField = value;
		}
	}
	public Func<IDSIdGenerator>? CustomIdGenerator
	{
		get => _customIdGenerator;
		set
		{
			if (_customIdGenerator != null)
				throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': option '{nameof(CustomIdGenerator)}' is already set.");
			_customIdGenerator = value;
		}
	}

	Expression<Func<TDoc, object?>>? IQBMongoInsertBuilder<TDoc, TDto>.DateCreateField
	{
		get => this.DateCreateField;
		set => this.DateCreateField = value;
	}
	Expression<Func<TDoc, object?>>? IQBMongoInsertBuilder<TDoc, TDto>.DateModifyField
	{
		get => this.DateModifyField;
		set => this.DateModifyField = value;
	}
	Func<IDSIdGenerator>? IQBMongoInsertBuilder<TDoc, TDto>.CustomIdGenerator
	{
		get => this.CustomIdGenerator;
		set => this.CustomIdGenerator = value;
	}

	private Expression<Func<TDoc, object?>>? _idField;
	private Expression<Func<TDoc, object?>>? _dateCreateField;
	private Expression<Func<TDoc, object?>>? _dateModifyField;
	private Func<IDSIdGenerator>? _customIdGenerator;

	public QBInsertBuilder()
	{
		_idField =
			typeof(TDoc).GetProperties().Where(x => x.IsDefined(typeof(DeIdAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>()
			?? typeof(TDoc).GetFields().Where(x => x.IsDefined(typeof(DeIdAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>();

		_dateCreateField =
			typeof(TDoc).GetProperties().Where(x => x.IsDefined(typeof(DeCreatedAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>()
			?? typeof(TDoc).GetFields().Where(x => x.IsDefined(typeof(DeCreatedAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>();

		DateModifyField =
			typeof(TDoc).GetProperties().Where(x => x.IsDefined(typeof(DeModifiedAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>()
			?? typeof(TDoc).GetFields().Where(x => x.IsDefined(typeof(DeModifiedAttribute), true)).FirstOrDefault()?.ToMemberExpression<TDoc>();
	}
	public QBInsertBuilder(QBInsertBuilder<TDoc, TDto> other)
	{
		if (!(IsNormalized = other.IsNormalized))
		{
			other.Normalize();
		}

		_containers = new List<QBContainer>(other._containers);
		if (other._parameters != null) _parameters = new List<QBParameter>(other._parameters);
		_idField = other._idField;
		_dateCreateField = other._dateCreateField;
		_dateModifyField = other._dateModifyField;
	}
	public object Clone() => new QBInsertBuilder<TDoc, TDto>(this);

	public void Normalize()
	{
		if (IsNormalized) return;
		
		if (_containers.Count != 1)
		{
			throw new InvalidOperationException($"Incompatible configuration of insert query builder '{typeof(TDto).ToPretty()}'.");
		}

		IsNormalized = true;
	}

	private QBInsertBuilder<TDoc, TDto> AddContainer(
		Type documentType,
		string alias,
		string dbSideName,
		ContainerTypes containerType,
		ContainerOperations containerOperation)
	{
		if (_containers.Count > 0)
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': initial container has already been added before.");
		}
		if (alias.Contains('.'))
		{
			throw new InvalidOperationException($"Incorrect definition of insert query builder '{typeof(TDto).ToPretty()}': container alias '{alias}' cannot contain a period character '.'.");
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

	private QBInsertBuilder<TDoc, TDto> AddParameter(Expression<Func<TDto, object?>> field, ParameterDirection direction, bool isErrorCode, bool isErrorMessage)
	{
		var fieldPath = new MongoFieldPath(field, false);
		return AddParameter(fieldPath.FullName, fieldPath.FieldType, fieldPath.IsNullable, direction, isErrorCode, isErrorMessage);
	}
	private QBInsertBuilder<TDoc, TDto> AddParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode, bool isErrorMessage)
	{
		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(nameof(name));
		}
		if (underlyingType == null)
		{
			throw new ArgumentNullException(nameof(underlyingType));
		}

		if (_parameters == null)
		{
			_parameters = new List<QBParameter>(8);
		}

		var param = _parameters.FirstOrDefault(x => x.Name == name);
		if (param != null)
		{
			if (param.UnderlyingType != underlyingType || param.IsNullable != isNullable || param.Direction != direction || param.IsErrorCode != isErrorCode || param.IsErrorMessage != isErrorMessage)
			{
				throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}': parameter '{name}' has already been added before.");
			}
		}
		else
		{
			if (direction != ParameterDirection.Input && !_containers.Any(x => x.ContainerOperation == ContainerOperations.Exec))
			{
				throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}'.");
			}
			if (isErrorCode)
			{
				if (underlyingType != typeof(int) || (direction != ParameterDirection.Output && direction != ParameterDirection.InputOutput && direction != ParameterDirection.ReturnValue))
				{
					throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}': error code parameter '{name}' must be an output parameter of data type 'int'.");
				}
				if (_parameters.Any(x => x.IsErrorCode))
				{
					throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}': an error code parameter such as '{name}' has already been added before.");
				}
			}
			if (isErrorMessage)
			{
				if (underlyingType != typeof(string) || (direction != ParameterDirection.Output && direction != ParameterDirection.InputOutput))
				{
					throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}': error message parameter '{name}' must be an output parameter of data type 'string'.");
				}
				if (_parameters.Any(x => x.IsErrorMessage))
				{
					throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}': an error message parameter such as '{name}' has already been added before.");
				}
			}
			if (direction == ParameterDirection.ReturnValue && _parameters.Any(x => x.Direction == ParameterDirection.ReturnValue))
			{
				throw new InvalidOperationException($"Incorrect parameter definition of insert query builder '{typeof(TDto).ToPretty()}': a return value parameter such as '{name}' has already been added before.");
			}
			

			IsNormalized = false;
			_parameters.Add(new QBParameter(name, underlyingType, isNullable, direction, isErrorCode, isErrorMessage));
		}

		return this;
	}

	private QBInsertBuilder<TDoc, TDto> AutoAddParameters() => this;

	public IQBInsertBuilder<TDoc, TDto> InsertTo(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Insert);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.InsertTo(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Insert);
	public IQBInsertBuilder<TDoc, TDto> ExecProcedure(string procedureName)
		=> AddContainer(typeof(TDoc), procedureName, procedureName, ContainerTypes.Procedure, ContainerOperations.Exec);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.ExecProcedure(string procedureName)
		=> AddContainer(typeof(TDoc), procedureName, procedureName, ContainerTypes.Procedure, ContainerOperations.Exec);
	public IQBInsertBuilder<TDoc, TDto> AutoBindParameters()
		=> AutoAddParameters();
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.AutoBindParameters()
		=> AutoAddParameters();
	public IQBInsertBuilder<TDoc, TDto> BindParameter(Expression<Func<TDto, object?>> field, ParameterDirection direction, bool isErrorCode = false)
		=> AddParameter(field, direction, isErrorCode, false);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.BindParameter(Expression<Func<TDto, object?>> field, ParameterDirection direction, bool isErrorCode)
		=> AddParameter(field, direction, isErrorCode, false);
	public IQBInsertBuilder<TDoc, TDto> BindParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false)
		=> AddParameter(name, underlyingType, isNullable, direction, isErrorCode, false);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.BindParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode)
		=> AddParameter(name, underlyingType, isNullable, direction, isErrorCode, false);
	public IQBInsertBuilder<TDoc, TDto> BindReturnValueToErrorCode()
		=> AddParameter("@@RETURN_VALUE", typeof(int), false, ParameterDirection.ReturnValue, true, false);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.BindReturnValueToErrorCode()
		=> AddParameter("@@RETURN_VALUE", typeof(int), false, ParameterDirection.ReturnValue, true, false);
	public IQBInsertBuilder<TDoc, TDto> BindParameterToErrorMessage(string name)
		=> AddParameter(name, typeof(string), true, ParameterDirection.Output, false, true);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.BindParameterToErrorMessage(string name)
		=> AddParameter(name, typeof(string), true, ParameterDirection.Output, false, true);
}