using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.DAL.Entities.DVP;

public class DataEntry
{
	public int DataEntryId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int GenericObjectId { get; set; }
	public virtual GenericObject GenericObject { get; set; } = null!;

	public virtual ICollection<DataEntryTranslation> DataEntryTranslations { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<DataEntry>
	{
		public override void Configure(EntityTypeBuilder<DataEntry> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.DataEntryId);

			builder
				.HasOne(x => x.GenericObject)
				.WithMany(x => x.DataEntries)
				.IsRequired()
				.HasForeignKey(x => x.GenericObjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(x => x.DataEntryTranslations)
				.WithOne(x => x.DataEntry);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new DataEntry[]
			{
				new DataEntry
				{
					DataEntryId = 1,
					Name = nameof(Project.ProjectId),
					GenericObjectId = 1 // "Projects"
				}
				, new DataEntry
				{
					DataEntryId = 2,
					Name = nameof(Project.Name),
					GenericObjectId = 1 // "Projects"
				}
				, new DataEntry
				{
					DataEntryId = 3,
					Name = nameof(Project.Desc),
					GenericObjectId = 1 // "Projects"
				}
				, new DataEntry
				{
					DataEntryId = 4,
					Name = nameof(Project.Inserted),
					GenericObjectId = 1 // "Projects"
				}
				, new DataEntry
				{
					DataEntryId = 5,
					Name = nameof(Project.Updated),
					GenericObjectId = 1 // "Projects"
				}
				, new DataEntry
				{
					DataEntryId = 6,
					Name = nameof(Project.Deleted),
					GenericObjectId = 1 // "Projects"
				}

				, new DataEntry
				{
					DataEntryId = 8,
					Name = nameof(App.AppId),
					GenericObjectId = 2 // "Apps"
				}
				, new DataEntry
				{
					DataEntryId = 9,
					Name = nameof(App.Name),
					GenericObjectId = 2 // "Apps"
				}
				, new DataEntry
				{
					DataEntryId = 10,
					Name = nameof(App.Desc),
					GenericObjectId = 2 // "Apps"
				}
				, new DataEntry
				{
					DataEntryId = 11,
					Name = nameof(App.Inserted),
					GenericObjectId = 2 // "Apps"
				}
				, new DataEntry
				{
					DataEntryId = 12,
					Name = nameof(App.Updated),
					GenericObjectId = 2 // "Apps"
				}
				, new DataEntry
				{
					DataEntryId = 13,
					Name = nameof(App.Deleted),
					GenericObjectId = 2 // "Apps"
				}
				, new DataEntry
				{
					DataEntryId = 7,
					Name = nameof(App.ProjectId),
					GenericObjectId = 2 // "Apps"
				}
			});
		}
	}
}