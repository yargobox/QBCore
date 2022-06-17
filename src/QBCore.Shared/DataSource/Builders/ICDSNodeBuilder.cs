using System.Linq.Expressions;

namespace QBCore.DataSource.Builders;

public interface ICDSNodeBuilder
{
	string Name { get; }
	Type DataSourceType { get; }
	IEnumerable<ICDSCondition> Conditions { get; }
	bool Hidden { get; }
	ICDSNodeBuilder Root { get; }
	ICDSNodeBuilder? Parent { get; }
	IReadOnlyDictionary<string, ICDSNodeBuilder> All { get; }
	IReadOnlyDictionary<string, ICDSNodeBuilder> Parents { get; }
	IReadOnlyDictionary<string, ICDSNodeBuilder> Children { get; }

	ICDSNodeBuilder AddNode<TDataSource>() where TDataSource : IDataSource;
	ICDSNodeBuilder AddNode<TDataSource>(string nodeName) where TDataSource : IDataSource;
	ICDSNodeBuilder AddCondition<TDocument, TParentDocument>(Expression<Func<TDocument, object?>> field, Expression<Func<TParentDocument, object?>> parentField, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null);
	ICDSNodeBuilder AddCondition<TDocument, TParentDocument>(Expression<Func<TDocument, object?>> field, ICDSNodeBuilder parentNode, Expression<Func<TParentDocument, object?>> parentField, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null);
	ICDSNodeBuilder AddCondition<TDocument>(Expression<Func<TDocument, object?>> field, object? constValue, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null);
	ICDSNodeBuilder SetHidden(bool hide = true);
}