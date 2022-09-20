namespace QBCore.DataSource;

public interface ICDSBuilder
{
	Type ConcreteType { get; }
	string? Name { get; set; }
	string? ControllerName { get; set; }
	ICDSNodeBuilder NodeBuilder { get; }
}