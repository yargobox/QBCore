namespace QBCore.DataSource;

/// <summary>
/// The Complex DataSource attribute to configure its name in the route.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CdsApiControllerAttribute : Attribute
{
	/// <summary>
	/// Complex DataSource controller name
	/// </summary>
	/// <remarks>
	/// You can use the placeholders "[CDS]" or "[CDS:guessPlural]" to specify the name of the complex datasource or its plural form, respectively.
	/// </remarks>
	public string Name { get; init; }

	public CdsApiControllerAttribute(string name = "[CDS:guessPlural]")
	{
		name = name?.Trim()!;

		if (name == null)
		{
			throw new ArgumentNullException($"{nameof(CdsApiControllerAttribute)}.{nameof(CdsApiControllerAttribute.Name)}");
		}
		if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('*'))
		{
			throw new ArgumentException($"{nameof(CdsApiControllerAttribute)}.{nameof(CdsApiControllerAttribute.Name)}");
		}

		Name = name;
	}
}