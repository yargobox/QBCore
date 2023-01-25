using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.DTOs.DVP;

public class ProjectUpdateDto
{
	[DeName, MaxLength(80), Required]
	public string Name { get; set; } = null!;

	[MaxLength(400)]
	public string? Desc { get; set; }
}