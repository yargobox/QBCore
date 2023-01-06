using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using QBCore.Extensions.Collections.Generic;
using QBCore.Extensions.Linq.Expressions;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class CDSNodeInfo : ICDSNodeInfo
{
	public string Name { get; }
	public Type DataSourceType { get; }
	public IDSInfo DSInfo => _dsInfo ?? (_dsInfo = StaticFactory.DataSources[DataSourceType]) ?? throw new InvalidOperationException($"'{nameof(DSInfo)}' is not available in the builder.");
	public IEnumerable<ICDSCondition> Conditions => _conditions ?? Enumerable.Empty<ICDSCondition>();
	public bool Hidden { get; set; }
	public ICDSNodeInfo? Parent { get; }
	public ICDSNodeInfo Root => _collection.Values.First();
	public IReadOnlyDictionary<string, ICDSNodeInfo> All => _collection;
	public IReadOnlyDictionary<string, ICDSNodeInfo> Parents => Parent != null ? new NodeDictionary(GetParents(this)) : _emptyReadOnlyDictionary;
	public IReadOnlyDictionary<string, ICDSNodeInfo> Children => _children != null ? new NodeDictionary(_children) : _emptyReadOnlyDictionary;

	private static readonly IReadOnlyDictionary<string, ICDSNodeInfo> _emptyReadOnlyDictionary = new Dictionary<string, ICDSNodeInfo>();
	private const string _rootName = "<ROOT>";

	private readonly OrderedDictionary<string, ICDSNodeInfo> _collection;
	private ICDSNodeInfo[]? _children;
	private ICDSCondition[]? _conditions;
	private IDSInfo? _dsInfo;

	// root node ctor to start collection
	public CDSNodeInfo()
	{
		_collection = new OrderedDictionary<string, ICDSNodeInfo>(StringComparer.OrdinalIgnoreCase);
		Name = _rootName;
		DataSourceType = typeof(void);
	}

	// regular node ctor
	public CDSNodeInfo(OrderedDictionary<string, ICDSNodeInfo> collection, Type dataSourceType, string name, ICDSNodeInfo? parent)
	{
		_collection = collection;
		Parent = parent;
		Name = name;
		DataSourceType = dataSourceType;
	}

	private IEnumerable<ICDSNodeInfo> GetParents(ICDSNodeInfo node)
	{
		while (node.Parent != null)
		{
			yield return node.Parent;
			node = node.Parent;
		}
	}

	public ICDSNodeInfo AddNode(Type dataSourceConcreteType, string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(nameof(name));
		}

		if (!dataSourceConcreteType.IsClass || dataSourceConcreteType.IsAbstract)
		{
			throw new InvalidOperationException($"Invalid datasource type {dataSourceConcreteType.ToPretty()}.");
		}
		if (dataSourceConcreteType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) == null)
		{
			throw new InvalidOperationException($"Invalid datasource type {dataSourceConcreteType.ToPretty()}.");
		}

		var node = new CDSNodeInfo(_collection, dataSourceConcreteType, name, Name != _rootName ? this : null);
		_collection.Add(name, node);

		AddNode(node);

		return node;
	}
	public ICDSNodeInfo AddCondition<TDoc, TParentDoc>(Expression<Func<TDoc, object?>> field, Expression<Func<TParentDoc, object?>> parentField, FO operation = FO.Equal, object? defaultValue = null)
	{
		var parentNodes = Parents
			.Where(x =>
				x.Value.DataSourceType.GetDataSourceTDoc() == typeof(TParentDoc) ||
				x.Value.DataSourceType.GetDataSourceTSelect() == typeof(TParentDoc))
			.ToArray();

		if (parentNodes.Length > 1)
		{
			throw new InvalidOperationException($"There is more than one datasource with document type '{typeof(TDoc).ToPretty()}' in parent CDS nodes. Specify the parent node.");
		}
		else if (parentNodes.Length < 1)
		{
			throw new InvalidOperationException($"There is no datasource with document type '{typeof(TDoc).ToPretty()}' in parent CDS nodes.");
		}

		return AddCondition<TDoc, TParentDoc>(field, parentNodes[0].Value, parentField, operation, null);
	}
	public ICDSNodeInfo AddCondition<TDoc, TParentDoc>(Expression<Func<TDoc, object?>> field, ICDSNodeInfo parentNode, Expression<Func<TParentDoc, object?>> parentField, FO operation = FO.Equal, object? defaultValue = null)
	{
		if (DataSourceType.GetDataSourceTDoc() != typeof(TDoc) && DataSourceType.GetDataSourceTSelect() != typeof(TDoc))
		{
			throw new InvalidOperationException($"Specified document type '{typeof(TDoc).ToPretty()}' does not match the datasource document types of node '{Name}'.");
		}
		if (parentNode.DataSourceType.GetDataSourceTDoc() != typeof(TParentDoc) && parentNode.DataSourceType.GetDataSourceTSelect() != typeof(TParentDoc))
		{
			throw new InvalidOperationException($"Specified document type '{typeof(TParentDoc).ToPretty()}' does not match the datasource document types of parent node '{parentNode.Name}'.");
		}

		var cond = new CDSCondition(this.Name, typeof(TDoc), field.GetMemberName())
		{
			Operation = operation,
			OperandSourceType = OperandSourceType.Document,
			ParentDocType = typeof(TParentDoc),
			ParentNodeName = parentNode.Name,
			ParentFieldName = parentField.GetMemberName(),
			DefaultValue = defaultValue
		};

		AddCondition(cond);

		return this;
	}
	public ICDSNodeInfo AddCondition<TDoc>(Expression<Func<TDoc, object?>> field, object? constValue, FO operation = FO.Equal, object? defaultValue = null)
	{
		if (DataSourceType.GetDataSourceTDoc() != typeof(TDoc) || DataSourceType.GetDataSourceTSelect() != typeof(TDoc))
		{
			throw new InvalidOperationException($"Specified document type '{typeof(TDoc).ToPretty()}' does not match the datasource document types of node '{Name}'.");
		}

		var cond = new CDSCondition(this.Name, typeof(TDoc), field.GetMemberName())
		{
			Operation = operation,
			OperandSourceType = OperandSourceType.ConstValue,
			ConstValue = constValue,
			DefaultValue = defaultValue
		};

		AddCondition(cond);

		return this;
	}

	private void AddNode(ICDSNodeInfo node)
	{
		if (_children == null)
		{
			_children = new ICDSNodeInfo[] { node };
		}
		else
		{
			Array.Resize(ref _children, _children.Length + 1);
			_children[_children.Length - 1] = node;
		}
	}

	private void AddCondition(ICDSCondition cond)
	{
		if (_conditions == null)
		{
			_conditions = new ICDSCondition[] { cond };
		}
		else
		{
			Array.Resize(ref _conditions, _conditions.Length + 1);
			_conditions[_conditions.Length - 1] = cond;
		}
	}

	internal class NodeDictionary : IReadOnlyDictionary<string, ICDSNodeInfo>
	{
		public readonly IEnumerable<ICDSNodeInfo> Nodes;
		public NodeDictionary(IEnumerable<ICDSNodeInfo> nodes) => Nodes = nodes;
		public ICDSNodeInfo this[string key] => Nodes.First(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
		public IEnumerable<string> Keys => Nodes.Select(x => x.Name);
		public IEnumerable<ICDSNodeInfo> Values => Nodes;
		public int Count => (Nodes as ICDSNodeInfo[])?.Length ?? Nodes.Count();
		public bool ContainsKey(string key) => Nodes.Any(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
		public IEnumerator<KeyValuePair<string, ICDSNodeInfo>> GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Name, x)).GetEnumerator();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out ICDSNodeInfo value)
		{
			value = Nodes.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
			return value != null;
		}
		IEnumerator IEnumerable.GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Name, x)).GetEnumerator();
	}
}