using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.DAL.Entities.DVP;

public class GenericObject
{
	public int GenericObjectId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int FuncGroupId { get; set; }
	public virtual FuncGroup FuncGroup { get; set; } = null!;

	public int? AppObjectId { get; set; }
	public virtual AppObject? AppObject { get; set; } = null!;

	public virtual ICollection<DataEntry> DataEntries { get; set; } = null!;
	public virtual ICollection<CDSNode> CDSNodes { get; set; } = null!;
	public virtual ICollection<AOListener> AOListeners { get; set; } = null!;
	public virtual ICollection<QueryBuilder> QueryBuilders { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<GenericObject>
	{
		public override void Configure(EntityTypeBuilder<GenericObject> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.GenericObjectId);

			builder
				.HasOne(x => x.FuncGroup)
				.WithMany(x => x.GenericObjects)
				.IsRequired()
				.HasForeignKey(x => x.FuncGroupId)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasOne(x => x.AppObject)
				.WithMany(x => x.GenericObjects)
				.HasForeignKey(x => x.AppObjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(x => x.DataEntries)
				.WithOne(x => x.GenericObject)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.CDSNodes)
				.WithOne(x => x.GenericObject)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QueryBuilders)
				.WithOne(x => x.GenericObject)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();
			builder.Property(x => x.FuncGroupId).IsRequired();

			builder.HasData(new GenericObject[]
			{
				new GenericObject
				{
					GenericObjectId = 1,
					AppObjectId = 1,
					Name = ToPlural(nameof(Project)),
					FuncGroupId = 2      // DVP
				}
				, new GenericObject
				{
					GenericObjectId = 2,
					AppObjectId = 2,
					Name = ToPlural(nameof(App)),
					FuncGroupId = 2      // DVP
				}
				, new GenericObject
				{
					GenericObjectId = 3,
					AppObjectId = 3,
					Name = ToPlural(nameof(FuncGroup)),
					FuncGroupId = 2      // DVP
				}
				, new GenericObject
				{
					GenericObjectId = 4,
					AppObjectId = 4,
					Name = ToPlural(nameof(AppObject)),
					FuncGroupId = 2      // DVP
				}
			});
		}
	}
}