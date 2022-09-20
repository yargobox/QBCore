namespace QBCore.DataSource;

public interface ICDSNodeInfo
{
	string Name { get; }
	Type DataSourceType { get; }
	IDSInfo DSInfo { get; }
	IEnumerable<ICDSCondition> Conditions { get; }
	bool Hidden { get; }
	ICDSNodeInfo? Parent { get; }
	ICDSNodeInfo Root { get; }
	IReadOnlyDictionary<string, ICDSNodeInfo> All { get; }
	IReadOnlyDictionary<string, ICDSNodeInfo> Parents { get; }
	IReadOnlyDictionary<string, ICDSNodeInfo> Children { get; }
}