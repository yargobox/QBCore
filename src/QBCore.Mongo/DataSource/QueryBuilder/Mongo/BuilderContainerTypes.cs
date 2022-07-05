namespace QBCore.DataSource.QueryBuilder.Mongo;

[Flags]
internal enum BuilderContainerTypes
{
	None = 0,
	Table = 1,
	View = 2,
	Function = 4,
	Procedure = 8
}