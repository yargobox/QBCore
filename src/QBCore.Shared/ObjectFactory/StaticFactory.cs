using QBCore.DataSource;

namespace QBCore.ObjectFactory;

public static class StaticFactory
{
	public static IFactoryObjectDictionary<Type, LazyObject<DSDocumentInfo>> Documents { get; } = new FactoryObjectRegistry<Type, LazyObject<DSDocumentInfo>>();
	public static IFactoryObjectDictionary<Type, IDSInfo> DataSources { get; } = new FactoryObjectRegistry<Type, IDSInfo>();
	public static IFactoryObjectDictionary<Type, ICDSInfo> ComplexDataSources { get; } = new FactoryObjectRegistry<Type, ICDSInfo>();
	public static IFactoryObjectDictionary<string, BusinessObject> BusinessObjects { get; } = new FactoryObjectRegistry<string, BusinessObject>();
}