namespace QBCore.Controllers.Models;

public class DataSourceResponse<T>
{
	public int PageSize { get; set; } = -1;
	public int PageNumber { get; set; } = -1;
	public long TotalCount { get; set; } = -1;
	public int IsLastPage { get; set; } = -1;
	public IEnumerable<T>? Data { get; set; }
}