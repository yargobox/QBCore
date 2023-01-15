namespace QBCore.Extensions.Internals;

public static class ExtensionsForEXShared
{
	public static ArgumentException NoCoercionOperatorOrTypeConversionAvailable(this EX.Shared _, string fromType, string toType)
		=> new ArgumentException($"No coercion operator or type conversion available from {fromType} to {toType}.");
	
	public static InvalidOperationException FailedToMapObjectFromTo(this EX.Shared _, string fromType, string toType, string details)
		=> new InvalidOperationException($"Failed to map an object from {fromType} to {toType}: {details}");

	public static ArgumentNullException ValueTypeCannotBeNull(this EX.Shared _, string valueType)
		=> new ArgumentNullException($"Value type '{valueType}' cannot be null.");
}