namespace QBCore.DataSource;

public interface IDSAsyncEnumerable<out T> : IAsyncEnumerable<T>
{
	bool IsObtainEOF { get; }
	bool IsEOFAvailable { get; }
	bool EOF { get; }
	
	bool IsObtainTotalCount { get; }
	bool IsTotalCountAvailable { get; }
	long TotalCount { get; }
}