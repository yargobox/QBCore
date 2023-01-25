using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class CDSNode
{
	[DeId, Required]
	public int CDSNodeId { get; set; }

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
	public int GenericObjectId { get; set; }
	//public virtual GenericObject GenericObject { get; set; } = null!;

	[DeForeignId]
	public int? ParentId { get; set; }
	//public virtual CDSNode? Parent { get; set; } = null!;
	//public virtual ICollection<CDSNode>? Children { get; set; } = null!;

	//public virtual ICollection<CDSCondition> CDSConditions { get; set; } = null!;
}