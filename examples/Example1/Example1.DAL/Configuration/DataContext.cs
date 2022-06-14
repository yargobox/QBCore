using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class DataContext : IDataContext
{
	public object Context { get; }
	public KeyValuePair<string, object?>[]? TenantArgs { get; }

	public DataContext(object context, KeyValuePair<string, object?>[]? tenantArgs)
	{
		Context = context;
		TenantArgs = tenantArgs;
	}
}