namespace QBCore.DataSource.QueryBuilder;

public class QBParameter
{
	public string FieldName { get; }
	public object? Value { get; }

	public QBParameter(string fieldName, object? value)
	{
		FieldName = fieldName;
		Value = value;
	}
}