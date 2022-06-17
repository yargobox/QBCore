using System.Linq.Expressions;
using QBCore.DataSource.Builders;

namespace QBCore.DataSource;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ComplexDataSourceAttribute : Attribute
{
	public string Name { get; init; } = "[CDS]";
	public Type? Builder { get; init; }
	public string? BuilderMethod { get; init; }

	public ComplexDataSourceAttribute() { }
	public ComplexDataSourceAttribute(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException(nameof(name));
		}
		Name = name;
	}
}