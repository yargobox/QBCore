using QBCore.DataSource.QueryBuilder;

namespace QBCore.DataSource;

public interface IDSInfo
{
	string Name { get; }

	DSTypeInfo DSTypeInfo { get; }
	Type DataSourceServiceType { get; }

	Lazy<DSDocumentInfo> DocumentInfo { get; }
	Lazy<DSDocumentInfo>? CreateInfo { get; }
	Lazy<DSDocumentInfo>? SelectInfo { get; }
	Lazy<DSDocumentInfo>? UpdateInfo { get; }
	Lazy<DSDocumentInfo>? DeleteInfo { get; }
	Lazy<DSDocumentInfo>? RestoreInfo { get; }

	DataSourceOptions Options { get; }

	string DataContextName { get; }

	IQueryBuilderFactory QBFactory { get; }
	Func<IServiceProvider, IDataSourceListener>? ListenerFactory { get; }

	bool IsServiceSingleton { get; }

	string? ControllerName { get; }
	bool? IsAutoController { get; }
}