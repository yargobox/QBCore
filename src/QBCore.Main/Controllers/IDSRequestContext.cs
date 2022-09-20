
using QBCore.DataSource;

namespace QBCore.Controllers;

public enum DSRequestCommands : int
{
	None = 0,

	Create = 1,
	Index = 2,
	Get = 3,
	Update = 4,
	Delete = 5,
	Restore = 6,

	RefreshOnCreate = Create | RefreshDependsFlag,
	RefreshOnUpdate = Update | RefreshDependsFlag,
	IndexForFilter = Index | FilterFlag,
	GetForFilter = Get | FilterFlag,
	IndexForCell = Index | CellFlag,
	GetForCell = Get | CellFlag,

	FilterFlag = 0x00010000,
	CellFlag = 0x00020000,
	RefreshDependsFlag = 0x00040000
}

public interface IDSRequestContext
{
	bool Ready { get; }
	DSRequest Request { get; set; }
}

public class DSRequestContext : IDSRequestContext
{
	public bool Ready => _request != null;
	public DSRequest Request
	{
		get => _request ?? throw new InvalidOperationException(nameof(DSRequest) + " has not ready yet.");
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(Request));
			}

			if (Interlocked.CompareExchange(ref _request, value, null) != null)
			{
				throw new InvalidOperationException(nameof(DSRequest) + " has already been set.");
			}
		}
	}

	private DSRequest? _request = null;

	public DSRequestContext() { }
}

public abstract class DSRequest
{
	public abstract DSRequestCommands Command { get; }
	public abstract IDataSource DS { get; }
	public abstract IComplexDataSource? CDS { get; }
	public abstract IDataSource? ForDS { get; }
	public abstract DEPath? ForDE { get; }
	public abstract IReadOnlyDictionary<string, object?> CurrentRecordIDs { get; }
}