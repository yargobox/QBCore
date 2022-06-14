namespace QBCore.DataSource;

public sealed class EmptyDto
{
	public static readonly EmptyDto Empty = new EmptyDto();

	private EmptyDto() { }
}