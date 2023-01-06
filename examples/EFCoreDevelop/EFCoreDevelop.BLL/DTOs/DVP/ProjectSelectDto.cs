using Develop.Entities.DVP;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.EfCore;

namespace Develop.DTOs.DVP;

public class ProjectSelectDto
{
	[DeId] public int ProjectId { get; set; }
	[DeViewName] public string Name { get; set; } = string.Empty;
	public string Desc { get; set; } = string.Empty;
	[DeCreated] public DateTime Inserted { get; set; }
	[DeUpdated] public DateTime? Updated { get; set; }
	[DeDeleted] public DateTime? Deleted { get; set; }

 	static void Builder(IEfCoreSelectQBBuilder<Project, ProjectSelectDto> builder)
	{
		builder.Select("projects");
	}
}