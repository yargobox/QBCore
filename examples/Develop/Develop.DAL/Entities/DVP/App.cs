using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class App
{
	public int AppId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int ProjectId { get; set; }
	public virtual Project Project { get; set; } = null!;

	public virtual ICollection<FuncGroup> FuncGroups { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<App>
	{
		public override void Configure(EntityTypeBuilder<App> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.AppId);

			builder
				.HasOne(x => x.Project)
				.WithMany(x => x.Apps)
				.IsRequired()
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(x => x.FuncGroups)
				.WithMany(x => x.Apps)
				.UsingEntity(x => x
					.ToTable($"{ToPlural(nameof(FuncGroup))}By{ToPlural(nameof(App))}")
					.HasData(new[]
					{
						new { AppsAppId = 1, FuncGroupsFuncGroupId = 1 },
						new { AppsAppId = 1, FuncGroupsFuncGroupId = 2 }
					})
				);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new App[]
			{
				new App
				{
					AppId = 1,
					Name = "Develop",
					Desc = "Застосунок для обліку та розробки застосунків на основі QBCore",
					ProjectId = 1 // "General"
				}
			});
		}
	}
}