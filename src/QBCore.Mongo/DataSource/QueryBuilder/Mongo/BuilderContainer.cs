namespace QBCore.DataSource.QueryBuilder.Mongo;

internal record BuilderContainer
(
	Type DocumentType,
	string Name,
	string DBSideName,
	BuilderContainerTypes ContainerType,
	BuilderContainerOperations ContainerOperation,
	string? ConnectTemplate
);