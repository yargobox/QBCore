using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class GenericObject
{
	[DeId, Required]
	public int GenericObjectId { get; set; }
	
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
	public int FuncGroupId { get; set; }
	//public virtual FuncGroup FuncGroup { get; set; } = null!;

	[DeForeignId, Required]
	public int? AppObjectId { get; set; }
	//public virtual AppObject? AppObject { get; set; } = null!;

	//public virtual ICollection<DataEntry> DataEntries { get; set; } = null!;
	//public virtual ICollection<CDSNode> CDSNodes { get; set; } = null!;
	//public virtual ICollection<AOListener> AOListeners { get; set; } = null!;
	//public virtual ICollection<QueryBuilder> QueryBuilders { get; set; } = null!;
}