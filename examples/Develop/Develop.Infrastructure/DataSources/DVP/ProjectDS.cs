using Develop.Entities.DVP;
using Develop.DTOs.DVP;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;
using Develop.DTOs;
using Develop.Services;

namespace Develop.DataSources;

[DsApiController]
[DataSource("project", typeof(PgSqlDataLayer), DataSourceOptions.SoftDelete)]
public sealed class ProjectDS : DataSource<int, Project, ProjectCreateDto, ProjectSelectDto, ProjectUpdateDto, SoftDelDto, SoftDelDto, ProjectDS>, IProjectService
{
	public ProjectDS(IServiceProvider serviceProvider) : base(serviceProvider) { }

	static void DefinitionBuilder(IDSBuilder builder)
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
		builder.Insert("projects");
	}
	static void Builder(ISqlSelectQBBuilder<Project, ProjectSelectDto> builder)
	{
		builder.Select("projects");
	}
	static void Builder(ISqlUpdateQBBuilder<Project, ProjectUpdateDto> builder)
	{
		builder.Update("projects");
	}
	static void Builder(ISqlSoftDelQBBuilder<Project, SoftDelDto> builder)
	{
		builder.Update("projects");
	}
	static void Builder(ISqlRestoreQBBuilder<Project, SoftDelDto> builder)
	{
		builder.Update("projects");
	}

/* 	static void SoftDelBuilder(IQbEfSoftDelBuilder<Project, SoftDelDto> qb)
	{
		qb.Update("projects")
			.Condition(doc => doc.Id, FO.Equal, "id")
			.Condition(doc => doc.Deleted, null, FO.IsNull)
		;
	}
	static void RestoreBuilder(IQbEfRestoreBuilder<Project, SoftDelDto> qb)
	{
		qb.Update("projects")
			.Condition(doc => doc.Id, FO.Equal, "id")
			.Condition(doc => doc.Deleted, null, FO.IsNotNull)
		;
	}
	static void DeleteBuilder(IQbEfDeleteBuilder<Project, SoftDelDto> qb)
	{
		qb.Delete("projects")
			.Condition(doc => doc.Id, FO.Equal, "id");
	} */
}