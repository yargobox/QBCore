using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Develop.DAL.PostgreSQL;

namespace Develop.Entities.COM;

public class User
{
	public int UserId { get; set; }
	public string Login { get; set; } = string.Empty;
	public string? Name { get; set; }
	public string? Desc { get; set; }
	public DateTime Inserted { get; set; }
	public DateTime? Updated { get; set; }
	public DateTime? Deleted { get; set; }

	internal class EntityTypeConfiguration : PluralNamingConfiguration<User>
	{
		public override void Configure(EntityTypeBuilder<User> builder)
		{
			builder
				.ToTable(ObjectName, SchemaName)
				.HasKey(x => x.UserId);

			builder.HasIndex(x => x.Login).IsUnique();
			builder.HasIndex(x => x.Deleted).HasFilterNotNull(x => x.Deleted);

			builder.Property(x => x.Login).HasMaxLength(60).IsRequired();
			builder.Property(x => x.Name).HasMaxLength(100);
			builder.Property(x => x.Desc).HasMaxLength(400);
			builder.Property(x => x.Inserted).HasDefaultDateTimeNowConstraint();

			builder.HasData(new User[]
			{
				new User
				{
					UserId = 1,
					Login = "Admin",
					Name = "Admin",
					Desc = "Default admin account"
				}
			});
		}
	}
}