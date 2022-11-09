namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DsListenersAttribute : Attribute
{
	public Type[] Types { get; init; }

	public DsListenersAttribute(params Type[] listenerTypes)
	{
		Types = listenerTypes;
	}
}