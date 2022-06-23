namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ComplexDataSourceAttribute : Attribute
{
	public string? Name { get; init; }
	public Type? Builder { get; init; }
	public string? BuilderMethod { get; init; }

	public ComplexDataSourceAttribute() { }
	public ComplexDataSourceAttribute(string Name)
	{
		this.Name = Name;
	}
}