using System.Globalization;
using QBCore.DataSource;

namespace QBCore.Controllers.Helpers;

internal static class SoftDelHelper
{
	private static readonly int[] _softDelIntValues = Enum.GetValues<SoftDel>().Cast<int>().ToArray();

	public static bool TryParse(string? mode, out SoftDel softDelMode)
	{
		if (string.IsNullOrEmpty(mode))
		{
			softDelMode = SoftDel.Actual;
			return true;
		}

		int n;
		if (int.TryParse(mode, NumberStyles.Integer, null, out n) && _softDelIntValues.Contains(n))
		{
			softDelMode = (SoftDel)n;
			return true;
		}

		if (Enum.TryParse<SoftDel>(mode, true, out softDelMode))
		{
			return true;
		}

		return false;
	}

	public static SoftDel TryParse(string? mode, SoftDel @default)
	{
		if (string.IsNullOrEmpty(mode))
		{
			return @default;
		}

		int n;
		if (int.TryParse(mode, NumberStyles.Integer, null, out n) && _softDelIntValues.Contains(n))
		{
			return (SoftDel)n;
		}

		SoftDel softDelMode;
		if (Enum.TryParse<SoftDel>(mode, true, out softDelMode))
		{
			return softDelMode;
		}

		return @default;
	}

	public static SoftDel Parse(string? mode)
	{
		SoftDel softDelMode;
		if (TryParse(mode, out softDelMode))
		{
			return softDelMode;
		}

		throw new FormatException($"'{mode}' is not a valid soft delete mode value.");
	}
}