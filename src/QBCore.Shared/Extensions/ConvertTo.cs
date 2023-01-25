using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using QBCore.Extensions.Internals;

namespace QBCore.Extensions;

public static class ExtensionsForConvertTo
{
	/// <summary>
    /// Has any flags (bitwise OR)?
    /// </summary>
	public static bool HasAnyFlag<T>(this T value, T mask) where T : struct, Enum
	{
		return (ConvertTo<ulong>.FromUnchecked(value) & ConvertTo<ulong>.FromUnchecked(mask)) != 0UL;
	}

/* 	/// <summary>
    /// Has all flags (bitwise AND)?
    /// </summary>
    /// <remarks>Four times slower than Enum.HasFlag() in the release mode.</remarks>
	public static bool HasAll<T>(this T value, T mask) where T : struct, Enum
	{
		var ul = ConvertTo<ulong>.FromUnchecked(mask);
		return (ConvertTo<ulong>.FromUnchecked(value) & ul) == ul;
	} */
}

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

	/// <summary>
	/// Map a POCO object of type <see cref="From"/> to a new one of type <see cref="To"/>.
	/// </summary>
	/// <typeparam name="From">source type to map from</typeparam>
    /// <param name="obj">object to map</param>
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static To MapFrom<From>(From obj)
	{
		return StaticMapFrom<From>._mapFrom(obj);
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

	private static class StaticMapFrom<From>
	{
		static StaticMapFrom() { }

		public static readonly Func<From, To> _mapFrom = MapFrom();

		private static Func<From, To> MapFrom()
		{
			try
			{
				var fromProps = typeof(From).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
						.Where(x => x.CanRead && x.GetMethod?.IsPublic == true).ToArray();
				var fromFields = typeof(From).GetFields(BindingFlags.Public | BindingFlags.Instance)
						.Where(x => x.IsPublic)/* .Concat(typeof(From).GetFields(BindingFlags.Public | BindingFlags.Static)
						.Where(x => x.IsPublic && x.IsLiteral && !x.IsInitOnly)) */.ToArray();
				var toProps = typeof(To).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
						.Where(x => x.CanWrite && x.SetMethod?.IsPublic == true).ToArray();
				var toFields = typeof(To).GetFields(BindingFlags.Public | BindingFlags.Instance)
						.Where(x => !x.IsInitOnly && x.IsPublic).ToArray();

				ConstructorInfo? winnerCtor = null;
				bool isCopyConstructor = false;
				#region Find the most suitable constructor
				{
					int matchedParams, score = 0;
					bool applicant;
					ParameterInfo[] ctorParams;
					foreach (var ctor in typeof(To).GetConstructors(BindingFlags.Public | BindingFlags.Instance))
					{
						applicant = true;
						matchedParams = 0;
						ctorParams = ctor.GetParameters();

						if (ctorParams.Length == 1 && IsEqualIgnoreNullability(typeof(From), ctorParams[0].ParameterType))
						{
							winnerCtor = ctor;
							isCopyConstructor = true;
							break;
						}

						foreach (var p in ctorParams)
						{
							if (fromProps.Any(x => IsEqualIgnoreFirstCharCase(x.Name, p.Name!) && IsEqualIgnoreNullability(x.PropertyType, p.ParameterType)) ||
									fromFields.Any(x => IsEqualIgnoreFirstCharCase(x.Name, p.Name!) && IsEqualIgnoreNullability(x.FieldType, p.ParameterType)))
							{
								matchedParams++;
							}
							else if (!p.HasDefaultValue)
							{
								applicant = false;
								break;
							}
						}

						if (applicant && (winnerCtor == null || matchedParams > score))
						{
							winnerCtor = ctor;
							score = matchedParams;
						}
					}

					if (winnerCtor == null)
					{
						throw new InvalidOperationException("No suitable constructor.");
					}
				}
				#endregion

				var pairs = new List<(MemberInfo from, MemberInfo to)>(toProps.Length + toFields.Length);
				PropertyInfo? fromProp;
				FieldInfo? fromField;

				#region Match properties and fields
				foreach (var toProp in toProps)
				{
					fromProp = fromProps.FirstOrDefault(x => x.Name == toProp.Name);
					if (fromProp != null)
					{
						if (IsEqualIgnoreNullability(fromProp.PropertyType, toProp.PropertyType))
						{
							pairs.Add((fromProp, toProp));
						}
						continue;
					}

					fromField = fromFields.FirstOrDefault(x => x.Name == toProp.Name);
					if (fromField != null)
					{
						if (IsEqualIgnoreNullability(fromField.FieldType, toProp.PropertyType))
						{
							pairs.Add((fromField, toProp));
						}
						continue;
					}
				}
				foreach (var toField in toFields)
				{
					fromProp = fromProps.FirstOrDefault(x => x.Name == toField.Name);
					if (fromProp != null)
					{
						if (IsEqualIgnoreNullability(fromProp.PropertyType, toField.FieldType))
						{
							pairs.Add((fromProp, toField));
						}
						continue;
					}

					fromField = fromFields.FirstOrDefault(x => x.Name == toField.Name);
					if (fromField != null)
					{
						if (IsEqualIgnoreNullability(fromField.FieldType, toField.FieldType))
						{
							pairs.Add((fromField, toField));
						}
						continue;
					}
				}
				#endregion

				var exFromParam = Expression.Parameter(typeof(From), "from");

				NewExpression exNew;
				#region Build new expression
				{
					var ctorParams = winnerCtor.GetParameters();
					var args = new List<Expression>(ctorParams.Length);

					if (isCopyConstructor)
					{
						if (typeof(From) != ctorParams[0].ParameterType)
						{
							args.Add(Expression.ConvertChecked(exFromParam, ctorParams[0].ParameterType));
						}
						else
						{
							args.Add(exFromParam);
						}
					}
					else
					{
						foreach (var p in ctorParams)
						{
							fromProp = fromProps.FirstOrDefault(x => IsEqualIgnoreFirstCharCase(x.Name, p.Name!) && IsEqualIgnoreNullability(x.PropertyType, p.ParameterType));
							if (fromProp != null)
							{
								if (fromProp.PropertyType != p.ParameterType)
								{
									args.Add(Expression.ConvertChecked(Expression.Property(exFromParam, fromProp), p.ParameterType));
								}
								else
								{
									args.Add(Expression.Property(exFromParam, fromProp));
								}
								continue;
							}

							fromField = fromFields.FirstOrDefault(x => IsEqualIgnoreFirstCharCase(x.Name, p.Name!) && IsEqualIgnoreNullability(x.FieldType, p.ParameterType));
							if (fromField != null)
							{
								if (fromField.FieldType != p.ParameterType)
								{
									args.Add(Expression.ConvertChecked(Expression.Field(exFromParam, fromField), p.ParameterType));
								}
								else
								{
									args.Add(Expression.Field(exFromParam, fromField));
								}
								continue;
							}

							System.Diagnostics.Debug.Assert(p.HasDefaultValue);

							args.Add(Expression.Constant(p.DefaultValue, p.ParameterType));
						}
					}

					exNew = Expression.New(winnerCtor, args);
				}
				#endregion

				var exToVar = Expression.Variable(typeof(To), "to");
				var list = new List<Expression>(pairs.Count + 2) { Expression.Assign(exToVar, exNew) };
				
				#region Build assign expressions for properties and fields
				if (!isCopyConstructor) 
				{
					Expression exFrom;
					foreach (var p in pairs)
					{
						if (p.from is PropertyInfo pi)
						{
							exFrom = Expression.Property(exFromParam, pi);

							if (p.to is PropertyInfo pi2)
							{
								if (pi.PropertyType != pi2.PropertyType)
								{
									exFrom = Expression.ConvertChecked(exFrom, pi2.PropertyType);
								}

								list.Add(Expression.Assign(Expression.Property(exToVar, pi2), exFrom));
							}
							else if (p.to is FieldInfo fi2)
							{
								if (pi.PropertyType != fi2.FieldType)
								{
									exFrom = Expression.ConvertChecked(exFrom, fi2.FieldType);
								}

								list.Add(Expression.Assign(Expression.Field(exToVar, fi2), exFrom));
							}
						}
						else if (p.from is FieldInfo fi)
						{
							exFrom = Expression.Field(exFromParam, (FieldInfo)p.from);

							if (p.to is PropertyInfo pi2)
							{
								if (fi.FieldType != pi2.PropertyType)
								{
									exFrom = Expression.ConvertChecked(exFrom, pi2.PropertyType);
								}

								list.Add(Expression.Assign(Expression.Property(exToVar, pi2), exFrom));
							}
							else if (p.to is FieldInfo fi2)
							{
								if (fi.FieldType != fi2.FieldType)
								{
									exFrom = Expression.ConvertChecked(exFrom, fi2.FieldType);
								}

								list.Add(Expression.Assign(Expression.Field(exToVar, fi2), exFrom));
							}
						}
					}

					list.Add(exToVar);
				}
				#endregion

				var block = Expression.Block(new[] { exToVar }, list);
				var lambda = Expression.Lambda<Func<From, To>>(block, exFromParam);

				return lambda.Compile();
			}
			catch (Exception ex)
			{
				return _ => throw EX.Shared.Make.FailedToMapObjectFromTo(typeof(From).ToPretty(), typeof(To).ToPretty(), ex.Message);
			}
		}

		private static bool IsEqualIgnoreNullability(Type a, Type b)
		{
			if (a == b) return true;
			if (a.IsValueType && Nullable.GetUnderlyingType(a) == b) return true;
			if (b.IsValueType && Nullable.GetUnderlyingType(b) == a) return true;
			return false;
		}

		private static bool IsEqualIgnoreFirstCharCase(string s1, string s2)
		{
			if (s1 == s2) return true;
			if (s1.Length > 0 && s2.Length > 0 && s1.AsSpan(1).SequenceEqual(s2.AsSpan(1)) && char.ToLowerInvariant(s1[0]) == char.ToLowerInvariant(s2[0])) return true;
			return false;
		}
	}
}