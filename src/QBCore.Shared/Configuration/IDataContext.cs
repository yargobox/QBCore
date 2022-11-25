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
	/// Data Context Interface Type
	/// </summary>
	Type InterfaceType { get; }

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

public abstract class DataContext : IDataContext, IDisposable
{
	public string Name => _dataContextName;
	public Type InterfaceType => Context.GetType();
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

	protected virtual void Dispose(bool disposing)
	{
		var context = _context as IDisposable;

		_context = null;
		_args = null;

		context?.Dispose();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(true);
	}

	~DataContext()
	{
		Dispose(false);
	}
}

public record DataContextInfo
{
	/// <summary>
	/// Data Context Name
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Data Context Concrete Type
	/// </summary>
	public Type ConcreteType { get; }

	/// <summary>
	/// Data Layer
	/// </summary>
	public IDataLayerInfo DataLayer => _funcIDataLayerInfo();

	private readonly Func<IDataLayerInfo> _funcIDataLayerInfo;

	public DataContextInfo(string name, Type concreteType, Func<IDataLayerInfo> getDataLayer)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		ConcreteType = concreteType ?? throw new ArgumentNullException(nameof(concreteType));
		_funcIDataLayerInfo = getDataLayer ?? throw new ArgumentNullException(nameof(getDataLayer));
	}
}