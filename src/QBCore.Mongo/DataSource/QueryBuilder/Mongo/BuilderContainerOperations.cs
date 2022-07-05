namespace QBCore.DataSource.QueryBuilder.Mongo;

[Flags]
internal enum BuilderContainerOperations
{
	None = 0,
	Insert = 1,
	Select = 2,
	Update = 4,
	Delete = 8,
	Exec = 0x10,
	Join = 0x20,
	LeftJoin = 0x40,
	CrossJoin = 0x80,

	MainMask = BuilderContainerOperations.Insert | BuilderContainerOperations.Select | BuilderContainerOperations.Update | BuilderContainerOperations.Delete | BuilderContainerOperations.Exec,
	SlaveMask = BuilderContainerOperations.Join | BuilderContainerOperations.LeftJoin
}