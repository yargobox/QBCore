using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QBCore.Extensions.Reflection;

public static class ExtensionsForReflection
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

	[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
	public static Type? GetSubclassOf<T>(this Type @this)
	{
		return GetSubclassOf(@this, typeof(T));
	}

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

	public static Type? GetInterfaceOf(this Type @this, Type test)
	{
		if (test.IsGenericTypeDefinition)
		{
			return @this.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == test);
		}
		else
		{
			return @this.GetInterfaces().FirstOrDefault(i => i == test);
		}
	}

	public static IEnumerable<Type> GetInterfacesOf(this Type @this, Type test)
	{
		if (test.IsGenericTypeDefinition)
		{
			return @this.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == test);
		}
		else
		{
			return @this.GetInterfaces().Where(i => i == test);
		}
	}

	public static bool IsNullableValueType(this Type type)
	{
		return type.IsValueType && Nullable.GetUnderlyingType(type) is not null;
	}

	public static bool IsTuple(this Type type)
	{
		return type.IsValueType && type.FullName?.StartsWith("System.ValueTuple`") == true;
	}

	public static bool IsAnonymous(this Type type)
	{
		return
			type.Namespace == null &&
			(type.Name.StartsWith("<>") || type.Name.StartsWith("VB$")) &&
			type.Name.Contains("AnonymousType") &&
			type.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
			type.Attributes.HasFlag(TypeAttributes.NotPublic);
	}

	public static Type GetUnderlyingType(this Type type)
	{
		return type.IsValueType ? Nullable.GetUnderlyingType(type) ?? type : type;
	}

	public static bool IsNullable(this PropertyInfo propertyInfo)
	{
		var propertyType = propertyInfo.PropertyType;

		if (propertyType.IsValueType)
		{
			return Nullable.GetUnderlyingType(propertyType) != null;
		}
		else if (propertyType.IsGenericType)
		{
			return propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		var classNullableContextAttribute = propertyInfo?.DeclaringType?.CustomAttributes
			.FirstOrDefault(c => c.AttributeType.Name == "NullableContextAttribute");

		var classNullableContext = classNullableContextAttribute
			?.ConstructorArguments
			.First(ca => ca.ArgumentType.Name == "Byte")
			.Value;

		// EDIT: This logic is not correct for nullable generic types
		var propertyNullableContext = propertyInfo?.CustomAttributes
			.FirstOrDefault(c => c.AttributeType.Name == "NullableAttribute")
			?.ConstructorArguments
			.First(ca => ca.ArgumentType.Name == "Byte")
			.Value;

		// If the property does not have the nullable attribute then it's 
		// nullability is determined by the declaring class 
		propertyNullableContext ??= classNullableContext;

		// If NullableContextAttribute on class is not set and the property
		// does not have the NullableAttribute, then the proeprty is non nullable
		if (propertyNullableContext == null)
		{
			return true;
		}

		// nullableContext == 0 means context is null oblivious (Ex. Pre C#8)
		// nullableContext == 1 means not nullable
		// nullableContext == 2 means nullable
		switch ((byte)propertyNullableContext)
		{
			case 1:// NonNullableContextValue
				return false;
			case 2:// NullableContextValue
				return true;
			default:
				throw new NotSupportedException();
		}
	}
	public static bool IsNullable(this FieldInfo fieldInfo)
	{
		var propertyType = fieldInfo.FieldType;

		if (propertyType.IsValueType)
		{
			return Nullable.GetUnderlyingType(propertyType) != null;
		}
		else if (propertyType.IsGenericType)
		{
			return propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		var classNullableContextAttribute = fieldInfo?.DeclaringType?.CustomAttributes
			.FirstOrDefault(c => c.AttributeType.Name == "NullableContextAttribute");

		var classNullableContext = classNullableContextAttribute
			?.ConstructorArguments
			.First(ca => ca.ArgumentType.Name == "Byte")
			.Value;

		// EDIT: This logic is not correct for nullable generic types
		var propertyNullableContext = fieldInfo?.CustomAttributes
			.FirstOrDefault(c => c.AttributeType.Name == "NullableAttribute")
			?.ConstructorArguments
			.First(ca => ca.ArgumentType.Name == "Byte")
			.Value;

		// If the property does not have the nullable attribute then it's 
		// nullability is determined by the declaring class 
		propertyNullableContext ??= classNullableContext;

		// If NullableContextAttribute on class is not set and the property
		// does not have the NullableAttribute, then the proeprty is non nullable
		if (propertyNullableContext == null)
		{
			return true;
		}

		// nullableContext == 0 means context is null oblivious (Ex. Pre C#8)
		// nullableContext == 1 means not nullable
		// nullableContext == 2 means nullable
		switch ((byte)propertyNullableContext)
		{
			case 1:// NonNullableContextValue
				return false;
			case 2:// NullableContextValue
				return true;
			default:
				throw new NotSupportedException();
		}
	}

	public static Type GetPropertyOrFieldType(this MemberInfo memberInfo)
	{
		if (memberInfo is PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType;
		}
		else if (memberInfo is FieldInfo fieldInfo)
		{
			return fieldInfo.FieldType;
		}

		if (memberInfo == null)
		{
			throw new ArgumentNullException(nameof(memberInfo));
		}
		throw new ArgumentException(nameof(memberInfo));
	}

	public static Type GetPropertyOrFieldDeclaringType(this MemberInfo memberInfo)
	{
		if (memberInfo is PropertyInfo propertyInfo)
		{
			return propertyInfo.DeclaringType!;
		}
		else if (memberInfo is FieldInfo fieldInfo)
		{
			return fieldInfo.DeclaringType!;
		}

		if (memberInfo == null)
		{
			throw new ArgumentNullException(nameof(memberInfo));
		}
		throw new ArgumentException(nameof(memberInfo));
	}

	public static object? GetDefaultValue(this Type type)
	{
		return type.IsValueType ? Activator.CreateInstance(type) : null;
	}

	public static Type? GetEnumerationItemType(this object? value)
	{
		if (value is not IEnumerable objectEnumeration)
		{
			return null;
		}

		foreach (var item in objectEnumeration)
		{
			if (item is null)
			{
				continue;
			}

			return item.GetType();
		}

		var types = value.GetType().GetInterfacesOf(typeof(IEnumerable<>)).Select(x => x.GetGenericArguments()[0]);
		
		return types.FirstOrDefault(x => x != typeof(object)) ?? types.FirstOrDefault();
	}
}