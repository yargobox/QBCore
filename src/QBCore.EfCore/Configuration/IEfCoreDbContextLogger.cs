namespace QBCore.Configuration;

public interface IEfCoreDbContextLogger
{
	public event Action<string>? QueryStringCallback;
}