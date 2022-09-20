namespace QBCore.Controllers.Models;

public class DataSourceResponse<T>
{
	public int PageSize { get; set; } = -1;
	public int PageNumber { get; set; } = -1;
	public long TotalCount { get => _totalCount; set => Interlocked.Exchange(ref _totalCount, value); }
	public int IsLastPage { get => _isLastPage; set => Interlocked.Exchange(ref _isLastPage, value); }
	public IEnumerable<T>? Data { get; set; }
	public IReadOnlyDictionary<string, object?>? Aggregations { get; set; }

	private int _isLastPage = -1;
	private long _totalCount = -1;
}