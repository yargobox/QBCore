using Develop.Entities.DVP;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.EfCore;

namespace Develop.DTOs.DVP;

public class ProjectUpdateDto
{
	[DeViewName] public string Name { get; set; } = string.Empty;
	public string Desc { get; set; } = string.Empty;

	static void Builder(IEfCoreUpdateQBBuilder<Project, ProjectUpdateDto> builder)
	{
		builder.Update("projects");
	}
}