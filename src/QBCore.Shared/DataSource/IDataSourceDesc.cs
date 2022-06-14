namespace QBCore.DataSource;

public interface IDataSourceDesc
{
	string Name { get; }
	Type IdType { get; }
	Type DocumentType { get; }
	Type CreateDocumentType { get; }
	Type SelectDocumentType { get; }
	Type UpdateDocumentType { get; }
	Type DeleteDocumentType { get; }
	Type DataSourceConcreteType { get; }
	Type DataSourceInterfaceType { get; }
	Type DataSourceServiceType { get; }
	DataSourceOptions Options { get; }
	bool IsServiceSingleton { get; }
	Type DatabaseContextInterfaceType { get; }
	string DataContextName { get; }
	string? ControllerName { get; }
}