using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class App
{
	[DeId, Required]
	public int AppId { get; set; }
	
	[DeViewName, Required, MaxLength(80)]
	public string Name { get; set; } = null!;
	
	[MaxLength(400)]
	public string? Desc { get; set; }

	[DeCreated, DeReadOnly]
	public DateTime Inserted { get; set; }
	
	[DeUpdated]
	public DateTime? Updated { get; set; }
	
	[DeDeleted]
	public DateTime? Deleted { get; set; }

	[DeForeignId]
	public int ProjectId { get; set; }
	//public virtual Project Project { get; set; } = null!;

	//public virtual ICollection<FuncGroup> FuncGroups { get; set; } = null!;
}