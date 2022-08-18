namespace QBCore.DataSource.QueryBuilder;

public record QBSortOrder
{
	public readonly DEPath Field;
	public readonly bool Descending;

	public QBSortOrder(DEPath Field, bool Descending)
	{
		this.Field = Field;
		this.Descending = Descending;
	}
}