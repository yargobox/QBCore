namespace QBCore.DataSource.QueryBuilder.Mongo;

internal record BuilderContainer
(
	Type DocumentType,
	string Alias,
	string DBSideName,
	BuilderContainerTypes ContainerType,
	BuilderContainerOperations ContainerOperation
);