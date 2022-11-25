using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class Language
{
	public int LanguageId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public virtual ICollection<Translation> Translations { get; set; } = null!;
	public virtual ICollection<DataEntryTranslation> DataEntryTranslations { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<Language>
	{
		public override void Configure(EntityTypeBuilder<Language> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.LanguageId);

			builder
				.HasMany(x => x.Translations)
				.WithOne(x => x.Language)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.DataEntryTranslations)
				.WithOne(x => x.Language)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Name).IsUnique();
			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);

			builder.Property(x => x.Name).HasMaxLength(20).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new Language[]
			{
				new Language
				{
					LanguageId = 1,
					Name = "en"
				},
				new Language
				{
					LanguageId = 2,
					Name = "uk"
				}
			});
		}
	}
}