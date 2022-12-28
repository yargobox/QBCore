using Develop.Entities.DVP;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.EfCore;

namespace Develop.DTOs.DVP;

public class ProjectCreateDto
{
	[DeViewName] public string Name { get; set; } = string.Empty;
	public string Desc { get; set; } = string.Empty;

	static void Builder(IQBEfCoreInsertBuilder<Project, ProjectCreateDto> builder)
	{
		builder.Insert("projects");
	}
}