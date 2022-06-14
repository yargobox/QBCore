namespace QBCore.ObjectFactory;

public record Origin
{
	public Type Type { get; }
	public string? Name { get; }

	public Origin(Type type, string? name = null)
	{
		Type = type;
		Name = name;
	}
}