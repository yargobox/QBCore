using QBCore.DataSource.QueryBuilder;

namespace QBCore.DataSource;

public interface IDSInfo
{
	string Name { get; }

	Type Key { get; }
	Type Document { get; }
	Type CreateDocument { get; }
	Type SelectDocument { get; }
	Type UpdateDocument { get; }
	Type DeleteDocument { get; }
	Type RestoreDocument { get; }

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