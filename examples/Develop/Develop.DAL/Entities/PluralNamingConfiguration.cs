using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Develop.Entities;

internal abstract class PluralNamingConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
	public virtual string SchemaName
	{
		get
		{
			var nsp = typeof(TEntity).Namespace ?? throw new InvalidOperationException();
			return nsp.Substring(nsp.LastIndexOf('.') + 1).ToLower(CultureInfo.InvariantCulture);
		}
	}
	public virtual string ObjectName
	{
		get
		{
			return ToPlural(typeof(TEntity).Name);
		}
	}

	public abstract void Configure(EntityTypeBuilder<TEntity> builder);


	#region Pluralizer

	private static readonly string[] _pluralEndingsType1 = { "s", "ss", "sh", "ch", "x", "z" };
	private static readonly char[] _pluralEndingsType2 = { 'a', 'e', 'i', 'o', 'u' };

	/// <summary>
	/// Translates the last word of the given string into the plural form (tries to guess it).
	/// </summary>
	[return: NotNullIfNotNull("word")]
	internal static string? ToPlural(string? word)
	{
		if (word == null || word.Length <= 2)
		{
			return word;
		}
		if (_pluralEndingsType1.Any(x => word.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
		{
			return word + "es";
		}
		if (word.EndsWith('f'))
		{
			return word.Substring(0, word.Length - 1) + "ves";
		}
		if (word.EndsWith("fe"))
		{
			return word.Substring(0, word.Length - 2) + "ves";
		}
		if (word.EndsWith('y'))
		{
			var lower = new String(word[word.Length - 2], 1).ToLower()[0];

			return _pluralEndingsType2.Any(x => x == lower)
				? word.Substring(0, word.Length - 1) + 's'
				: word.Substring(0, word.Length - 1) + "ies";
		}
		if (word.EndsWith("is"))
		{
			return word.Substring(0, word.Length - 2) + "es";
		}

		return char.IsDigit(word[word.Length - 2]) ? word : word + 's';
	}

	#endregion
}