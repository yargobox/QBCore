namespace QBCore.DataSource;

public enum DataSourceOptions
{
	None = 0,
	CanTestInsert = 1,
	CanInsert = 2,
	CanSelect = 4,
	CanTestUpdate = 8,
	CanUpdate = 0x10,
	CanTestDelete = 0x20,
	CanDelete = 0x40,
	CanTestRestore = 0x80,
	CanRestore = 0x0100,
	SoftDelete = 0x0200,
	CompositeId = 0x0400,
	CompoundId = 0x0800,
	SingleRecord = 0x1000,
	FewRecords = 0x2000,
	IdentityInsertOn = 0x4000,
	SelectLastIdentity = 0x8000,
	RefreshAfterInsert = 0x010000,
	RefreshAfterUpdate = 0x020000,
	RefreshAfterDelete = 0x040000,
	RefreshAfterRestore = 0x080000
}