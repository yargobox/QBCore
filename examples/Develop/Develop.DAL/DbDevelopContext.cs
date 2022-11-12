using Develop.DAL.Entities.COM;
using Develop.DAL.Entities.DVP;
using Microsoft.EntityFrameworkCore;

namespace Develop.DAL;

public class DbDevelopContext : DbContext
{
	public DbDevelopContext(DbContextOptions<DbDevelopContext> options)
        : base(options)
	{
		//Database.EnsureDeleted();
		//Database.EnsureCreated();
		//Database.Migrate();
	}

	public virtual DbSet<User> Users { get; set; } = null!;

	public virtual DbSet<Language> Languages { get; set; } = null!;
	public virtual DbSet<Translation> Translations { get; set; } = null!;

	public virtual DbSet<Project> Projects { get; set; } = null!;
	public virtual DbSet<App> Apps { get; set; } = null!;
	public virtual DbSet<GenericObject> GenericObjects { get; set; } = null!;
	public virtual DbSet<FuncGroup> FuncGroups { get; set; } = null!;
	public virtual DbSet<AppObject> AppObjects { get; set; } = null!;
	public virtual DbSet<DataEntry> DataEntries { get; set; } = null!;
	public virtual DbSet<CDSNode> CDSNodes { get; set; } = null!;
	public virtual DbSet<CDSCondition> CDSConditions { get; set; } = null!;
	public virtual DbSet<AOListener> AOListeners { get; set; } = null!;
	public virtual DbSet<QueryBuilder> QueryBuilders { get; set; } = null!;
	public virtual DbSet<QBObject> QBObjects { get; set; } = null!;
	public virtual DbSet<QBColumn> QBColumns { get; set; } = null!;
	public virtual DbSet<QBParameter> QBParameters { get; set; } = null!;
	public virtual DbSet<QBJoinCondition> QBJoinConditions { get; set; } = null!;
	public virtual DbSet<QBCondition> QBConditions { get; set; } = null!;
	public virtual DbSet<QBSortOrder> QBSortOrders { get; set; } = null!;
	public virtual DbSet<QBAggregation> QBAggregations { get; set; } = null!;

	public virtual DbSet<DataEntryTranslation> DataEntryTranslations { get; set; } = null!;
	

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		configurationBuilder.Properties<string>().UseCollation("uk-UA-x-icu");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		new User.EntityTypeConfiguration().Configure(modelBuilder.Entity<User>());

		new Language.EntityTypeConfiguration().Configure(modelBuilder.Entity<Language>());
		new Translation.EntityTypeConfiguration().Configure(modelBuilder.Entity<Translation>());

		new Project.EntityTypeConfiguration().Configure(modelBuilder.Entity<Project>());
		new App.EntityTypeConfiguration().Configure(modelBuilder.Entity<App>());
		new GenericObject.EntityTypeConfiguration().Configure(modelBuilder.Entity<GenericObject>());
		new FuncGroup.EntityTypeConfiguration().Configure(modelBuilder.Entity<FuncGroup>());
		new AppObject.EntityTypeConfiguration().Configure(modelBuilder.Entity<AppObject>());
		new DataEntry.EntityTypeConfiguration().Configure(modelBuilder.Entity<DataEntry>());
		new CDSNode.EntityTypeConfiguration().Configure(modelBuilder.Entity<CDSNode>());
		new CDSCondition.EntityTypeConfiguration().Configure(modelBuilder.Entity<CDSCondition>());
		new AOListener.EntityTypeConfiguration().Configure(modelBuilder.Entity<AOListener>());
		new QueryBuilder.EntityTypeConfiguration().Configure(modelBuilder.Entity<QueryBuilder>());
		new QBObject.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBObject>());
		new QBColumn.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBColumn>());
		new QBParameter.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBParameter>());
		new QBJoinCondition.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBJoinCondition>());
		new QBCondition.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBCondition>());
		new QBSortOrder.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBSortOrder>());
		new QBAggregation.EntityTypeConfiguration().Configure(modelBuilder.Entity<QBAggregation>());

		new DataEntryTranslation.EntityTypeConfiguration().Configure(modelBuilder.Entity<DataEntryTranslation>());
	}
}