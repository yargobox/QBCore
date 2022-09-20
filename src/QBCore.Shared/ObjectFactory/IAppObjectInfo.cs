namespace QBCore.ObjectFactory;

public interface IAppObjectInfo
{
	string Name { get; }
	string Tech { get; }
	Type ConcreteType { get; }
	string? ControllerName { get; }
}