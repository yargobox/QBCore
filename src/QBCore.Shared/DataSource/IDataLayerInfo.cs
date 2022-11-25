using QBCore.DataSource.QueryBuilder;

namespace QBCore.DataSource;

public interface IDataLayerInfo
{
	string Name { get; }
	Type DataContextInterfaceType { get; }
	Type DataContextProviderInterfaceType { get; }
	Func<Type, bool> IsDocumentType { get; set; }
	Func<Type, string> GetDefaultDBSideContainerName { get; set; }

	DSDocumentInfo CreateDocumentInfo(Type documentType);
	IQueryBuilderFactory CreateQBFactory(
		DSTypeInfo dSTypeInfo,
		DataSourceOptions options,
		Delegate? insertBuilderMethod,
		Delegate? selectBuilderMethod,
		Delegate? updateBuilderMethod,
		Delegate? deleteBuilderMethod,
		Delegate? softDelBuilderMethod,
		Delegate? restoreBuilderMethod,
		bool lazyInitialization
	);
}