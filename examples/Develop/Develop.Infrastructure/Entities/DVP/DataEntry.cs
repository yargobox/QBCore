using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class DataEntry
{
	[DeId, Required]
	public int DataEntryId { get; set; }
	
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
	public int GenericObjectId { get; set; }
	//public virtual GenericObject GenericObject { get; set; } = null!;

	//public virtual ICollection<DataEntryTranslation> DataEntryTranslations { get; set; } = null!;
}