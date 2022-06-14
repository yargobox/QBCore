namespace QBCore.Controllers.Models;

public class DataSourceResponse<T>
{
	public long TotalRows { get; set; }
	public int PageSize { get; set; }
	public int PageNumber { get; set; }

	public IEnumerable<T> Data { get; set; }

	public DataSourceResponse(long totalRows, int pageSize, int pageNumber, IEnumerable<T> data)
	{
		TotalRows = totalRows;
		PageSize = pageSize;
		PageNumber = pageNumber;
		Data = data;
	}
}