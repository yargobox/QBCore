using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class AppObject
{
	[DeId, Required]
	public int AppObjectId { get; set; }
	
	[DeName, MaxLength(80), Required]
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
	public int FuncGroupId { get; set; }
	//public virtual FuncGroup FuncGroup { get; set; } = null!;

	//public virtual ICollection<GenericObject> GenericObjects { get; set; } = null!;
}