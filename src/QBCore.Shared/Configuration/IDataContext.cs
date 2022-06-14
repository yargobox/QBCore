namespace QBCore.Configuration;

public interface IDataContext
{
	object Context { get; }
	KeyValuePair<string, object?>[]? TenantArgs { get; }
}