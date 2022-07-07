namespace QBCore.DataSource.QueryBuilder;

public class QBArgument
{
	public string FieldName { get; }
	public object? Value { get; }

	public QBArgument(string fieldName, object? value)
	{
		FieldName = fieldName;
		Value = value;
	}
}