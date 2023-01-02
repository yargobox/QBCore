using QBCore.DataSource;

namespace QBCore.Configuration;

/// <summary>
/// Data Context
/// </summary>
public interface IDataContext
{
	/// <summary>
	/// Data Context Name
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Data context optional (tenant) arguments
	/// </summary>
	IReadOnlyDictionary<string, object?>? Args { get; }

	/// <summary>
	/// Data Context Instance
	/// </summary>
	object Context { get; }
}

public interface IDataContextProvider
{
	IEnumerable<DataContextInfo> Infos { get; }

	IDataContext GetDataContext(string dataContextName = "default");
}

public abstract class DataContext : IDataContext
{
	public string Name => _dataContextName;
	public IReadOnlyDictionary<string, object?>? Args => _args;
	public object Context => _context ?? throw new ObjectDisposedException(GetType().Name);
	public bool IsDisposed => _context == null;

	private object? _context;
	private string _dataContextName;
	private IReadOnlyDictionary<string, object?>? _args;

	public DataContext(object context, string dataContextName = "default", IReadOnlyDictionary<string, object?>? args = null)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));
		if (dataContextName == null) throw new ArgumentNullException(nameof(dataContextName));

		_context = context;
		_dataContextName = dataContextName;
		_args = args?.Count > 0 ? args : null;
	}
}

public record DataContextInfo
{
	/// <summary>
	/// Data Context Name
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Data Layer
	/// </summary>
	public IDataLayerInfo DataLayer => _funcIDataLayerInfo();

	private readonly Func<IDataLayerInfo> _funcIDataLayerInfo;

	public DataContextInfo(string name, Func<IDataLayerInfo> getDataLayer)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		_funcIDataLayerInfo = getDataLayer ?? throw new ArgumentNullException(nameof(getDataLayer));
	}
}