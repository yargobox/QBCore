namespace QBCore.DataSource.QueryBuilder;

public record QBSortOrder
{
	public readonly FieldPath Field;
	public readonly bool Descending;

	public QBSortOrder(FieldPath Field, bool Descending)
	{
		this.Field = Field;
		this.Descending = Descending;
	}
}