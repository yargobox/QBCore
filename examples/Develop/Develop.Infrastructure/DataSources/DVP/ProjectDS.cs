using Develop.Entities.DVP;
using Develop.DTOs.DVP;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;
using Develop.DTOs;
using Develop.Services;
using QBCore.ObjectFactory;

namespace Develop.DataSources.DVP;

[DsApiController]
[DataSource("project", typeof(PgSqlDataLayer), DataSourceOptions.SoftDelete)]
public sealed class ProjectDS : DataSource<int, Project, ProjectCreateDto, ProjectSelectDto, ProjectUpdateDto, SoftDelDto, SoftDelDto, ProjectDS>, IProjectService, ITransient<IProjectService>
{
	public ProjectDS(IServiceProvider sp) : base(sp) { }

	static void Builder(IDSBuilder builder)
	{
		//builder.Name = "[DS]";
		//builder.Options |= DataSourceOptions.SoftDelete | DataSourceOptions.CanInsert | DataSourceOptions.CanSelect;
		//builder.DataContextName = "default";
		//builder.DataLayer = typeof(MongoDataLayer);
		//builder.IsAutoController = true;
		//builder.ControllerName = "[DS:guessPlural]";
		//builder.IsServiceSingleton = false;
		//builder.Listeners.Add(typeof(ProjectServiceListener));
		builder.ServiceInterface = typeof(IProjectService);
		//builder.InsertBuilder = ProjectCreateDto.InsertBuilder;
		//builder.SelectBuilder = ProjectSelectDto.SelectBuilder;
		//builder.UpdateBuilder = ProjectUpdateDto.UpdateBuilder;
		//builder.SoftDelBuilder = SoftDelBuilder;
		//builder.RestoreBuilder = RestoreBuilder;
	}
	
	static void Builder(ISqlInsertQBBuilder<Project, ProjectCreateDto> builder)
	{
		builder.AutoBuild("dvp.Projects");
	}
	static void Builder(ISqlSelectQBBuilder<Project, ProjectSelectDto> builder)
	{
		builder.AutoBuild("dvp.Projects");
	}
	static void Builder(ISqlUpdateQBBuilder<Project, ProjectUpdateDto> builder)
	{
		builder.AutoBuild("dvp.Projects");
	}
	static void Builder(ISqlSoftDelQBBuilder<Project, SoftDelDto> builder)
	{
		builder.AutoBuild("dvp.Projects");
	}
	static void Builder(ISqlRestoreQBBuilder<Project, SoftDelDto> builder)
	{
		builder.AutoBuild("dvp.Projects");
	}
}