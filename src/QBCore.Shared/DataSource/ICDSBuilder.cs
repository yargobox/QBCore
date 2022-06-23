namespace QBCore.DataSource;

public interface ICDSBuilder
{
	Type ConcreteType { get; }
	string? Name { get; set; }
	ICDSNodeBuilder NodeBuilder { get; }
}