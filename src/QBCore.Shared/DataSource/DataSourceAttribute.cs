namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DataSourceAttribute : Attribute
{
	public string Name { get; }
	public Type? ServiceInterface { get; init; }
	public Type? Listener { get; init; }
	public DataSourceOptions Options { get; init; }
	public bool IsServiceSingleton { get; init; }
	public string? DataContextName { get; init; }

	public DataSourceAttribute(string Name)
	{
		this.Name = Name;
	}
}