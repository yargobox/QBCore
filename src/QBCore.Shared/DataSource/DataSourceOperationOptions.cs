namespace QBCore.DataSource.Options;

public class DataSourceOperationOptions
{
	public object? NativeOptions { get; set; }
}

public class DataSourceCountOptions : DataSourceOperationOptions { }
public class DataSourceTestInsertOptions : DataSourceOperationOptions { }
public class DataSourceInsertOptions : DataSourceOperationOptions { }
public class DataSourceSelectOptions : DataSourceOperationOptions { }
public class DataSourceTestUpdateOptions : DataSourceOperationOptions { }
public class DataSourceUpdateOptions : DataSourceOperationOptions { }
public class DataSourceTestDeleteOptions : DataSourceOperationOptions { }
public class DataSourceDeleteOptions : DataSourceOperationOptions { }
public class DataSourceRestoreOptions : DataSourceOperationOptions { }
public class DataSourceTestRestoreOptions : DataSourceOperationOptions { }