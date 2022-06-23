using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using QBCore.Extensions.Collections.Generic;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

internal sealed class CDSNode : ICDSNode
{
	private static readonly IReadOnlyDictionary<string, ICDSNode> _emptyReadOnlyDictionary = new Dictionary<string, ICDSNode>();
	private const string _rootName = "<ROOT>";

	private readonly OrderedDictionary<string, ICDSNode> _collection;
	private ICDSNode[]? _children;
	private ICDSCondition[]? _conditions;

	public string Name { get; }
	public Type DataSourceType { get; }
	public IEnumerable<ICDSCondition> Conditions => _conditions ?? Enumerable.Empty<ICDSCondition>();
	public bool Hidden { get; set; }
	public ICDSNode? Parent { get; }
	public ICDSNode Root => _collection.Values.First();
	public IReadOnlyDictionary<string, ICDSNode> All => _collection;
	public IReadOnlyDictionary<string, ICDSNode> Parents => Parent != null ? new NodeDictionary(GetParents(this)) : _emptyReadOnlyDictionary;
	public IReadOnlyDictionary<string, ICDSNode> Children => _children != null ? new NodeDictionary(_children) : _emptyReadOnlyDictionary;

	// root node ctor to start collection
	public CDSNode()
	{
		_collection = new OrderedDictionary<string, ICDSNode>(StringComparer.OrdinalIgnoreCase);
		Name = _rootName;
		DataSourceType = typeof(void);
	}

	// regular node ctor
	public CDSNode(OrderedDictionary<string, ICDSNode> collection, Type dataSourceType, string name, ICDSNode? parent)
	{
		_collection = collection;
		Parent = parent;
		Name = name;
		DataSourceType = dataSourceType;
	}

	private IEnumerable<ICDSNode> GetParents(ICDSNode node)
	{
		while (node.Parent != null)
		{
			yield return node.Parent;
			node = node.Parent;
		}
	}

	public ICDSNode AddNode(Type dataSourceConcreteType, string name)
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
			throw new InvalidOperationException($"Invalid data source type {dataSourceConcreteType.ToPretty()}.");
		}
		if (dataSourceConcreteType.GetInterfaceOf(typeof(IDataSource<,,,,,,>)) == null)
		{
			throw new InvalidOperationException($"Invalid data source type {dataSourceConcreteType.ToPretty()}.");
		}

		var node = new CDSNode(_collection, dataSourceConcreteType, name, Name != _rootName ? this : null);
		_collection.Add(name, node);

		AddNode(node);

		return node;
	}
	public ICDSNode AddCondition<TDocument, TParentDocument>(Expression<Func<TDocument, object?>> field, Expression<Func<TParentDocument, object?>> parentField, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null)
	{
		var parentNodes = Parents
			.Where(x =>
				x.Value.DataSourceType.GetDataSourceTDocument() == typeof(TParentDocument) ||
				x.Value.DataSourceType.GetDataSourceTSelect() == typeof(TParentDocument))
			.ToArray();

		if (parentNodes.Length > 1)
		{
			throw new InvalidOperationException($"There is more than one datasource with document type '{typeof(TDocument).ToPretty()}' in parent CDS nodes. Specify the parent node.");
		}
		else if (parentNodes.Length < 1)
		{
			throw new InvalidOperationException($"There is no datasource with document type '{typeof(TDocument).ToPretty()}' in parent CDS nodes.");
		}

		return AddCondition<TDocument, TParentDocument>(field, parentNodes[0].Value, parentField, operation, null);
	}
	public ICDSNode AddCondition<TDocument, TParentDocument>(Expression<Func<TDocument, object?>> field, ICDSNode parentNode, Expression<Func<TParentDocument, object?>> parentField, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null)
	{
		if (DataSourceType.GetDataSourceTDocument() != typeof(TDocument) && DataSourceType.GetDataSourceTSelect() != typeof(TDocument))
		{
			throw new InvalidOperationException($"Specified document type '{typeof(TDocument).ToPretty()}' does not match the datasource document types of node '{Name}'.");
		}
		if (parentNode.DataSourceType.GetDataSourceTDocument() != typeof(TParentDocument) && parentNode.DataSourceType.GetDataSourceTSelect() != typeof(TParentDocument))
		{
			throw new InvalidOperationException($"Specified document type '{typeof(TParentDocument).ToPretty()}' does not match the datasource document types of parent node '{parentNode.Name}'.");
		}

		var cond = new CDSCondition(this.Name, typeof(TDocument), field.GetMemberName())
		{
			Operation = operation,
			OperandSourceType = OperandSourceType.Document,
			ParentDocumentType = typeof(TParentDocument),
			ParentNodeName = parentNode.Name,
			ParentFieldName = parentField.GetMemberName(),
			DefaultValue = defaultValue
		};

		AddCondition(cond);

		return this;
	}
	public ICDSNode AddCondition<TDocument>(Expression<Func<TDocument, object?>> field, object? constValue, ConditionOperations operation = ConditionOperations.Equal, object? defaultValue = null)
	{
		if (DataSourceType.GetDataSourceTDocument() != typeof(TDocument) || DataSourceType.GetDataSourceTSelect() != typeof(TDocument))
		{
			throw new InvalidOperationException($"Specified document type '{typeof(TDocument).ToPretty()}' does not match the datasource document types of node '{Name}'.");
		}

		var cond = new CDSCondition(this.Name, typeof(TDocument), field.GetMemberName())
		{
			Operation = operation,
			OperandSourceType = OperandSourceType.ConstValue,
			ConstValue = constValue,
			DefaultValue = defaultValue
		};

		AddCondition(cond);

		return this;
	}

	private void AddNode(ICDSNode node)
	{
		if (_children == null)
		{
			_children = new ICDSNode[] { node };
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

	internal class NodeDictionary : IReadOnlyDictionary<string, ICDSNode>
	{
		public readonly IEnumerable<ICDSNode> Nodes;
		public NodeDictionary(IEnumerable<ICDSNode> nodes) => Nodes = nodes;
		public ICDSNode this[string key] => Nodes.First(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
		public IEnumerable<string> Keys => Nodes.Select(x => x.Name);
		public IEnumerable<ICDSNode> Values => Nodes;
		public int Count => (Nodes as ICDSNode[])?.Length ?? Nodes.Count();
		public bool ContainsKey(string key) => Nodes.Any(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
		public IEnumerator<KeyValuePair<string, ICDSNode>> GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Name, x)).GetEnumerator();
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out ICDSNode value)
		{
			value = Nodes.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
			return value != null;
		}
		IEnumerator IEnumerable.GetEnumerator()
			=> Nodes.Select(x => KeyValuePair.Create(x.Name, x)).GetEnumerator();
	}
}