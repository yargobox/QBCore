namespace QBCore.DataSource.Options;

public class DataSourceOperationOptions
{
	public object? NativeOptions;
	public object? NativeClientSession;
	public Action<string>? QueryStringCallback;
	public Func<string, Task>? QueryStringAsyncCallback;
}

public class DataSourceIdGeneratorOptions : DataSourceOperationOptions { }

public class DataSourceCountOptions : DataSourceOperationOptions
{
	public object? NativeSelectQuery;
	public long Skip;
	public long CountNoMoreThan = -1;
}
public class DataSourceInsertOptions : DataSourceOperationOptions { }
public class DataSourceQueryableOptions : DataSourceOperationOptions { }
public class DataSourceSelectOptions : DataSourceOperationOptions
{
	public bool ObtainLastPageMarker;
	public Action<object>? NativeSelectQueryCallback;
}
public class DataSourceUpdateOptions : DataSourceOperationOptions { }
public class DataSourceDeleteOptions : DataSourceOperationOptions { }
public class DataSourceRestoreOptions : DataSourceOperationOptions { }