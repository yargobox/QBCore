namespace QBCore.DataSource.QueryBuilder;

[Flags]
public enum ContainerOperations
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
	Unwind = 0x0100,

	MainMask = ContainerOperations.Insert | ContainerOperations.Select | ContainerOperations.Update | ContainerOperations.Delete | ContainerOperations.Exec,
	SlaveMask = ContainerOperations.Join | ContainerOperations.LeftJoin | ContainerOperations.CrossJoin | ContainerOperations.Unwind
}