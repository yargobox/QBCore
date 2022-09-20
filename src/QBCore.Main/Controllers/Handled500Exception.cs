namespace QBCore.Controllers;

public class Handled500Exception : ApplicationException
{
	public Handled500Exception()
	{
	}

	public Handled500Exception(string errorMessage) : base(errorMessage)
	{
	}
}