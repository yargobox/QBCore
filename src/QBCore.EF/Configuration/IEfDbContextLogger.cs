namespace QBCore.Configuration;

public interface IEfDbContextLogger
{
	public event Action<string>? QueryStringCallback;
}