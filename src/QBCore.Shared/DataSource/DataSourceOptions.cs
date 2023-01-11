namespace QBCore.DataSource;

[Flags]
public enum DataSourceOptions : ulong
{
	None = 0,
	CanInsert = 1,
	CanSelect = 2,
	CanUpdate = 4,
	CanDelete = 8,
	CanRestore = 0x10,
	SoftDelete = 0x20,
	CompositeId = 0x40,
	CompoundId = 0x80,
	SingleRecord = 0x100,
	FewRecords = 0x200,
	IdentityInsertOn = 0x400,
	SelectLastIdentity = 0x800,
	RefreshAfterInsert = 0x1000,
	RefreshAfterUpdate = 0x2000,
	RefreshAfterDelete = 0x4000,
	RefreshAfterRestore = 0x8000
}