namespace QBCore.DataSource.QueryBuilder;

public class QBCondition
{
	public string FieldName { get; }
	public FO Operation { get; }
	public object? ConstValue { get; }

	public QBCondition(string fieldName, FO operation, object? constValue)
	{
		FieldName = fieldName;
		Operation = operation;
	}
}