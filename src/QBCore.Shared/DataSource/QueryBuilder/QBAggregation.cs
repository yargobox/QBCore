namespace QBCore.DataSource.QueryBuilder;

public record QBAggregation
{
	public readonly DataEntryPath Field;
	public readonly AggregationOperations Operation;

	public QBAggregation(DataEntryPath Field, AggregationOperations Operation)
	{
		this.Field = Field;
		this.Operation = Operation;
	}
}