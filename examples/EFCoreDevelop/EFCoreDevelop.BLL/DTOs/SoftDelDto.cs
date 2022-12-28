using QBCore.DataSource;

namespace Develop.DTOs;

public class SoftDelDto
{
	[DeDeleted] public DateTime? Deleted { get; set; }
}