using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class FuncGroup
{
	[DeId, Required]
	public int FuncGroupId { get; set; }
	
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

	[DeForeignId, Required]
	public int ProjectId { get; set; }
	//public virtual Project Project { get; set; } = null!;

	//public virtual ICollection<AppObject> AppObjects { get; set; } = null!;
	//public virtual ICollection<GenericObject> GenericObjects { get; set; } = null!;
	//public virtual ICollection<App> Apps { get; set; } = null!;
}