using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class Language
{
	[DeId, Required]
	public int LanguageId { get; set; }
	
	[DeViewName, MaxLength(20), Required]
	public string Name { get; set; } = null!;
	
	[MaxLength(400)]
	public string? Desc { get; set; }
	
	[DeCreated, DeReadOnly]
	public DateTime Inserted { get; set; }
	
	[DeUpdated]
	public DateTime? Updated { get; set; }
	
	[DeDeleted]
	public DateTime? Deleted { get; set; }

	//public virtual ICollection<Translation> Translations { get; set; } = null!;
	//public virtual ICollection<DataEntryTranslation> DataEntryTranslations { get; set; } = null!;
}