using QBCore.Extensions.Text;

namespace QBCore.DataSource;

internal static class SOHelper
{
	public static string? SerializeToString<T>(DSSortOrder<T>[]? values)
	{
		if (values == null || values.All(x => x.SortOrder == SO.None))
		{
			return null;
		}

		var stringValues = values
			.Where(x => x.SortOrder != SO.None)
			.Select(x => x.SortOrder == SO.Ascending
				? x.Field.ToString()
				: string.Concat(x.Field.ToString(), ",", ((uint)x.SortOrder).ToString()));

		return stringValues.EscapeAndJoin(';', '\\');
	}

	public static DSSortOrder<T>[]? SerializeFromString<T>(string? sort)
	{
		if (string.IsNullOrWhiteSpace(sort))
		{
			return null;
		}

		var stringValues = sort.UnescapeAndSplit(';', StringSplitOptions.RemoveEmptyEntries, '\\');
		if (stringValues.Length == 0)
		{
			return null;
		}

		var sortOrderValues = new DSSortOrder<T>[stringValues.Length];
		int i = 0;
		foreach (var s in stringValues)
		{
			string orderName;
			SO operation;
			if (s.EndsWith(",2"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Descending;
			}
			else if (s.EndsWith(",1"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Ascending;
			}
			else if (s.EndsWith(",4"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Rank;
			}
			else if (s.EndsWith(",5"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Ascending | SO.Rank;
			}
			else if (s.EndsWith(",6"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Descending | SO.Rank;
			}
			else if (!s.Contains(','))
			{
				orderName = s;
				operation = SO.Ascending;
			}
			else
			{
				throw new FormatException($"Sort order value '{sort}' is invalid.");
			}

			sortOrderValues[i++] = new DSSortOrder<T>(orderName, operation);
		}

		return sortOrderValues;
	}
}