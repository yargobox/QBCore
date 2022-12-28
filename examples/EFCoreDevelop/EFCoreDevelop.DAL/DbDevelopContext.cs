using Develop.Entities.COM;
using Develop.Entities.DVP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QBCore.Configuration;

namespace Develop.DAL;

public class DbDevelopContext : DbContext, IEfCoreDbContextLogger
{
	private volatile List<Action<string>>? _queryStringCallbacks;

	/// <summary>
    /// A callback delegate for logging a query string. Adding and removing the delegate here is not thread-safe for performance reasons.
    /// </summary>
	public event Action<string>? QueryStringCallback
	{
		add => (_queryStringCallbacks ??= new List<Action<string>>(2)).Add(value ?? throw new ArgumentNullException(nameof(value)));
		remove => _queryStringCallbacks?.Remove(value!);
	}

	public DbDevelopContext(DbContextOptions<DbDevelopContext> options)
        : base(options)
	{
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

		base.ConfigureConventions(configurationBuilder);
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.LogTo(
			OnQueryStringCallback,
			//new string[] { DbLoggerCategory.Query.Name, DbLoggerCategory.Database.Command.Name, DbLoggerCategory.Database.Transaction.Name },
			new EventId[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted.Id },
			LogLevel.Trace)
			.EnableSensitiveDataLogging();

		base.OnConfiguring(optionsBuilder);
	}

	private void OnQueryStringCallback(string queryString)
	{
		_queryStringCallbacks?.ForEach(x => x(queryString));
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