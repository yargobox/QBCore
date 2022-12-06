using QBCore.DataSource;

namespace QBCore.ObjectFactory.Internals;

public static class DataSourceStaticHelper
{
	public static IDSInfo CreateDSInfo(Type concreteType) => new DSInfo(concreteType);
	public static ICDSInfo CreateCDSInfo(Type concreteType) => new CDSInfo(concreteType);
}