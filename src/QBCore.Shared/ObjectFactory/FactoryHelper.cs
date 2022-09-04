using System.Reflection;

namespace QBCore.ObjectFactory;

public static class FactoryHelper
{
	private static readonly MethodInfo _findBuilderMethodInfo =
		typeof(FactoryHelper).GetMethod("FindBuilder", 1, new Type[] { typeof(Type), typeof(string) })!;

	public static Action<TBuilderActionParam> FindRequiredBuilder<TBuilderActionParam>(Type source, string? methodOrField) where TBuilderActionParam : notnull
	{
		return FindBuilder<TBuilderActionParam>(source, methodOrField) ??
			throw new InvalidOperationException($"Could not find builder '{source.ToPretty()}.{methodOrField ?? "???"}{typeof(TBuilderActionParam).Name})'.");
	}

	public static Delegate? FindRequiredBuilder(Type builderActionParam, Type source, string? methodOrField)
	{
		return FindBuilder(builderActionParam, source, methodOrField) ??
			throw new InvalidOperationException($"Could not find builder '{source.ToPretty()}.{methodOrField ?? "???"}{builderActionParam.Name})'.");
	}

	public static Delegate? FindBuilder(Type builderActionParam, Type source, string? methodOrField)
	{
		if (Nullable.GetUnderlyingType(builderActionParam) != null)
		{
			throw new InvalidOperationException($"The {nameof(builderActionParam)} parameter value cannot be a nullable type, namely {builderActionParam.ToPretty()}.");
		}

		var method = _findBuilderMethodInfo.MakeGenericMethod(builderActionParam);
		return (Delegate?)method.Invoke(null, new object?[] { source, methodOrField });
	}

	public static Action<TBuilderActionParam>? FindBuilder<TBuilderActionParam>(Type source, string? methodOrField) where TBuilderActionParam : notnull
	{
		MethodInfo? methodInfo = null;
		FieldInfo? fieldInfo = null;

		if (methodOrField != null)
		{
			methodInfo = source.GetMethod(methodOrField, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[] { typeof(TBuilderActionParam) });
			if (methodInfo == null)
			{
				fieldInfo = source.GetField(methodOrField, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					if (!fieldInfo.IsInitOnly || fieldInfo.FieldType != typeof(Action<TBuilderActionParam>))
					{
						fieldInfo = null;
					}
				}
			}

			if (methodInfo == null && fieldInfo == null)
			{
				return null;
			}
		}
		else
		{
			var methodInfos =
				source.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(x =>
					!x.IsGenericMethod &&
					x.ReturnType == typeof(void) &&
					AreValidBuilderParameters<TBuilderActionParam>(x.GetParameters()))
				.ToArray();

			if (methodInfos.Length > 1)
			{
				throw new InvalidOperationException($"There is more than one builder '{source.ToPretty()}.{{???}}.({typeof(TBuilderActionParam).Name})'.");
			}

			var fieldInfos =
				source.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(x =>
					x.IsInitOnly &&
					x.FieldType == typeof(Action<TBuilderActionParam>))
				.ToArray();
			
			if (fieldInfos.Length > 1 || (fieldInfos.Length > 0 && methodInfos.Length > 0))
			{
				throw new InvalidOperationException($"There is more than one builder '{source.ToPretty()}.{{???}}.({typeof(TBuilderActionParam).Name})'.");
			}
			
			if (methodInfos.Length < 1 && fieldInfos.Length < 1)
			{
				return null;
			}
			else if (methodInfos.Length == 1)
			{
				methodInfo = methodInfos[0];
			}
			else/* if (fieldInfos.Length == 1) */
			{
				fieldInfo = fieldInfos[0];
			}
		}

		if (methodInfo != null)
		{
			return void (TBuilderActionParam building) => methodInfo.Invoke(null, new object[] { building });
		}
		else
		{
			var builder = (Action<TBuilderActionParam>?) fieldInfo!.GetValue(null);
			if (builder == null)
			{
				throw new InvalidOperationException($"Builder '{source.ToPretty()}.{fieldInfo.Name}.({typeof(TBuilderActionParam).Name})' is not set.");
			}

			return void (TBuilderActionParam building) => builder(building);
		}
	}

	private static bool AreValidBuilderParameters<TBuilder>(ParameterInfo[] parameters)
	{
		return parameters.Length == 1 && parameters[0].ParameterType == typeof(TBuilder);
	}
}