using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class Translation
{
	[DeId, DeNoStorage, DeDependsOn(nameof(RefId), nameof(LanguageId), nameof(RefKey))]
	public (int RefId, int LanguageId, string RefKey) TranslationId
	{
		get => (RefId, LanguageId, RefKey);
		set
		{
			RefId = value.RefId;
			LanguageId = value.LanguageId;
			RefKey = value.RefKey;
		}
	}

	[Required]
	public int RefId { get; set; }

	[MaxLength(60), Required]
	public string RefKey { get; set; } = null!;

	[MaxLength(80), Required]
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
}