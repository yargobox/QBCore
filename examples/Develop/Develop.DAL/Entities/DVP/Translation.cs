using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.DAL.Entities.DVP;

public class Translation
{
	public int RefId { get; set; }
	public string RefKey { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int LanguageId { get; set; }
	public virtual Language Language { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<Translation>
	{
		public override void Configure(EntityTypeBuilder<Translation> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => new { x.RefId, x.LanguageId, x.RefKey });

			builder
				.HasOne(x => x.Language)
				.WithMany(x => x.Translations)
				.IsRequired()
				.HasForeignKey(x => x.LanguageId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);

			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.Property(x => x.RefId).ValueGeneratedNever().IsRequired();
			builder.Property(x => x.LanguageId).IsRequired();
			builder.Property(x => x.RefKey).HasMaxLength(60).IsRequired();

			builder.HasData(new Translation[]
			{
				new Translation
				{
					RefId = 1, // DataEntry = Project.ProjectId
					LanguageId = 2, // "uk"
					RefKey = nameof(DataEntry),
					Name = "Ід."
				}
				, new Translation
				{
					RefId = 2, // DataEntry = Project.Name
					LanguageId = 2, // "uk"
					RefKey = nameof(DataEntry),
					Name = "Проект"
				}
				, new Translation
				{
					RefId = 3, // DataEntry = Project.Desc
					LanguageId = 2, // "uk"
					RefKey = nameof(DataEntry),
					Name = "Опис"
				}

				, new Translation
				{
					RefId = 1, // DataEntry = Project.ProjectId
					LanguageId = 1, // "en"
					RefKey = nameof(DataEntry),
					Name = "Id."
				}
				, new Translation
				{
					RefId = 2, // DataEntry = Project.Name
					LanguageId = 1, // "en"
					RefKey = nameof(DataEntry),
					Name = "Project"
				}
				, new Translation
				{
					RefId = 3, // DataEntry = Project.Desc
					LanguageId = 1, // "en"
					RefKey = nameof(DataEntry),
					Name = "Description"
				},


				new Translation
				{
					RefId = 8, // DataEntry = App.AppId
					LanguageId = 2, // "uk"
					RefKey = nameof(DataEntry),
					Name = "Ід."
				}
				, new Translation
				{
					RefId = 9, // DataEntry = App.Name
					LanguageId = 2, // "uk"
					RefKey = nameof(DataEntry),
					Name = "Застосунок"
				}
				, new Translation
				{
					RefId = 10, // DataEntry = App.Desc
					LanguageId = 2, // "uk"
					RefKey = nameof(DataEntry),
					Name = "Опис"
				}

				, new Translation
				{
					RefId = 8, // DataEntry = App.AppId
					LanguageId = 1, // "en"
					RefKey = nameof(DataEntry),
					Name = "Id."
				}
				, new Translation
				{
					RefId = 9, // DataEntry = App.Name
					LanguageId = 1, // "en"
					RefKey = nameof(DataEntry),
					Name = "Application"
				}
			});
		}
	}
}