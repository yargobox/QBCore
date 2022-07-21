namespace QBCore.DataSource.QueryBuilder;

[Flags]
public enum ContainerTypes
{
	None = 0,
	Table = 1,
	View = 2,
	Function = 4,
	Procedure = 8
}