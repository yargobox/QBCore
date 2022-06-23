namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DataSourceAttribute : Attribute
{
	public string? Name { get; }
	public Type? ServiceInterface { get; init; }
	public Type? Listener { get; init; }
	public DataSourceOptions? Options { get; init; }
	public bool? IsServiceSingleton { get; init; }
	public string? DataContextName { get; init; }
	public Type? QBFactory { get; init; }

	public Type? Builder { get; init; }
	public string? BuilderMethod { get; init; }

	public DataSourceAttribute() { }
	public DataSourceAttribute(string Name)
	{
		this.Name = Name;
	}
	public DataSourceAttribute(Type QBFactory)
	{
		this.QBFactory = QBFactory;
	}
	public DataSourceAttribute(DataSourceOptions Options)
	{
		this.Options = Options;
	}
	public DataSourceAttribute(string Name, Type QBFactory)
	{
		this.Name = Name;
		this.QBFactory = QBFactory;
	}
	public DataSourceAttribute(string Name, DataSourceOptions Options)
	{
		this.Name = Name;
		this.Options = Options;
	}
	public DataSourceAttribute(Type QBFactory, DataSourceOptions Options)
	{
		this.QBFactory = QBFactory;
		this.Options = Options;
	}
	public DataSourceAttribute(string Name, Type QBFactory, DataSourceOptions Options)
	{
		this.Name = Name;
		this.QBFactory = QBFactory;
		this.Options = Options;
	}
}