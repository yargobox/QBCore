using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class Project
{
	[DeId, Required, DeReadOnly]
	public int ProjectId { get; set; }

	[DeViewName, MaxLength(80), Required]
	public string Name { get; set; } = null!;

	[MaxLength(400)]
	public string? Desc { get; set; }

	[DeCreated, DeReadOnly]
	public DateTime Inserted { get; set; }

	[DeUpdated]
	public DateTime? Updated { get; set; }

	[DeDeleted]
	public DateTime? Deleted { get; set; }

	//public virtual ICollection<App> Apps { get; set; } = null!;
	//public virtual ICollection<FuncGroup> FuncGroups { get; set; } = null!;
}