namespace QBCore.DataSource.QueryBuilder;

public record QBAggregation
{
	public readonly FieldPath Field;
	public readonly AggregationOperations Operation;

	public QBAggregation(FieldPath Field, AggregationOperations Operation)
	{
		this.Field = Field;
		this.Operation = Operation;
	}
}