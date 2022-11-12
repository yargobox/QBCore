using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class DataContext : IDataContext
{
	public string Name { get; }
	public Type ContextType => Context.GetType();
	public IReadOnlyDictionary<string, object?>? Args { get; }
	public object Context { get; }

	public DataContext(object context, string name, IReadOnlyDictionary<string, object?>? args = null)
	{
		Context = context;
		Name = name;
		Args = args;
	}
}