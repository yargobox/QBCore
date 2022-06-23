using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class CDSNodeBuilder : ICDSNodeBuilder
{
	private readonly CDSNode _node;

	public string Name => _node.Name;
	public Type DataSourceType => _node.DataSourceType;
	public IEnumerable<ICDSCondition> Conditions => _node.Conditions;
	public bool Hidden => _node.Hidden;
	public ICDSNodeBuilder Root => new CDSNodeBuilder(_node.Root);
	public ICDSNodeBuilder? Parent => _node.Parent != null ? new CDSNodeBuilder(_node.Parent) : null;
	public IReadOnlyDictionary<string, ICDSNodeBuilder> All => new NodeDictionary(_node.All);
	public IReadOnlyDictionary<string, ICDSNodeBuilder> Parents => new NodeDictionary(_node.Parents);
	public IReadOnlyDictionary<string, ICDSNodeBuilder> Children => new NodeDictionary(_node.Children);

	public CDSNodeBuilder(ICDSNode node) => _node = (CDSNode)node;

	public ICDSNode AsNode() => _node;

	public ICDSNodeBuilder AddNode<TDataSource>() where TDataSource : IDataSource
	{
		var dataSourceDefinition = StaticFactory.DataSources[typeof(TDataSource)];

		return new CDSNodeBuilder(_node.AddNode(typeof(TDataSource), dataSourceDefinition.ControllerName ?? dataSourceDefinition.Name));
	}
	public ICDSNodeBuilder AddNode<TDataSource>(string nodeName) where TDataSource : IDataSource
	{
		_ = StaticFactory.DataSources[typeof(TDataSource)];

		return new CDSNodeBuilder(_node.AddNode(typeof(TDataSource), nodeName));
	}
	public ICDSNodeBuilder AddCondition<TDocument, TParentDocument>(Expression<Func<TDocument, object?>> field, Expression<Func<TParentDocument, object?>> parentField, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null)
	{
		_node.AddCondition<TDocument, TParentDocument>(field, parentField, operation, defaultValue);
		return this;
	}
	public ICDSNodeBuilder AddCondition<TDocument, TParentDocument>(Expression<Func<TDocument, object?>> field, ICDSNodeBuilder parentNode, Expression<Func<TParentDocument, object?>> parentField, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null)
	{
		_node.AddCondition<TDocument, TParentDocument>(field, ((CDSNodeBuilder)parentNode)._node, parentField, operation, defaultValue);
		return this;
	}
	public ICDSNodeBuilder AddCondition<TDocument>(Expression<Func<TDocument, object?>> field, object? constValue, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null)
	{
		_node.AddCondition<TDocument>(field, constValue, operation, defaultValue);
		return this;
	}
	public ICDSNodeBuilder SetHidden(bool hide = true)
	{
		_node.Hidden = hide;
		return this;
	}

	internal class NodeDictionary : IReadOnlyDictionary<string, ICDSNodeBuilder>
	{
		public readonly IReadOnlyDictionary<string, ICDSNode> Nodes;
		public NodeDictionary(IReadOnlyDictionary<string, ICDSNode> nodes) => Nodes = nodes;
		public ICDSNodeBuilder this[string key] => new CDSNodeBuilder(Nodes[key]);
		public IEnumerable<string> Keys => Nodes.Keys;
		public IEnumerable<ICDSNodeBuilder> Values => Nodes.Values.Select(x => new CDSNodeBuilder(x));
		public int Count => Nodes.Count;
		public bool ContainsKey(string key) => Nodes.ContainsKey(key);
		public IEnumerator<KeyValuePair<string, ICDSNodeBuilder>> GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Key, (ICDSNodeBuilder)new CDSNodeBuilder(x.Value))).GetEnumerator();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out ICDSNodeBuilder value)
		{
			ICDSNode? node;
			if (Nodes.TryGetValue(key, out node))
			{
				value = new CDSNodeBuilder(node);
				return true;
			}
			value = null;
			return false;
		}
		IEnumerator IEnumerable.GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Key, (ICDSNodeBuilder)new CDSNodeBuilder(x.Value))).GetEnumerator();
	}
}