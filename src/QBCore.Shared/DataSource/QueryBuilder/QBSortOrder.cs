namespace QBCore.DataSource.QueryBuilder;

public record QBSortOrder
{
	public readonly DataEntryPath Field;
	public readonly bool Descending;

	public QBSortOrder(DataEntryPath Field, bool Descending)
	{
		this.Field = Field;
		this.Descending = Descending;
	}
}