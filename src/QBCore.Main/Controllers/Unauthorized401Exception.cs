namespace QBCore.Controllers;

public class Unauthorized401Exception : ApplicationException
{
	public Unauthorized401Exception()
	{
	}

	public Unauthorized401Exception(string errorMessage) : base(errorMessage)
	{
	}
}