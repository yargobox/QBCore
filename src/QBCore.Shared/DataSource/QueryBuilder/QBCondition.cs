namespace QBCore.DataSource.QueryBuilder;

public class QBCondition
{
	public string FieldName { get; }
	public ConditionOperations Operation { get; }
	public object? ConstValue { get; }

	public QBCondition(string fieldName, ConditionOperations operation, object? constValue)
	{
		FieldName = fieldName;
		Operation = operation;
	}
}