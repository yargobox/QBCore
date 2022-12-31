namespace QBCore.Extensions.Internals;

public static class ExtensionsForEXDataSource
{
	public static NotSupportedException DataSourceDoesNotSupportOperation(this EX.DataSource _, string dataSource, string queryBuilderType)
		=> new NotSupportedException($"DataSource '{dataSource}' does not support the {queryBuilderType} operation.");
}