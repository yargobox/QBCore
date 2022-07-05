using System.Reflection;

namespace QBCore.Extensions.Runtime;

public static class ExtensionsForRuntime
{
	public static T? GetCustomAttributeOfType<T>(this PropertyInfo propertyInfo, bool inherit)
	{
		var t = typeof(T);
		return (T?) propertyInfo.GetCustomAttributes(inherit).OfType<T>().Where(x => x?.GetType() == t).FirstOrDefault();
	}
	public static T[] GetCustomAttributesOfType<T>(this PropertyInfo propertyInfo, bool inherit)
	{
		var t = typeof(T);
		return propertyInfo.GetCustomAttributes(inherit).OfType<T>().Where(x => x?.GetType() == t).ToArray();
	}
}