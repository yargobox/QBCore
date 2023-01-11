using System.Runtime.CompilerServices;

namespace QBCore.Extensions.Internals;

public static class ExtensionsForEXDataSource
{
	public static NotSupportedException DataSourceDoesNotSupportOperation(this EX.DataSource _, string dataSource, string queryBuilderType)
		=> new NotSupportedException($"DataSource '{dataSource}' does not support the {queryBuilderType} operation.");

	public static InvalidOperationException EventHandlerIsAlreadySetMoreThanOneIsNotSupported(this EX.DataSource _, [CallerMemberName] string memberName = "")
		=> new InvalidOperationException($"Event handler '{memberName}' is already set. More than one handler is not supported.");
	public static NotSupportedException PropertyOrMethodNotSupportedByThisCursor(this EX.DataSource _, [CallerMemberName] string memberName = "")
		=> new NotSupportedException($"Property or method '{memberName}' is not supported by this cursor!");
	public static InvalidOperationException PropertyOrMethodIsNotAvailableYet(this EX.DataSource _, [CallerMemberName] string memberName = "")
		=> new InvalidOperationException($"Property or method {nameof(memberName)} is not available yet!");
}