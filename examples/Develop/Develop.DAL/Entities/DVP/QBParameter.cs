using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.DVP;

public class QBParameter
{
	public int QBParameterId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int QueryBuilderId { get; set; }
	public virtual QueryBuilder QueryBuilder { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<QBParameter>
	{
		public override void Configure(EntityTypeBuilder<QBParameter> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.QBParameterId);

			builder
				.HasOne(x => x.QueryBuilder)
				.WithMany(x => x.QBParameters)
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