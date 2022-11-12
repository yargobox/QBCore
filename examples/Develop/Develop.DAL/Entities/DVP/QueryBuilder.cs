using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.DAL.Entities.DVP;

public class QueryBuilder
{
	public int QueryBuilderId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	public int GenericObjectId { get; set; }
	public virtual GenericObject GenericObject { get; set; } = null!;

	public virtual ICollection<QBObject> QBObjects { get; set; } = null!;
	public virtual ICollection<QBColumn> QBColumns { get; set; } = null!;
	public virtual ICollection<QBParameter> QBParameters { get; set; } = null!;
	public virtual ICollection<QBJoinCondition> QBJoinConditions { get; set; } = null!;
	public virtual ICollection<QBCondition> QBConditions { get; set; } = null!;
	public virtual ICollection<QBSortOrder> QBSortOrders { get; set; } = null!;
	public virtual ICollection<QBAggregation> QBAggregations { get; set; } = null!;


	internal class EntityTypeConfiguration : PluralNamingConfiguration<QueryBuilder>
	{
		public override void Configure(EntityTypeBuilder<QueryBuilder> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.QueryBuilderId);

			builder
				.HasOne(x => x.GenericObject)
				.WithMany(x => x.QueryBuilders)
				.IsRequired()
				.HasForeignKey(x => x.GenericObjectId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(x => x.QBObjects)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QBColumns)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QBParameters)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QBJoinConditions)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QBConditions)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QBSortOrders)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);
			
			builder
				.HasMany(x => x.QBAggregations)
				.WithOne(x => x.QueryBuilder)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);
			
			builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();
		}
	}
}