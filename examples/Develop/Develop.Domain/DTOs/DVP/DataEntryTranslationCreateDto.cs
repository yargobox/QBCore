using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QBCore.DataSource;

namespace Develop.DTOs.DVP;

public class DataEntryTranslationCreateDto
{
	[Required, Column("RefId")]
	public int DataEntryId { get; set; }

	[DeHidden]
	private string RefKey => "DataEntry";

	[DeName, MaxLength(80), Required]
	public string Name { get; set; } = null!;

	[MaxLength(400)]
	public string? Desc { get; set; }

	[DeForeignId, Required]
	public int LanguageId { get; set; }
}