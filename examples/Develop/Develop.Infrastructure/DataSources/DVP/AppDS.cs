using Develop.DTOs;
using Develop.DTOs.DVP;
using Develop.Entities.DVP;
using Develop.Services;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;
using QBCore.ObjectFactory;

namespace Develop.DataSources;

[DsApiController]
[DataSource("app", typeof(PgSqlDataLayer), DataSourceOptions.SoftDelete)]
public sealed class AppDS : DataSource<int, App, AppCreateDto, AppSelectDto, AppUpdateDto, EmptyDto, EmptyDto, AppDS>, IAppService, ITransient<IAppService>
{
	public AppDS(IServiceProvider sp) : base(sp) { }

	static void Builder(IDSBuilder builder)
	{
		builder.ServiceInterface = typeof(IAppService);
	}
	static void Builder(ISqlInsertQBBuilder<App, AppCreateDto> builder)
	{
		builder.AutoBuild("dvp.Apps");
	}
	static void Builder(ISqlSelectQBBuilder<App, AppSelectDto> builder)
	{
		builder.Select("dvp.Apps")
			.LeftJoin<Project>("dvp.Projects")
				.Connect<Project, App>(project => project.ProjectId, app => app.ProjectId, FO.Equal)
				.Include<Project>(sel => sel.ProjectName, project => project.Name);
	}
	static void Builder(ISqlUpdateQBBuilder<App, AppUpdateDto> builder)
	{
		builder.AutoBuild("dvp.Apps");
	}
	static void Builder(ISqlSoftDelQBBuilder<App, EmptyDto> builder)
	{
		builder.AutoBuild("dvp.Apps");
	}
	static void Builder(ISqlRestoreQBBuilder<App, EmptyDto> builder)
	{
		builder.AutoBuild("dvp.Apps");
	}
}