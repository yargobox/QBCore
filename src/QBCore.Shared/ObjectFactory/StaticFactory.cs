using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;

namespace QBCore.ObjectFactory;

public static class StaticFactory
{
	public static IFactoryObjectDictionary<Type, IDSDocument> DocumentsPool { get; } = new FactoryObjectRegistry<Type, IDSDocument>();
	public static IFactoryObjectDictionary<Type, IDSDefinition> DataSources { get; } = new FactoryObjectRegistry<Type, IDSDefinition>();
	public static IFactoryObjectDictionary<Type, ICDSDefinition> ComplexDataSources { get; } = new FactoryObjectRegistry<Type, ICDSDefinition>();
	public static IFactoryObjectDictionary<string, BusinessObject> BusinessObjects { get; } = new FactoryObjectRegistry<string, BusinessObject>();
}