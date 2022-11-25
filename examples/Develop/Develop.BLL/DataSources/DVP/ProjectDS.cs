using Develop.Entities.DVP;
using Develop.DTOs.DVP;
using QBCore.Configuration;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.EntityFramework;
using Develop.DTOs;

namespace Develop.DataSources;

[DsApiController]
[DataSource("project", typeof(EfDataLayer), DataSourceOptions.SoftDelete)]
public sealed class ProjectDS : DataSource<int, Project, ProjectCreateDto, ProjectSelectDto, ProjectUpdateDto, SoftDelDto, SoftDelDto, ProjectDS>
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
		//builder.ServiceInterface = null;
		//builder.InsertBuilder = ProjectCreateDto.InsertBuilder;
		//builder.SelectBuilder = ProjectSelectDto.SelectBuilder;
		//builder.UpdateBuilder = ProjectUpdateDto.UpdateBuilder;
		//builder.SoftDelBuilder = SoftDelBuilder;
		//builder.RestoreBuilder = RestoreBuilder;
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