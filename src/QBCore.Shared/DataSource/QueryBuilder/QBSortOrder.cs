namespace QBCore.DataSource.QueryBuilder;

public class QBSortOrder
{
	public string FieldName { get; }
	public bool Descending { get; }

	public QBSortOrder(string fieldName, bool descending)
	{
		FieldName = fieldName;
		Descending = descending;
	}
}