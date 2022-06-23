namespace Example1.DAL.Entities;

public sealed class SoftDelDto
{
	DateTimeOffset? Deleted { get; set; }
}