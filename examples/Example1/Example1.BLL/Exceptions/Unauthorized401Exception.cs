namespace Example1.BLL.Exceptions;

public class Unauthorized401Exception : ApplicationException
{
	public Unauthorized401Exception()
	{
	}

	public Unauthorized401Exception(string errorMessage) : base(errorMessage)
	{
	}
}