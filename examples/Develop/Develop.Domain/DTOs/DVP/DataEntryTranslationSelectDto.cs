using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using QBCore.DataSource;

namespace Develop.DTOs.DVP;

public record struct DataEntryTranslationID
{
	public int DataEntryId { get; set; }
	public int LanguageId { get; set; }

	public DataEntryTranslationID() { }
	public DataEntryTranslationID(int DataEntryId, int LanguageId)
	{
		this.DataEntryId = DataEntryId;
		this.LanguageId = LanguageId;
	}
}

public class DataEntryTranslationSelectDto
{
	[DeId, DeDependsOn(nameof(DataEntryId), nameof(LanguageId)), NotMapped]
	public DataEntryTranslationID DataEntryTranslationId
	{
		get => new DataEntryTranslationID(DataEntryId, LanguageId);
		set
		{
			DataEntryId = value.DataEntryId;
			LanguageId = value.LanguageId;
		}
	}

	[Required, Column("RefId"), XmlIgnore, JsonIgnore]
	public int DataEntryId { get; set; }

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

	[DeForeignId, Required, XmlIgnore, JsonIgnore]
	public int LanguageId { get; set; }
	public string LanguageName { get; set; } = null!;
}