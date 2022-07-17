namespace QBCore.DataSource.Options;

public class DataSourceOperationOptions
{
	public object? NativeOptions { get; set; }
	public object? NativeClientSession { get; set; }
	public Action<string>? GetQueryString;
}

public class DataSourceCountOptions : DataSourceOperationOptions
{
	public Action<object>? GetNativeQuery;
	public object? PreparedNativeQuery;
}
public class DataSourceInsertOptions : DataSourceOperationOptions { }
public class DataSourceQueryableOptions : DataSourceOperationOptions { }
public class DataSourceSelectOptions : DataSourceOperationOptions
{
	public Action<object>? GetNativeQuery;
	public object? PreparedNativeQuery;
}
public class DataSourceUpdateOptions : DataSourceOperationOptions { }
public class DataSourceDeleteOptions : DataSourceOperationOptions { }
public class DataSourceRestoreOptions : DataSourceOperationOptions { }