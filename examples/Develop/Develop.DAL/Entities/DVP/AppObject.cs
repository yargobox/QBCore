using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.DAL.Entities.DVP;

public class AppObject
{
	public int AppObjectId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int FuncGroupId { get; set; }
	public virtual FuncGroup FuncGroup { get; set; } = null!;

	public virtual ICollection<GenericObject> GenericObjects { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<AppObject>
	{
		public override void Configure(EntityTypeBuilder<AppObject> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.AppObjectId);

			builder
				.HasOne(x => x.FuncGroup)
				.WithMany(x => x.AppObjects)
				.IsRequired()
				.HasForeignKey(x => x.FuncGroupId)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.GenericObjects)
				.WithOne(x => x.AppObject)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new AppObject[]
			{
				new AppObject
				{
					AppObjectId = 1,
					Name = ToPlural(nameof(Project)),
					FuncGroupId = 2      // DVP
				}
				, new AppObject
				{
					AppObjectId = 2,
					Name = ToPlural(nameof(App)),
					FuncGroupId = 2      // DVP
				}
				, new AppObject
				{
					AppObjectId = 3,
					Name = ToPlural(nameof(FuncGroup)),
					FuncGroupId = 2      // DVP
				}
				, new AppObject
				{
					AppObjectId = 4,
					Name = ToPlural(nameof(AppObject)),
					FuncGroupId = 2      // DVP
				}
			});
		}
	}
}