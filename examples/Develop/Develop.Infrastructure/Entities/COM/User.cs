using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.COM;

public class User
{
	[DeId, Required]
	public int UserId { get; set; }

	[Required, MaxLength(60)]
	public string Login { get; set; } = null!;

	[DeViewName, MaxLength(100)]
	public string? Name { get; set; }

	[MaxLength(400)]
	public string? Desc { get; set; }

	[DeCreated, DeReadOnly]
	public DateTime Inserted { get; set; }

	[DeUpdated]
	public DateTime? Updated { get; set; }

	[DeDeleted]
	public DateTime? Deleted { get; set; }
}