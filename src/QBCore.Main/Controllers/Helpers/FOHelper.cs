using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using QBCore.DataSource;
using QBCore.Extensions.Text;

namespace QBCore.Controllers;

internal static class FOHelper
{
	static FOHelper()
	{
		_dotNumberFormatInvariantCulture = (CultureInfo) CultureInfo.InvariantCulture.Clone();
		_dotNumberFormatInvariantCulture.NumberFormat.CurrencyDecimalSeparator = ".";
	}

	public static string? SerializeToString<T>(DSCondition<T>[]? values, IDataLayerInfo dataLayer)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Serializes filter values from string
	/// </summary>
	/// <remarks>
	/// Flags:
	/// TrueWhenNull: [n]
	/// CaseInsensitive: [i]
	///
	/// Operations:
	/// IsNull:			 {name}[|-n]
	/// IsNotNull:		!{name}[|-n]
	/// Equal:			 {name}[|-ni]=[arg1]
	/// NotEqual:		!{name}[|-ni]=[arg1]
	/// In:				 {name}[|-ni]:[[arg1],[arg2]...]
	/// NotIn:			!{name}[|-ni]:[[arg1],[arg2]...]
	/// Greater:		 {name}[|-n]>[arg1]
	/// GreaterOrEqual:	 {name}[|-n]>=[arg1]
	/// Less:			 {name}[|-n]<[arg1]
	/// LessOrEqual:	 {name}[|-n]<=[arg1]
	/// Between:		 {name}[|-n]-[arg1],[arg2]
	/// NotBetween:		!{name}[|-n]-[arg1],[arg2]
	/// Like:			 {name}[|-ni]~[arg1]
	/// NotLike:		!{name}[|-ni]~[arg1]
	/// BitsAnd:		 {name}[|-n].=[arg1]
	/// BitsOr:			 {name}[|-n].~[arg1]  or {name}[|-n]~[arg1]
	/// 
	/// </remarks>
	/// <param name="filter">Filter input string to parse</param>
	/// <exception cref="InvalidOperationException"></exception>
	/// <exception cref="FormatException"></exception>
	public static DSCondition<T>[]? SerializeFromString<T>(string? filter, IDataLayerInfo dataLayer)
	{
		if (string.IsNullOrWhiteSpace(filter)) return null;

		var filterStringValues = filter.UnescapeAndSplit(';', StringSplitOptions.RemoveEmptyEntries, '\\');
		var filterValues = new DSCondition<T>[filterStringValues.Length];
		int i = 0;
		FO trueOperation;
		DEPath? field;

		foreach (var p in filterStringValues.Select(x => PrepareFilterValueFromString(x)))
		{
			field = new DEPath(typeof(T), p.name, true, false, dataLayer);

			trueOperation = p.operation;
			if (trueOperation.HasFlag(FO.Like) && field.UnderlyingType != typeof(string))
			{
				trueOperation = (p.operation & ~FO.Like) | FO.BitsOr;
			}

			if ((trueOperation & (FO.IsNull | FO.IsNotNull)) != 0)
			{
				if (!string.IsNullOrEmpty(p.arguments))
				{
					throw new FormatException($"Operation IsNull of filter '{field.Path}' cannot have arguments.");
				}

				filterValues[i++] = new DSCondition<T>(field, trueOperation, null);
			}
			else if ((trueOperation & (
					FO.Equal |
					FO.NotEqual |
					FO.Greater |
					FO.GreaterOrEqual |
					FO.Less |
					FO.LessOrEqual |
					FO.Like |
					FO.NotLike |
					FO.BitsAnd |
					FO.BitsOr
				)) != 0)
			{
				var value = ParseFilterValue(field.UnderlyingType, p.arguments);
				
				if (field.IsNullable && (trueOperation & (FO.Like | FO.NotLike)) == 0 &&
					value is string stringValue && stringValue.Length == 0)
				{
					value = null;
				}

				if (!field.IsNullable && value == null)
				{
					throw new ArgumentException($"Filter '{field.Path}' does not support nullable arguments.");
				}

				filterValues[i++] = new DSCondition<T>(field, trueOperation, value);
			}
			else if ((trueOperation & (FO.Between | FO.NotBetween)) != 0)
			{
				var castMethod = _castMethodInfo.MakeGenericMethod(new[] { field.DataEntryType });
				var toArrayMethod = _toArrayMethodInfo.MakeGenericMethod(new[] { field.DataEntryType });

				var arr1 = p.arguments!
					.UnescapeAndSplit(',', StringSplitOptions.None, '\\')
					.Select(x => ParseFilterValue(field.UnderlyingType, x)
						?? (field.IsNullable ? null : throw new ArgumentNullException($"Filter '{field.Path}' does not support nullable arguments.")));

				var arr = toArrayMethod.Invoke(null, new object?[] { castMethod!.Invoke(null, new object?[] { arr1 }) });

				if (((Array)arr!).Length != 2)
				{
					throw new ArgumentException($"Filter '{field.Path}' does not support such a format.");
				}

				filterValues[i++] = new DSCondition<T>(field, trueOperation, arr);
			}
			else if ((trueOperation & (FO.In | FO.NotIn)) != 0)
			{
				var castMethod = _castMethodInfo.MakeGenericMethod(new[] { field.DataEntryType });
				var toArrayMethod = _toArrayMethodInfo.MakeGenericMethod(new[] { field.DataEntryType });

				var arr1 = p.arguments!
					.UnescapeAndSplit(',', StringSplitOptions.RemoveEmptyEntries, '\\')
					.Select(x => ParseFilterValue(field.UnderlyingType, x)
						?? (field.IsNullable ? null : throw new ArgumentNullException($"Filter '{field.Path}' does not support nullable arguments.")));

				var arr = toArrayMethod.Invoke(null, new object?[] { castMethod!.Invoke(null, new object?[] { arr1 }) });

				filterValues[i++] = new DSCondition<T>(field, trueOperation, arr);
			}
			else
			{
				throw new NotSupportedException($"Filter '{field.Path}' does not support such an operation.");
			}
		}

		return filterValues;
	}

	/// <summary>
	/// Takes the first step of parsing the filter value string.
	/// It returns the argument(s) together as a single non-null string and does not check supported flags by operation or arguments.
	/// </summary>
	/// <param name="filterStringValue"></param>
	/// <exception cref="FormatException"></exception>
	private static (string name, FO operation, string? arguments) PrepareFilterValueFromString(string filterStringValue)
	{
		try
		{
			var m = _matchFilterValueFromString.Match(filterStringValue);
			if (m.Success)
			{
				var direct = !(m.Groups[1].Value == "!");
				var fo = m.Groups[4].Value switch
				{
					"=" => direct ? FO.Equal : FO.NotEqual,
					":" => direct ? FO.In : FO.NotIn,
					"-" => direct ? FO.Between : FO.NotBetween,
					">" => direct ? FO.Greater : throw new FormatException(),
					">=" => direct ? FO.GreaterOrEqual : throw new FormatException(),
					"<" => direct ? FO.Less : throw new FormatException(),
					"<=" => direct ? FO.LessOrEqual : throw new FormatException(),
					"~" => direct ? FO.Like : FO.NotLike,
					".=" => direct ? FO.BitsAnd : throw new FormatException(),
					".~" => direct ? FO.BitsOr : throw new FormatException(),
					"" => direct ? FO.IsNotNull : FO.IsNull,
					_ => throw new FormatException()
				};
				var flags = m.Groups[3].Value.ToLower();
				if (flags.Contains('i')) fo |= FO.CaseInsensitive;
				if (flags.Contains('n')) fo |= FO.TrueWhenNull;
				return (m.Groups[2].Value, fo, m.Groups[5].Value);
			}
		}
		catch (Exception ex)
		{
			if (ex is FormatException) throw;
			throw new FormatException(ex.Message, ex);
		}
		throw new FormatException();
	}
	
	private static object? ParseFilterValue(Type operandType, string? value)
	{
		Func<string?, object?>? converter;
		if (_parseFilterValueConverters.TryGetValue(operandType, out converter))
		{
			return converter(value);
		}
		throw new NotSupportedException($"Data type '{operandType.ToPretty()}' is not supported by the filter.");
	}

	private static readonly MethodInfo _castMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))
		?? throw new ArgumentNullException(nameof(_castMethodInfo));
	private static readonly MethodInfo _toArrayMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
		?? throw new ArgumentNullException(nameof(_toArrayMethodInfo));

	private static readonly Regex _matchFilterValueFromString =
		new Regex(@"^(\!?)([a-zA-z0-9_]+)((?:\|n|\|ni|\|i|\|in)?)(?:(=|:|-|>=|>|<=|<|~|\.=|\.~)(.*))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(300));
	private static readonly CultureInfo _dotNumberFormatInvariantCulture;
	private static readonly Dictionary<Type, Func<string?, object?>> _parseFilterValueConverters = new ()
	{
		{ typeof(string), s => s },
 		{ typeof(char), s => string.IsNullOrEmpty(s) ? null : s[0] },

		{ typeof(Guid), s => string.IsNullOrEmpty(s) ? null : Guid.Parse(s) },

		{ typeof(byte), s => string.IsNullOrEmpty(s) ? null : byte.Parse(s) },
		{ typeof(sbyte), s => string.IsNullOrEmpty(s) ? null : sbyte.Parse(s) },
		{ typeof(short), s => string.IsNullOrEmpty(s) ? null : short.Parse(s) },
		{ typeof(ushort), s => string.IsNullOrEmpty(s) ? null : ushort.Parse(s) },
		{ typeof(int), s => string.IsNullOrEmpty(s) ? null : int.Parse(s) },
		{ typeof(uint), s => string.IsNullOrEmpty(s) ? null : uint.Parse(s) },
		{ typeof(long), s => string.IsNullOrEmpty(s) ? null : long.Parse(s) },
		{ typeof(ulong), s => string.IsNullOrEmpty(s) ? null : ulong.Parse(s) },

		{ typeof(DateTime), s => string.IsNullOrEmpty(s) ? null : DateTime.ParseExact(s, "yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture) },
		{ typeof(DateTimeOffset), s => string.IsNullOrEmpty(s) ? null : DateTimeOffset.ParseExact(s, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture) },
		{ typeof(DateOnly), s => string.IsNullOrEmpty(s) ? null : DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture) },
		{ typeof(TimeOnly), s => string.IsNullOrEmpty(s) ? null : TimeOnly.ParseExact(s, "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture) },
		{ typeof(TimeSpan), s => string.IsNullOrEmpty(s) ? null : TimeSpan.ParseExact(s, "d:hh:mm:ss.FFFFFFF", CultureInfo.InvariantCulture) },

		{ typeof(float), s => string.IsNullOrEmpty(s) ? null : float.Parse(s, NumberStyles.Float, _dotNumberFormatInvariantCulture) },
		{ typeof(double), s => string.IsNullOrEmpty(s) ? null : double.Parse(s, NumberStyles.Float, _dotNumberFormatInvariantCulture) },
		{ typeof(decimal), s => string.IsNullOrEmpty(s) ? null : decimal.Parse(s, NumberStyles.Integer | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, _dotNumberFormatInvariantCulture) }
	};
}