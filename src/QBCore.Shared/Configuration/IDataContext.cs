namespace QBCore.Configuration;

public interface IDataContext
{
	string Name { get; }
	Type ContextType { get; }
	IReadOnlyDictionary<string, object?>? Args { get; }

	object Context { get; }
}