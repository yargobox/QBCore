using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class Project
{
	[DeId] public int ProjectId { get; set; }
	[DeViewName] public string Name { get; set; } = string.Empty;
	public string Desc { get; set; } = string.Empty;
	[DeCreated] public DateTime Inserted { get; set; }
	[DeUpdated] public DateTime? Updated { get; set; }
	[DeDeleted] public DateTime? Deleted { get; set; }

	public virtual ICollection<App> Apps { get; set; } = null!;
	public virtual ICollection<FuncGroup> FuncGroups { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<Project>
	{
		public override void Configure(EntityTypeBuilder<Project> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.ProjectId);

			builder
				.HasMany(x => x.Apps)
				.WithOne(x => x.Project)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(x => x.FuncGroups)
				.WithOne(x => x.Project)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Name).IsUnique();
			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);

			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400).IsRequired();
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new Project[]
			{
				new Project
				{
					ProjectId = 1,
					Name = "General"
				}
			});
		}
	}
}