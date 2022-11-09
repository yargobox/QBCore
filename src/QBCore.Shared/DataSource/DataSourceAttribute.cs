namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DataSourceAttribute : Attribute
{
	public string? Name { get; }
	public Type? ServiceInterface { get; init; }
	public DataSourceOptions? Options { get; init; }
	public bool? IsServiceSingleton { get; init; }
	public string? DataContextName { get; init; }
	public Type? DataLayer { get; init; }

	public Type? Builder { get; init; }
	public string? BuilderMethod { get; init; }

	public DataSourceAttribute() { }
	public DataSourceAttribute(string Name)
	{
		this.Name = Name;
	}
	public DataSourceAttribute(Type DataLayer)
	{
		this.DataLayer = DataLayer;
	}
	public DataSourceAttribute(DataSourceOptions Options)
	{
		this.Options = Options;
	}
	public DataSourceAttribute(string Name, Type DataLayer)
	{
		this.Name = Name;
		this.DataLayer = DataLayer;
	}
	public DataSourceAttribute(string Name, DataSourceOptions Options)
	{
		this.Name = Name;
		this.Options = Options;
	}
	public DataSourceAttribute(Type DataLayer, DataSourceOptions Options)
	{
		this.DataLayer = DataLayer;
		this.Options = Options;
	}
	public DataSourceAttribute(string Name, Type DataLayer, DataSourceOptions Options)
	{
		this.Name = Name;
		this.DataLayer = DataLayer;
		this.Options = Options;
	}
}