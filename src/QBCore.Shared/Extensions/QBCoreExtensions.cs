namespace QBCore.Extensions;

public static class QBCoreExtensions
{
	public static string ToPretty(this Type type, int recursionLevel = 8, bool expandNullable = false)
	{
		if (type.IsArray)
		{
			return $"{ToPretty(type.GetElementType()!, recursionLevel, expandNullable)}[]";
		}

		if (type.IsGenericType)
		{
			// find generic type name
			var genericTypeName = type.GetGenericTypeDefinition().Name;
			var index = genericTypeName.IndexOf('`');
			if (index != -1)
			{
				genericTypeName = genericTypeName.Substring(0, index);
			}

			// retrieve generic type aguments
			var argNames = new List<string>();
			var genericTypeArgs = type.GetGenericArguments();
			foreach (var genericTypeArg in genericTypeArgs)
			{
				argNames.Add(
					recursionLevel > 0
						? ToPretty(genericTypeArg, recursionLevel - 1, expandNullable)
						: "?");
			}

			// if type is nullable and want compact notation '?'
			if (!expandNullable && Nullable.GetUnderlyingType(type) != null)
			{
				return $"{argNames[0]}?";
			}

			// compose common generic type format "T<T1, T2, ...>"
			return $"{genericTypeName}<{string.Join(", ", argNames)}>";
		}

		return type.Name;
	}

	public static T? GetInstance<T>(this IServiceProvider provider)
		=> (T?)provider.GetService(typeof(T));

	public static T GetRequiredInstance<T>(this IServiceProvider provider)
		=> (T)provider.GetRequiredInstance(typeof(T));
	
	public static object GetRequiredInstance(this IServiceProvider provider, Type serviceType)
		=> provider.GetService(serviceType) ?? throw new InvalidOperationException($"Unable to resolve service for type '{serviceType.ToPretty()}'.");
	
	public static Type? GetSubclassOf<T>(this Type @this)
		=> GetSubclassOf(@this, typeof(T));

	public static Type? GetSubclassOf(this Type @this, Type test)
	{
		Type? type = @this;
		while (type != null)
		{
			if (test.IsGenericTypeDefinition)
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == test)
				{
					return type;
				}
			}
			else if (type == test)
			{
				return type;
			}

			type = type.BaseType;
		}
		return null;
	}
}