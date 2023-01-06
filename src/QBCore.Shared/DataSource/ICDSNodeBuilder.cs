using System.Linq.Expressions;

namespace QBCore.DataSource;

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
	ICDSNodeBuilder AddCondition<TDoc, TParentDoc>(Expression<Func<TDoc, object?>> field, Expression<Func<TParentDoc, object?>> parentField, FO operation = FO.Equal, object? defaultValue = null);
	ICDSNodeBuilder AddCondition<TDoc, TParentDoc>(Expression<Func<TDoc, object?>> field, ICDSNodeBuilder parentNode, Expression<Func<TParentDoc, object?>> parentField, FO operation = FO.Equal, object? defaultValue = null);
	ICDSNodeBuilder AddCondition<TDoc>(Expression<Func<TDoc, object?>> field, object? constValue, FO operation = FO.Equal, object? defaultValue = null);
	ICDSNodeBuilder SetHidden(bool hide = true);
}