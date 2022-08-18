namespace QBCore.DataSource;

public interface ICDSInfo
{
	string Name { get; }
	Type ComplexDataSourceConcrete { get; }
	IReadOnlyDictionary<string, ICDSNode> Nodes { get; }
}