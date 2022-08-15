using System.Data;
using System.Linq.Expressions;
using QBCore.Extensions.Linq;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class QBInsertBuilder<TDoc, TDto> : IQBInsertBuilder<TDoc, TDto>, IQBMongoInsertBuilder<TDoc, TDto>, ICloneable
{
	public QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;
	public DSDocumentInfo DocumentInfo => _documentInfo;
	public DSDocumentInfo? ProjectionInfo => _projectionInfo;
	public bool IsNormalized { get; private set; }

	public IReadOnlyList<QBContainer> Containers => _containers;
	public IReadOnlyList<QBCondition> Connects => BuilderEmptyLists.Conditions;
	public IReadOnlyList<QBCondition> Conditions => BuilderEmptyLists.Conditions;
	public IReadOnlyList<QBField> Fields => BuilderEmptyLists.Fields;
	public IReadOnlyList<QBParameter> Parameters => _parameters ?? BuilderEmptyLists.Parameters;
	public IReadOnlyList<QBSortOrder> SortOrders => BuilderEmptyLists.SortOrders;
	public IReadOnlyList<QBAggregation> Aggregations => BuilderEmptyLists.Aggregations;

	private readonly DSDocumentInfo _documentInfo;
	private readonly DSDocumentInfo? _projectionInfo;
	private readonly List<QBContainer> _containers;
	private List<QBParameter>? _parameters;
	private Func<IDSIdGenerator>? _customIdGenerator;

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

	Func<IDSIdGenerator>? IQBMongoInsertBuilder<TDoc, TDto>.CustomIdGenerator
	{
		get => this.CustomIdGenerator;
		set => this.CustomIdGenerator = value;
	}

	public QBInsertBuilder()
	{
		_documentInfo = StaticFactory.Documents[typeof(TDoc)].Value;
		_projectionInfo = StaticFactory.Documents.GetValueOrDefault(typeof(TDoc))?.Value;
		_containers = new List<QBContainer>(1);
	}

	public QBInsertBuilder(QBInsertBuilder<TDoc, TDto> other)
	{
		if (!(IsNormalized = other.IsNormalized))
		{
			other.Normalize();
		}

		_documentInfo = other._documentInfo;
		_projectionInfo = other._projectionInfo;
		_containers = new List<QBContainer>(other._containers);
		if (other._parameters != null) _parameters = new List<QBParameter>(other._parameters);
		_customIdGenerator = other._customIdGenerator;
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

	private QBInsertBuilder<TDoc, TDto> AutoAddParameters()
		=> throw new NotSupportedException($"{nameof(QBInsertBuilder<TDoc, TDto>)}.{nameof(AutoAddParameters)} is not supported by Mongo query builder.");

	public IQBInsertBuilder<TDoc, TDto> InsertTo(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Insert);
	IQBMongoInsertBuilder<TDoc, TDto> IQBMongoInsertBuilder<TDoc, TDto>.InsertTo(string tableName)
		=> AddContainer(typeof(TDoc), tableName, tableName, ContainerTypes.Table, ContainerOperations.Insert);
	public IQBInsertBuilder<TDoc, TDto> ExecProcedure(string procedureName)
		=> throw new NotSupportedException($"{nameof(QBInsertBuilder<TDoc, TDto>)}.{nameof(ExecProcedure)} is not supported by Mongo query builder.");
	public IQBInsertBuilder<TDoc, TDto> AutoBindParameters()
		=> AutoAddParameters();
	public IQBInsertBuilder<TDoc, TDto> BindParameter(Expression<Func<TDto, object?>> field, ParameterDirection direction, bool isErrorCode = false)
		=> AddParameter(field, direction, isErrorCode, false);
	public IQBInsertBuilder<TDoc, TDto> BindParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false)
		=> AddParameter(name, underlyingType, isNullable, direction, isErrorCode, false);
	public IQBInsertBuilder<TDoc, TDto> BindReturnValueToErrorCode()
		=> AddParameter("@@RETURN_VALUE", typeof(int), false, ParameterDirection.ReturnValue, true, false);
	public IQBInsertBuilder<TDoc, TDto> BindParameterToErrorMessage(string name)
		=> AddParameter(name, typeof(string), true, ParameterDirection.Output, false, true);
}