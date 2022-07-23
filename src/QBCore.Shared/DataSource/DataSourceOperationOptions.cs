namespace QBCore.DataSource.Options;

public class DataSourceOperationOptions
{
	public object? NativeOptions;
	public object? NativeClientSession;
	public Action<string>? GetQueryString;
}

public class DataSourceCountOptions : DataSourceOperationOptions
{
}
public class DataSourceInsertOptions : DataSourceOperationOptions { }
public class DataSourceQueryableOptions : DataSourceOperationOptions { }
public class DataSourceSelectOptions : DataSourceOperationOptions
{
	public bool ObtainEOF;
	public bool ObtainTotalCount;
}
public class DataSourceUpdateOptions : DataSourceOperationOptions { }
public class DataSourceDeleteOptions : DataSourceOperationOptions { }
public class DataSourceRestoreOptions : DataSourceOperationOptions { }