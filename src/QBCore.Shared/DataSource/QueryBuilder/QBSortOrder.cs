namespace QBCore.DataSource.QueryBuilder;

public record QBSortOrder
{
	public readonly string Alias;
	public readonly DEPath Field;
	public readonly SO SortOrder;

	public QBSortOrder(string alias, DEPath Field, SO SortOrder)
	{
		if (alias == null)
		{
			throw new ArgumentNullException(nameof(alias));
		}
		if (Field == null)
		{
			throw new ArgumentNullException(nameof(Field));
		}

		this.Alias = alias;
		this.Field = Field;
		this.SortOrder = SortOrder;
	}
}