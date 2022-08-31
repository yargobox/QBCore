using QBCore.DataSource;

namespace Example1.DAL.Entities;

public sealed class SoftDelDto
{
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }
}