using System.ComponentModel.DataAnnotations;
using QBCore.DataSource;

namespace Develop.Entities.DVP;

public class FuncGroupByApps
{
	[DeForeignId, Required]
	public int AppsAppId { get; set; }

	[DeId, Required]
	public int FuncGroupsFuncGroupId { get; set; }
}