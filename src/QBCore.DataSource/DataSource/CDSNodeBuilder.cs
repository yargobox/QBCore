using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class CDSNodeBuilder : ICDSNodeBuilder
{
	private readonly CDSNodeInfo _node;

	public string Name => _node.Name;
	public Type DataSourceType => _node.DataSourceType;
	public IEnumerable<ICDSCondition> Conditions => _node.Conditions;
	public bool Hidden => _node.Hidden;
	public ICDSNodeBuilder Root => new CDSNodeBuilder(_node.Root);
	public ICDSNodeBuilder? Parent => _node.Parent != null ? new CDSNodeBuilder(_node.Parent) : null;
	public IReadOnlyDictionary<string, ICDSNodeBuilder> All => new NodeDictionary(_node.All);
	public IReadOnlyDictionary<string, ICDSNodeBuilder> Parents => new NodeDictionary(_node.Parents);
	public IReadOnlyDictionary<string, ICDSNodeBuilder> Children => new NodeDictionary(_node.Children);

	public CDSNodeBuilder(ICDSNodeInfo node) => _node = (CDSNodeInfo)node;

	public ICDSNodeInfo AsNode() => _node;

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
	public ICDSNodeBuilder AddCondition<TDoc, TParentDoc>(Expression<Func<TDoc, object?>> field, Expression<Func<TParentDoc, object?>> parentField, FO operation = FO.Equal, object? defaultValue = null)
	{
		_node.AddCondition<TDoc, TParentDoc>(field, parentField, operation, defaultValue);
		return this;
	}
	public ICDSNodeBuilder AddCondition<TDoc, TParentDoc>(Expression<Func<TDoc, object?>> field, ICDSNodeBuilder parentNode, Expression<Func<TParentDoc, object?>> parentField, FO operation = FO.Equal, object? defaultValue = null)
	{
		_node.AddCondition<TDoc, TParentDoc>(field, ((CDSNodeBuilder)parentNode)._node, parentField, operation, defaultValue);
		return this;
	}
	public ICDSNodeBuilder AddCondition<TDoc>(Expression<Func<TDoc, object?>> field, object? constValue, FO operation = FO.Equal, object? defaultValue = null)
	{
		_node.AddCondition<TDoc>(field, constValue, operation, defaultValue);
		return this;
	}
	public ICDSNodeBuilder SetHidden(bool hide = true)
	{
		_node.Hidden = hide;
		return this;
	}

	internal class NodeDictionary : IReadOnlyDictionary<string, ICDSNodeBuilder>
	{
		public readonly IReadOnlyDictionary<string, ICDSNodeInfo> Nodes;
		public NodeDictionary(IReadOnlyDictionary<string, ICDSNodeInfo> nodes) => Nodes = nodes;
		public ICDSNodeBuilder this[string key] => new CDSNodeBuilder(Nodes[key]);
		public IEnumerable<string> Keys => Nodes.Keys;
		public IEnumerable<ICDSNodeBuilder> Values => Nodes.Values.Select(x => new CDSNodeBuilder(x));
		public int Count => Nodes.Count;
		public bool ContainsKey(string key) => Nodes.ContainsKey(key);
		public IEnumerator<KeyValuePair<string, ICDSNodeBuilder>> GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Key, (ICDSNodeBuilder)new CDSNodeBuilder(x.Value))).GetEnumerator();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out ICDSNodeBuilder value)
		{
			ICDSNodeInfo? node;
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