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
		if (@this == null || @this.Length < 2)
		{
			return @this;
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
}