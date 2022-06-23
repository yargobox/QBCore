namespace QBCore.DataSource.Builders;

public interface ICDSBuilder
{
	Type ConcreteType { get; }
	string? Name { get; set; }
	ICDSNodeBuilder NodeBuilder { get; }
}