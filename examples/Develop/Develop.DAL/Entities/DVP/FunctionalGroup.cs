using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class FuncGroup
{
	public int FuncGroupId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int ProjectId { get; set; }
	public virtual Project Project { get; set; } = null!;

	public virtual ICollection<AppObject> AppObjects { get; set; } = null!;
	public virtual ICollection<GenericObject> GenericObjects { get; set; } = null!;
	public virtual ICollection<App> Apps { get; set; } = null!;



	internal class EntityTypeConfiguration : PluralNamingConfiguration<FuncGroup>
	{
		public override void Configure(EntityTypeBuilder<FuncGroup> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.FuncGroupId);

			builder
				.HasOne(x => x.Project)
				.WithMany(x => x.FuncGroups)
				.IsRequired()
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(x => x.AppObjects)
				.WithOne(x => x.FuncGroup)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.GenericObjects)
				.WithOne(x => x.FuncGroup)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new FuncGroup[]
			{
				new FuncGroup
				{
					FuncGroupId = 1,
					Name = "COM",
					ProjectId = 1 // "General"
				},
				new FuncGroup
				{
					FuncGroupId = 2,
					Name = "DVP",
					ProjectId = 1 // "General"
				}
			});
		}
	}
}