using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class CDSNode
{
	public int CDSNodeId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int GenericObjectId { get; set; }
	public virtual GenericObject GenericObject { get; set; } = null!;

	public int? ParentId { get; set; }
	public virtual CDSNode? Parent { get; set; } = null!;
	public virtual ICollection<CDSNode>? Children { get; set; } = null!;

	public virtual ICollection<CDSCondition> CDSConditions { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<CDSNode>
	{
		public override void Configure(EntityTypeBuilder<CDSNode> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.CDSNodeId);

			builder
				.HasOne(x => x.GenericObject)
				.WithMany(x => x.CDSNodes)
				.IsRequired()
				.HasForeignKey(x => x.GenericObjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasOne(x => x.Parent)
				.WithMany(x => x.Children)
				.HasForeignKey(x => x.ParentId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();
		}
	}
}