using QBCore.DataSource.QueryBuilder;

namespace QBCore.DataSource;

public interface IDataLayerInfo
{
	string Name { get; }
	Type DatabaseContextInterface { get; }
	Func<Type, bool> IsDocumentType { get; set; }

	DSDocumentInfo CreateDocumentInfo(Type documentType);
	IQueryBuilderFactory CreateQBFactory(
		Type dataSourceConcrete,
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