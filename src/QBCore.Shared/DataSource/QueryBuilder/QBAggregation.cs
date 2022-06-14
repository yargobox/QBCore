namespace QBCore.DataSource.QueryBuilder;

public class QBAggregation
{
	public string FieldName { get; }
	public DSAggregationOperations Operation { get; }

	public QBAggregation(string fieldName, DSAggregationOperations operation)
	{
		FieldName = fieldName;
		Operation = operation;
	}
}