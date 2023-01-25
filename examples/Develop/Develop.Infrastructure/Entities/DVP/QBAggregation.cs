using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class QBAggregation
{
	[DeId, Required]
	public int QBAggregationId { get; set; }
	
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

	[DeForeignId, Required]
	public int QueryBuilderId { get; set; }
	//public virtual QueryBuilder QueryBuilder { get; set; } = null!;
}