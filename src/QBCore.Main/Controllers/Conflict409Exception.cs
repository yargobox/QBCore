namespace QBCore.Controllers;

public class Conflict409Exception : ApplicationException
{
	public Conflict409Exception()
	{
	}

	public Conflict409Exception(string errorMessage) : base(errorMessage)
	{
	}
}
