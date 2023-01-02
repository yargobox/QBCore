using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class AOListener
{
	[DeId, Required]
	public int AOListenerId { get; set; }

	[Required, MaxLength(80)]
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
}