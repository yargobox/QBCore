namespace QBCore.DataSource;

[Flags]
public enum AggregationOperations
{
	None = 0,
	Sum = 1,
	Min = 2,
	Max = 4,
	Count = 8
}