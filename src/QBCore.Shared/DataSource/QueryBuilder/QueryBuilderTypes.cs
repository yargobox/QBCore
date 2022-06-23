namespace QBCore.DataSource.QueryBuilder;

[Flags]
public enum QueryBuilderTypes
{
	None = 0,
	Insert = 1,
	Select = 2,
	Update = 4,
	Delete = 8,
	SoftDel = 16,
	Restore = 32
}