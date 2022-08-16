using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

public interface IQBBuilder
{
	QueryBuilderTypes QueryBuilderType { get; }

	DSDocumentInfo DocumentInfo { get; }
	DSDocumentInfo? ProjectionInfo { get; }

	IReadOnlyList<QBContainer> Containers { get; }
	IReadOnlyList<QBCondition> Connects { get; }
	IReadOnlyList<QBCondition> Conditions { get; }
	IReadOnlyList<QBField> Fields { get; }
	IReadOnlyList<QBParameter> Parameters { get; }
	IReadOnlyList<QBSortOrder> SortOrders { get; }
	IReadOnlyList<QBAggregation> Aggregations { get; }

	bool IsNormalized { get; }

	void Normalize();
}

public abstract class QBBuilder<TDoc, TDto> : IQBBuilder
{
	protected static class EmptyLists
	{
		public static readonly List<QBContainer> Containers = new List<QBContainer>(0);
		public static readonly List<QBCondition> Conditions = new List<QBCondition>(0);
		public static readonly List<QBField> Fields = new List<QBField>(0);
		public static readonly List<QBParameter> Parameters = new List<QBParameter>(0);
		public static readonly List<QBSortOrder> SortOrders = new List<QBSortOrder>(0);
		public static readonly List<QBAggregation> Aggregations = new List<QBAggregation>(0);
	}

	public abstract QueryBuilderTypes QueryBuilderType { get; }
	public DSDocumentInfo DocumentInfo => _documentInfo;
	public DSDocumentInfo? ProjectionInfo => _projectionInfo;
	public bool IsNormalized { get; protected set; }

	public IReadOnlyList<QBContainer> Containers => _containers;
	public IReadOnlyList<QBCondition> Connects => _connects ?? EmptyLists.Conditions;
	public IReadOnlyList<QBCondition> Conditions => _conditions ?? EmptyLists.Conditions;
	public IReadOnlyList<QBField> Fields => _fields ?? EmptyLists.Fields;
	public IReadOnlyList<QBParameter> Parameters => _parameters ?? EmptyLists.Parameters;
	public IReadOnlyList<QBSortOrder> SortOrders => _sortOrders ?? EmptyLists.SortOrders;
	public IReadOnlyList<QBAggregation> Aggregations => _aggregations ?? EmptyLists.Aggregations;

	private readonly DSDocumentInfo _documentInfo;
	private readonly DSDocumentInfo? _projectionInfo;
	protected readonly List<QBContainer> _containers;
	protected List<QBField>? _fields;
	protected List<QBCondition>? _connects;
	protected List<QBCondition>? _conditions;
	protected List<QBParameter>? _parameters;
	protected List<QBSortOrder>? _sortOrders;
	protected List<QBAggregation>? _aggregations;

	public QBBuilder()
	{
		_documentInfo = StaticFactory.Documents[typeof(TDoc)].Value;
		_projectionInfo = StaticFactory.Documents.GetValueOrDefault(typeof(TDoc))?.Value;
		_containers = new List<QBContainer>(3);
	}
	public QBBuilder(QBBuilder<TDoc, TDto> other)
	{
		if (!(IsNormalized = other.IsNormalized))
		{
			other.Normalize();
		}

		_documentInfo = other._documentInfo;
		_projectionInfo = other._projectionInfo;
		_containers = new List<QBContainer>(other._containers);
		if (other._fields != null) _fields = new List<QBField>(other._fields);
		if (other._parameters != null) _parameters = new List<QBParameter>(other._parameters);
		if (other._connects != null) _connects = new List<QBCondition>(other._connects);
		if (other._conditions != null) _conditions = new List<QBCondition>(other._conditions);
		if (other._sortOrders != null) _sortOrders = new List<QBSortOrder>(other._sortOrders);
		if (other._aggregations != null) _aggregations = new List<QBAggregation>(other._aggregations);
	}

	public void Normalize()
	{
		if (IsNormalized)
		{
			return;
		}

		OnNormalize();

		IsNormalized = true;
	}

	protected virtual void OnNormalize() { }

	public virtual Func<IDSIdGenerator>? IdGenerator { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

	public virtual QBBuilder<TDoc, TDto> SelectFrom(string tableName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> SelectFrom(string alias, string tableName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> InsertTo(string tableName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> LeftJoin<TRef>(string tableName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> LeftJoin<TRef>(string alias, string tableName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Join<TRef>(string tableName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Join<TRef>(string alias, string tableName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> CrossJoin<TRef>(string tableName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> CrossJoin<TRef>(string alias, string tableName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	//public virtual QBBuilder<TDoc, TDto> Condition(DEDefinition<TDoc> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Begin() => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> End() => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> And() => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Or() => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Optional(Expression<Func<TDto, object?>> field) => throw new NotSupportedException();
}