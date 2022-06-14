namespace Example1.DAL.Entities.Brands;

public class Brand
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}