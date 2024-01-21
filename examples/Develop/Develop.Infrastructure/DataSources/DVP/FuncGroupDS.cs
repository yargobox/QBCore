using Develop.DTOs;
using Develop.DTOs.DVP;
using Develop.Entities.DVP;
using Develop.Services;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;
using QBCore.ObjectFactory;

namespace Develop.DataSources;

[DsApiController]
[DataSource("func-group", typeof(PgSqlDataLayer), DataSourceOptions.SoftDelete)]
public sealed class FuncGroupDS : DataSource<int, FuncGroup, FuncGroupCreateDto, FuncGroupSelectDto, FuncGroupUpdateDto, EmptyDto, EmptyDto, FuncGroupDS>, IFuncGroupService, ITransient<IFuncGroupService>
{
	public FuncGroupDS(IServiceProvider sp) : base(sp) { }

	static void Builder(IDSBuilder builder)
	{
		builder.ServiceInterface = typeof(IFuncGroupService);
	}
	static void Builder(ISqlInsertQBBuilder<FuncGroup, FuncGroupCreateDto> builder)
	{
		builder.AutoBuild("dvp.FuncGroups");
	}
	static void Builder(ISqlSelectQBBuilder<FuncGroup, FuncGroupSelectDto> builder)
	{
		builder.Select("dvp.FuncGroups")
			.LeftJoin<Project>("dvp.Projects")
				.Connect<Project, FuncGroup>(project => project.ProjectId, doc => doc.ProjectId, FO.Equal)
				.Include<Project>(sel => sel.ProjectName, project => project.Name)
		;
	}
	static void Builder(ISqlUpdateQBBuilder<FuncGroup, FuncGroupUpdateDto> builder)
	{
		builder.AutoBuild("dvp.FuncGroups");
	}
	static void Builder(ISqlSoftDelQBBuilder<FuncGroup, EmptyDto> builder)
	{
		builder.AutoBuild("dvp.FuncGroups");
	}
	static void Builder(ISqlRestoreQBBuilder<FuncGroup, EmptyDto> builder)
	{
		builder.AutoBuild("dvp.FuncGroups");
	}
}