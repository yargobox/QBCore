using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class DataEntryTranslation
{
	[DeId, DeNoStorage, DeDependsOn(nameof(DataEntryId), nameof(LanguageId))]
	public (int DataEntryId, int LanguageId) DataEntryTranslationId
	{
		get => (DataEntryId, LanguageId);
		set
		{
			DataEntryId = value.DataEntryId;
			LanguageId = value.LanguageId;
		}
	}

	[Required]
	public int DataEntryId { get; set; }

	[DeIgnore]
	private string RefKey { get => nameof(DataEntry); set { } }

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
	public int LanguageId { get; set; }
	//public virtual Language Language { get; set; } = null!;

	//public virtual DataEntry? DataEntry { get; set; } = null!;
}