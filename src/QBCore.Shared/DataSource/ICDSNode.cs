namespace QBCore.DataSource;

public interface ICDSNode
{
	string Name { get; }
	Type DataSourceType { get; }
	IEnumerable<ICDSCondition> Conditions { get; }
	bool Hidden { get; }
	ICDSNode? Parent { get; }
	ICDSNode Root { get; }
	IReadOnlyDictionary<string, ICDSNode> All { get; }
	IReadOnlyDictionary<string, ICDSNode> Parents { get; }
	IReadOnlyDictionary<string, ICDSNode> Children { get; }
}