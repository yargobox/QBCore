using System.Data.Common;
using QBCore.Configuration;

namespace QBCore.DataSource.Options;

public class DataSourceOperationOptions
{
	public object? NativeOptions;
	public object? NativeClientSession;
	public Action<string>? QueryStringCallback;
	public Func<string, Task>? QueryStringCallbackAsync;
	public DbConnection? Connection;
	public DbTransaction? Transaction;
}

public class DataSourceIdGeneratorOptions : DataSourceOperationOptions { }

public class DataSourceCountOptions : DataSourceOperationOptions
{
	public object? NativeSelectQuery;
	public long Skip;
	public long CountNoMoreThan = -1;
}
public class DataSourceInsertOptions : DataSourceOperationOptions
{
	public DataSourceIdGeneratorOptions? GeneratorOptions;
}
public class DataSourceQueryableOptions : DataSourceOperationOptions { }
public class DataSourceSelectOptions : DataSourceOperationOptions
{
	public bool ObtainLastPageMarker;
	public Action<object>? NativeSelectQueryCallback;
}
public class DataSourceUpdateOptions : DataSourceOperationOptions
{
	public bool FetchResultDocument;
}
public class DataSourceDeleteOptions : DataSourceOperationOptions { }
public class DataSourceRestoreOptions : DataSourceOperationOptions { }