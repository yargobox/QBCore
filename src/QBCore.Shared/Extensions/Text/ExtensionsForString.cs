using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace QBCore.Extensions.Text;

public static class ExtensionsForString
{
	public static string ReplaceEnding(this string @this, string oldValue, string? newValue = null)
	{
		return @this.EndsWith(oldValue)
			? string.IsNullOrEmpty(newValue)
				? @this.Substring(0, @this.Length - oldValue.Length)
				: @this.Substring(0, @this.Length - oldValue.Length) + newValue
			: @this;
	}

	public static string ReplaceEnding(this string @this, string oldValue, string? newValue, StringComparison comparisonType)
	{
		return @this.EndsWith(oldValue, comparisonType)
			? string.IsNullOrEmpty(newValue)
				? @this.Substring(0, @this.Length - oldValue.Length)
				: @this.Substring(0, @this.Length - oldValue.Length) + newValue
			: @this;
	}

	[return: NotNullIfNotNull("@this")]
	public static string? ToCamelCase(this string? @this) => ToNamingConvention(@this, NamingConventions.CamelCase);

	[return: NotNullIfNotNull("@this")]
	public static string? ToPascalCase(this string? @this) => ToNamingConvention(@this, NamingConventions.PascalCase);

	[return: NotNullIfNotNull("@this")]
	public static string? ToUnderScoresCase(this string? @this) => ToNamingConvention(@this, NamingConventions.UnderScores);

	[return: NotNullIfNotNull("@this")]
	public static string? ToNamingConvention(this string? @this, NamingConventions conv)
	{
		if (@this == null)
		{
			return @this;
		}
		if (@this.Length < 2)
		{
			if (conv == NamingConventions.UnderScores || conv == NamingConventions.CamelCase) return @this.ToLower();
		}

		if (conv == NamingConventions.CamelCase)
		{
			if (@this.Contains('_'))
			{
				var sb = new StringBuilder(@this.Length);
				int i = 0;
				
				while (i < @this.Length && @this[i] == '_')
				{
					i++;
				}

				sb.Append('_', i);
				
				for ( ; i < @this.Length; i++)
				{
					if (@this[i] == '_' && i + 1 < @this.Length && @this[i + 1] != '_')
					{
						sb.Append(char.ToUpper(@this[++i]));
					}
					else
					{
						sb.Append(@this[i]);
					}
				}
				
				if (char.IsUpper(sb[0]))
				{
					sb[0] = char.ToLower(sb[0]);
					return sb.ToString();
				}
				return sb.Length == @this.Length ? @this : sb.ToString();
			}
			else
			{
				if (char.IsUpper(@this[0]))
				{
					return char.ToLower(@this[0]) + @this.Substring(1);
				}
				return @this;
			}
		}
		else if (conv == NamingConventions.PascalCase)
		{
			if (@this.Contains('_'))
			{
				var sb = new StringBuilder(@this.Length);
				int i = 0;
				
				while (i < @this.Length && @this[i] == '_')
				{
					i++;
				}

				sb.Append('_', i);
				
				for ( ; i < @this.Length; i++)
				{
					if (@this[i] == '_' && i + 1 < @this.Length && @this[i + 1] != '_')
					{
						sb.Append(char.ToUpper(@this[++i]));
					}
					else
					{
						sb.Append(@this[i]);
					}
				}
				
				if (char.IsLower(sb[0]))
				{
					sb[0] = char.ToUpper(sb[0]);
					return sb.ToString();
				}
				return sb.Length == @this.Length ? @this : sb.ToString();
			}
			else
			{
				if (char.IsLower(@this[0]))
				{
					return char.ToUpper(@this[0]) + @this.Substring(1);
				}
				return @this;
			}
		}
		else if (conv == NamingConventions.UnderScores)
		{
			var sb = new StringBuilder(@this.Length * 2);
			bool isPrevLower = false;
			for (int i = 0; i < @this.Length; i++)
			{
				if (char.IsUpper(@this[i]))
				{
					if (isPrevLower)
					{
						sb.Append('_');
					}
					isPrevLower = false;
					sb.Append(char.ToLower(@this[i]));
				}
				else
				{
					isPrevLower = true;
					sb.Append(@this[i]);
				}
			}
			return sb.ToString();
		}
		else
		{
			return @this;
		}
	}

	/// <summary>
	/// Unescapes and splits a string into substrings based on a specified delimiting and escape characters and options.
	/// </summary>
	/// <remarks>
	/// Opposite method is <c>IEnumerable<string>.EscapeAndJoin</c>
	/// </remarks>
	[return: NotNullIfNotNull("input")]
	public static string[]? UnescapeAndSplit(this string? input, char delimiter, StringSplitOptions options = StringSplitOptions.None, char escape = '\\')
	{
		if (input == null) return null;
		if (input.IndexOf(escape) < 0) return input.Split(delimiter, options);

		var escapeEscape = new string(escape, 2);
		var escapeDelimiter = string.Concat(escape, delimiter);
		var sescape = new string(escape, 1);
		var sdelimiter = new string(delimiter, 1);

		var list = new List<string>();
		int i = 0, j, k = 0;
		while ((j = input.IndexOf(delimiter, i)) >= 0)
		{
			if (ConsecutiveBackwordCharCount(input, j - 1, escape) % 2 == 0)
			{
				if (j + 1 < input.Length)
				{
					list.Add(input.Substring(k, j - k).Replace(escapeEscape, sescape).Replace(escapeDelimiter, sdelimiter));
					k = i = j + 1;
				}
				else
				{
					break;
				}
			}
			else if (j + 1 < input.Length)
			{
				i = j + 1;
			}
			else
			{
				break;
			}
		}
		list.Add(input.Substring(k, input.Length - k).Replace(escapeEscape, sescape).Replace(escapeDelimiter, sdelimiter));

		if (options.HasFlag(StringSplitOptions.TrimEntries))
			return list.Select(x => x.Trim()).Where(x => options.HasFlag(StringSplitOptions.RemoveEmptyEntries) ? x.Length > 0 : true).ToArray();
		else
			return list.Where(x => options.HasFlag(StringSplitOptions.RemoveEmptyEntries) ? x.Length > 0 : true).ToArray();
	}
	static int ConsecutiveBackwordCharCount(string input, int startIndex, char charToCount)
	{
		int count = 0;
		while (startIndex >= 0 && input[startIndex--] == charToCount) count++;
		return count;
	}

	/// <summary>
	/// Escapes the members of a collection and concatenates them, using the specified separator between each member.
	/// </summary>
	/// <remarks>
	/// Opposite method is <c>string.UnescapeAndSplit</c>
	/// </remarks>
	[return: NotNullIfNotNull("input")]
	public static string? EscapeAndJoin(this IEnumerable<string>? input, char delimiter, char escape = '\\')
	{
		if (input == null) return null;

		var delimiterString = delimiter.ToString();
		var escapeString = escape.ToString();
		var escapedDelimiter = escapeString + delimiterString;
		var escapedEscape = new string(escape, 2);

		var next = false;
		var sb = new StringBuilder();
		foreach (var s in input)
		{
			if (next) sb.Append(delimiter); else next = true;

			sb.Append(s.Replace(escapeString, escapedEscape).Replace(delimiterString, escapedDelimiter));
		}

		return sb.ToString();
	}
}