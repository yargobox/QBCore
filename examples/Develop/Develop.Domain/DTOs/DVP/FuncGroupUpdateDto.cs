using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.DTOs.DVP;

public class FuncGroupUpdateDto
{
	[DeName, Required, MaxLength(80)]
	public string Name { get; set; } = null!;

	[MaxLength(400)]
	public string? Desc { get; set; }
}