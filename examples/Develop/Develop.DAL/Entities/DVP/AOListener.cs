using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class AOListener
{
	public int AOListenerId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int GenericObjectId { get; set; }
	public virtual GenericObject GenericObject { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<AOListener>
	{
		public override void Configure(EntityTypeBuilder<AOListener> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.AOListenerId);

			builder
				.HasOne(x => x.GenericObject)
				.WithMany(x => x.AOListeners)
				.IsRequired()
				.HasForeignKey(x => x.GenericObjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();
		}
	}
}