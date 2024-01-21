using QBCore.DataSource.QueryBuilder;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSInfo : IAppObjectInfo
{
	DSTypeInfo DSTypeInfo { get; }
	Type DataSourceServiceType { get; }

	Lazy<DSDocumentInfo> DocInfo { get; }
	Lazy<DSDocumentInfo>? CreateInfo { get; }
	Lazy<DSDocumentInfo>? SelectInfo { get; }
	Lazy<DSDocumentInfo>? UpdateInfo { get; }
	Lazy<DSDocumentInfo>? DeleteInfo { get; }
	Lazy<DSDocumentInfo>? RestoreInfo { get; }

	DataSourceOptions Options { get; }

	string DataContextName { get; }

	IQueryBuilderFactory QBFactory { get; }
	Func<IServiceProvider, IDataSourceListener>[]? Listeners { get; }

	bool IsServiceSingleton { get; }

	bool BuildAutoController { get; }
}