namespace QBCore.DataSource;

public interface ICDSDefinition
{
	string Name { get; }
	Type ComplexDataSourceType { get; }
	IReadOnlyDictionary<string, ICDSNode> Nodes { get; }
}