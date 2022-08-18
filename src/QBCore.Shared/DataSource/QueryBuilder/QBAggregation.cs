namespace QBCore.DataSource.QueryBuilder;

public record QBAggregation
{
	public readonly DEPath Field;
	public readonly AggregationOperations Operation;

	public QBAggregation(DEPath Field, AggregationOperations Operation)
	{
		this.Field = Field;
		this.Operation = Operation;
	}
}