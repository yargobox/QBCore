using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class QueryBuilder
{
	[DeId, Required]
	public int QueryBuilderId { get; set; }

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
	public int GenericObjectId { get; set; }
	//public virtual GenericObject GenericObject { get; set; } = null!;

	//public virtual ICollection<QBObject> QBObjects { get; set; } = null!;
	//public virtual ICollection<QBColumn> QBColumns { get; set; } = null!;
	//public virtual ICollection<QBParameter> QBParameters { get; set; } = null!;
	//public virtual ICollection<QBJoinCondition> QBJoinConditions { get; set; } = null!;
	//public virtual ICollection<QBCondition> QBConditions { get; set; } = null!;
	//public virtual ICollection<QBSortOrder> QBSortOrders { get; set; } = null!;
	//public virtual ICollection<QBAggregation> QBAggregations { get; set; } = null!;
}