namespace QBCore.Controllers.Models;

public class DataSourceResponse<T>
{
	public int PageSize = -1;
	public int PageNumber = -1;
	public IEnumerable<T>? Data;
	public long TotalCount = -1;
	public int IsLastPage = -1;
}