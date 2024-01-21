using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.DTOs.DVP;

public class FuncGroupSelectDto
{
	[DeId, Required]
	public int FuncGroupId { get; set; }
	
	[DeName, Required, MaxLength(80)]
	public string Name { get; set; } = null!;

	[MaxLength(400)]
	public string? Desc { get; set; }

	[DeCreated, DeReadOnly]
	public DateTime Inserted { get; set; }
	
	[DeUpdated]
	public DateTime? Updated { get; set; }
	
	[DeDeleted]
	public DateTime? Deleted { get; set; }

	[DeForeignId, Required]
	public int ProjectId { get; set; }
	public string ProjectName { get; set; } = null!;
}