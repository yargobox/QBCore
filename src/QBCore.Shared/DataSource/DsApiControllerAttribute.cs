namespace QBCore.DataSource;

/// <summary>
/// DataSource attribute to configure its API controller
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DsApiControllerAttribute : Attribute
{
	/// <summary>
	/// DataSource controller name
	/// </summary>
	/// <remarks>
	/// You can use the placeholders "[DS]" or "[DS:guessPlural]" to specify the name of the datasource or its plural form, respectively.
	/// </remarks>
	public string Name { get; init; }

	/// <summary>
	/// Whether or not to build a generic datasource controller for the datasource. True by default.
	/// </summary>
	public bool IsAutoController { get; init; } = true;

	public DsApiControllerAttribute(string name = "[DS:guessPlural]")
	{
		name = name?.Trim()!;

		if (name == null)
		{
			throw new ArgumentNullException($"{nameof(DsApiControllerAttribute)}.{nameof(DsApiControllerAttribute.Name)}");
		}
		if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('*'))
		{
			throw new ArgumentException($"{nameof(DsApiControllerAttribute)}.{nameof(DsApiControllerAttribute.Name)}");
		}

		Name = name;
	}
}