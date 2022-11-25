using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class QBCondition
{
	public int QBConditionId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int QueryBuilderId { get; set; }
	public virtual QueryBuilder QueryBuilder { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<QBCondition>
	{
		public override void Configure(EntityTypeBuilder<QBCondition> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.QBConditionId);

			builder
				.HasOne(x => x.QueryBuilder)
				.WithMany(x => x.QBConditions)
				.IsRequired()
				.HasForeignKey(x => x.QueryBuilderId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();
		}
	}
}