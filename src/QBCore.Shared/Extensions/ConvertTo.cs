using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using QBCore.Extensions.Internals;

namespace QBCore.Extensions;

/// <summary>
/// Class to convert to type <see cref="To"/>
/// </summary>
/// <typeparam name="To">type to convert to</typeparam>
public static class ConvertTo<To>
{
	/// <summary>
    /// Get a conversion delegate from the specified type.
    /// </summary>
    /// <typeparam name="From">source type to convert from</typeparam>
    /// <returns>a conversion delegate from the specified type.</returns>
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static Func<From, To> GetConvertor<From>()
	{
		return Static<From>._convertChecked;
	}

	/// <summary>
    /// Get an unchecked conversion delegate from the specified type.
    /// </summary>
    /// <typeparam name="From">source type to convert from</typeparam>
    /// <returns>an unchecked conversion delegate from the specified type.</returns>
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static Func<From, To> GetConvertorUnchecked<From>()
	{
		return Static<From>._convertUnchecked;
	}

	/// <summary>
	/// Convert <see cref="From"/> to <see cref="To"/> without boxing of value types.
	/// </summary>
	/// <typeparam name="From">source type to convert from</typeparam>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentNullException">If the required type cannot be null, but the value is.</exception>
    /// <exception cref="ArgumentException">No coercion operator or type conversion available.</exception>
    /// <exception cref="OverflowException" />
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static To From<From>(From value)
	{
		return Static<From>._convertChecked(value);
	}

	/// <summary>
	/// Convert unchecked <see cref="From"/> to <see cref="To"/> without boxing of value types.
	/// </summary>
	/// <typeparam name="From">source type to convert from</typeparam>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentNullException">If the required type cannot be null, but the value is.</exception>
    /// <exception cref="ArgumentException">No coercion operator or type conversion available.</exception>
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static To FromUnchecked<From>(From value)
	{
		return Static<From>._convertUnchecked(value);
	}

	/// <summary>
    /// Convert the specified object of type <see cref="From"/> to type <see cref="To"/>.
    /// </summary>
    /// <typeparam name="From">source type to convert from</typeparam>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentNullException">If the required type cannot be null, but the value is.</exception>
    /// <exception cref="ArgumentException">No coercion operator or type conversion available.</exception>
    /// <exception cref="OverflowException" />
    /// <exception cref="InvalidCastException" />
	[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static To FromObject<From>(object? value)
	{
		return ConvertTo<To>.From<From>((From)value!);
	}

	/// <summary>
    /// Convert unchecked the specified object of type <see cref="From"/> to type <see cref="To"/>.
    /// </summary>
    /// <typeparam name="From">source type to convert from</typeparam>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentNullException">If the required type cannot be null, but the value is.</exception>
    /// <exception cref="ArgumentException">No coercion operator or type conversion available.</exception>
    /// <exception cref="OverflowException" />
    /// <exception cref="InvalidCastException" />
	[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static To FromObjectUnchecked<From>(object? value)
	{
		return ConvertTo<To>.FromUnchecked<From>((From)value!);
	}

	private static class Static<From>
	{
		static Static() { }

		public static readonly Func<From, To> _convertChecked = ConvertChecked();
		public static readonly Func<From, To> _convertUnchecked = ConvertUnchecked();

		private static Func<From, To> ConvertChecked()
		{
			try
			{
				var param = Expression.Parameter(typeof(From));
				var convert = Expression.ConvertChecked(param, typeof(To));

				if (default(From) is null && default(To) is not null)
				{
					LabelTarget returnTarget = Expression.Label(typeof(To));
					Expression test = Expression.NotEqual(param, Expression.Constant(default(From)));
					Expression ifthen = Expression.Return(returnTarget, convert);
					BlockExpression block = Expression.Block(
						Expression.IfThen(test, ifthen),
						Expression.Throw(Expression.Constant(EX.Shared.Make.ValueTypeCannotBeNull(typeof(From).ToPretty()))),
						Expression.Label(returnTarget, Expression.Constant(default(To))));

					return Expression.Lambda<Func<From, To>>(block, param).Compile();
				}
				else
				{
					return Expression.Lambda<Func<From, To>>(convert, param).Compile();
				}
			}
			catch
			{
				return _ => throw EX.Shared.Make.NoCoercionOperatorOrTypeConversionAvailable(typeof(From).ToPretty(), typeof(To).ToPretty());
			}
		}

		private static Func<From, To> ConvertUnchecked()
		{
			try
			{
				var param = Expression.Parameter(typeof(From));
				var convert = Expression.Convert(param, typeof(To));

				if (default(From) is null && default(To) is not null)
				{
					LabelTarget returnTarget = Expression.Label(typeof(To));
					Expression test = Expression.NotEqual(param, Expression.Constant(default(From)));
					Expression ifthen = Expression.Return(returnTarget, convert);
					BlockExpression block = Expression.Block(
						Expression.IfThen(test, ifthen),
						Expression.Throw(Expression.Constant(EX.Shared.Make.ValueTypeCannotBeNull(typeof(From).ToPretty()))),
						Expression.Label(returnTarget, Expression.Constant(default(To))));

					return Expression.Lambda<Func<From, To>>(block, param).Compile();
				}
				else
				{
					return Expression.Lambda<Func<From, To>>(convert, param).Compile();
				}
			}
			catch
			{
				return _ => throw EX.Shared.Make.NoCoercionOperatorOrTypeConversionAvailable(typeof(From).ToPretty(), typeof(To).ToPretty());
			}
		}
	}
}