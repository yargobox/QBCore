namespace QBCore.DataSource;

public interface ICDSDefinition
{
	string Name { get; }
	Type ComplexDataSourceConcrete { get; }
	IReadOnlyDictionary<string, ICDSNode> Nodes { get; }
}