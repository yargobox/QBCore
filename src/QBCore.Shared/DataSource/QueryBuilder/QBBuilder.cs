using System.Linq.Expressions;
using System.Reflection;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

public interface IQBBuilder
{
	QueryBuilderTypes QueryBuilderType { get; }
	IDataLayerInfo DataLayer { get; }

	Type DocumentType { get; }
	Type ProjectionType { get; }

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
	object SyncRoot { get; }

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
	public abstract IDataLayerInfo DataLayer { get; }
	public Type DocumentType => typeof(TDoc);
	public Type ProjectionType => typeof(TDto);
	public DSDocumentInfo DocumentInfo => _documentInfo;
	public DSDocumentInfo? ProjectionInfo => _projectionInfo;
	public bool IsNormalized { get; protected set; }

	public virtual IReadOnlyList<QBContainer> Containers => EmptyLists.Containers;
	public virtual IReadOnlyList<QBCondition> Connects => EmptyLists.Conditions;
	public virtual IReadOnlyList<QBCondition> Conditions => EmptyLists.Conditions;
	public virtual IReadOnlyList<QBField> Fields => EmptyLists.Fields;
	public virtual IReadOnlyList<QBParameter> Parameters => EmptyLists.Parameters;
	public virtual IReadOnlyList<QBSortOrder> SortOrders => EmptyLists.SortOrders;
	public virtual IReadOnlyList<QBAggregation> Aggregations => EmptyLists.Aggregations;

	public object SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;

	private object? _syncRoot;
	private readonly DSDocumentInfo _documentInfo;
	private readonly DSDocumentInfo? _projectionInfo;

	public QBBuilder()
	{
		_documentInfo = StaticFactory.Documents[typeof(TDoc)].Value;
		_projectionInfo = StaticFactory.Documents.GetValueOrDefault(typeof(TDto))?.Value;
	}
	public QBBuilder(QBBuilder<TDoc, TDto> other)
	{
		other.Normalize();
		IsNormalized = other.IsNormalized;

		_documentInfo = other._documentInfo;
		_projectionInfo = other._projectionInfo;
	}

	public virtual QBBuilder<TDoc, TDto> AutoBuild() => throw new NotSupportedException();

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

	public virtual QBBuilder<TDoc, TDto> Insert(string? tableName = null) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Update(string? tableName = null) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Delete(string? tableName = null) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Select(string? tableName = null, string? alias = null) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> LeftJoin<TRef>(string? tableName = null, string? alias = null) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Join<TRef>(string? tableName = null, string? alias = null) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> CrossJoin<TRef>(string? tableName = null, string? alias = null) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal, TRef>(DEPathDefinition<TLocal> field, DEPathDefinition<TRef> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, DEPathDefinition<TLocal> field, string refAlias, DEPathDefinition<TRef> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(DEPathDefinition<TLocal> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(string alias, DEPathDefinition<TLocal> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(DEPathDefinition<TLocal> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Connect<TLocal>(string alias, DEPathDefinition<TLocal> field, FO operation, string paramName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal, TRef>(DEPathDefinition<TLocal> field, DEPathDefinition<TRef> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, DEPathDefinition<TLocal> field, string refAlias, DEPathDefinition<TRef> refField, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(DEPathDefinition<TLocal> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(string alias, DEPathDefinition<TLocal> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(DEPathDefinition<TLocal> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition<TLocal>(string alias, DEPathDefinition<TLocal> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition(DEPathDefinition<TDoc> field, object? constValue, FO operation) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Condition(DEPathDefinition<TDoc> field, FO operation, string paramName) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Begin() => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> End() => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> And() => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Or() => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Include<TRef>(DEPathDefinition<TDto> field, DEPathDefinition<TRef> refField) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Include<TRef>(DEPathDefinition<TDto> field, string refAlias, DEPathDefinition<TRef> refField) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Exclude(DEPathDefinition<TDto> field) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Optional(Expression<Func<TDto, object?>> field) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> Optional(DEPathDefinition<TDto> field) => throw new NotSupportedException();

	public virtual QBBuilder<TDoc, TDto> SortBy(Expression<Func<TDto, object?>> field, SO sortOrder = SO.Ascending) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> SortBy(DEPathDefinition<TDto> field, SO sortOrder = SO.Ascending) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending) => throw new NotSupportedException();
	public virtual QBBuilder<TDoc, TDto> SortBy<TLocal>(string alias, DEPathDefinition<TLocal> field, SO sortOrder = SO.Ascending) => throw new NotSupportedException();
}