using QBCore.DataSource.QueryBuilder;

namespace QBCore.DataSource;

public interface IDSInfo
{
	string Name { get; }

	Type KeyType { get; }
	Type DocumentType { get; }
	Type CreateType { get; }
	Type SelectType { get; }
	Type UpdateType { get; }
	Type DeleteType { get; }
	Type RestoreType { get; }

	Lazy<DSDocumentInfo> DocumentInfo { get; }
	Lazy<DSDocumentInfo>? CreateInfo { get; }
	Lazy<DSDocumentInfo>? SelectInfo { get; }
	Lazy<DSDocumentInfo>? UpdateInfo { get; }
	Lazy<DSDocumentInfo>? DeleteInfo { get; }
	Lazy<DSDocumentInfo>? RestoreInfo { get; }

	Type DataSourceConcrete { get; }
	Type DataSourceInterface { get; }
	Type DataSourceService { get; }

	DataSourceOptions Options { get; }

	string DataContextName { get; }

	IQueryBuilderFactory QBFactory { get; }
	Func<IServiceProvider, IDataSourceListener>? ListenerFactory { get; }

	bool IsServiceSingleton { get; }

	string? ControllerName { get; }
	bool? IsAutoController { get; }
}