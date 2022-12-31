namespace QBCore.Extensions.Internals;

public static class ExtensionsForEXShared
{
	public static ArgumentException NoCoercionOperatorOrTypeConversionAvailable(this EX.Shared _, string fromType, string toType)
		=> new ArgumentException($"No coercion operator or type conversion available from {fromType} to {toType}.");

	public static ArgumentNullException ValueTypeCannotBeNull(this EX.Shared _, string valueType)
		=> new ArgumentNullException($"Value type '{valueType}' cannot be null.");
}