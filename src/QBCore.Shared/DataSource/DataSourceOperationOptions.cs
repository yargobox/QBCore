namespace QBCore.DataSource.Options;

public class DataSourceOperationOptions
{
	public object? NativeOptions { get; set; }
	public object? NativeClientSession { get; set; }
}

public class DataSourceCountOptions : DataSourceOperationOptions { }
public class DataSourceInsertOptions : DataSourceOperationOptions { }
public class DataSourceQueryableOptions : DataSourceOperationOptions { }
public class DataSourceSelectOptions : DataSourceOperationOptions { }
public class DataSourceUpdateOptions : DataSourceOperationOptions { }
public class DataSourceDeleteOptions : DataSourceOperationOptions { }
public class DataSourceRestoreOptions : DataSourceOperationOptions { }