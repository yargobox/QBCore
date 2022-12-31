namespace QBCore.Extensions.Internals;

public static class ExtensionsForEXQueryBuilder
{
	public static ArgumentNullException IdentifierValueNotSpecified(this EX.QueryBuilder _, string paramName)
		=> new ArgumentNullException(paramName, "Identifier value not specified.");

	public static ArgumentNullException DocumentNotSpecified(this EX.QueryBuilder _, string paramName)
		=> new ArgumentNullException(paramName, "Document not specified.");
	
	public static InvalidOperationException SpecifiedTransactionOpenedForDifferentConnection(this EX.QueryBuilder _)
		=> new InvalidOperationException("The specified transaction is opened for the different connection.");
	
	public static KeyNotFoundException OperationFailedNoSuchRecord(this EX.QueryBuilder _, string queryBuilderType, string? id, string? location)
		=> new KeyNotFoundException($"{queryBuilderType} operation failed: no such record as '{id}' or other conditions are not satisfied in '{location}'.");
	
	public static KeyNotFoundException OperationFailedNoSuchRecord(this EX.QueryBuilder _, string queryBuilderType, string? id, string? location, Exception ex)
		=> new KeyNotFoundException($"{queryBuilderType} operation failed: no such record as '{id}' or other conditions are not satisfied in '{location}'.", ex);
	
	public static InvalidOperationException OperationFailedNoAcknowledgment(this EX.QueryBuilder _, string queryBuilderType, string? id, string? location)
		=> new InvalidOperationException($"{queryBuilderType} operation failed: no acknowledgment for the operation on such record as '{id}' in '{location}'.");
	
	public static NotSupportedException QueryBuilderOperationNotSupported(this EX.QueryBuilder _, string dataLayerName, string queryBuilderType, string? containerOperation)
		=> new NotSupportedException($"{dataLayerName} {queryBuilderType} query builder does not support an operation like '{containerOperation}'.");
	
	public static InvalidOperationException QueryBuilderMustHaveAtLeastOneCondition(this EX.QueryBuilder _, string dataLayerName, string queryBuilderType)
		=> new InvalidOperationException($"{dataLayerName} {queryBuilderType} query builder must have at least one condition.");
	
	public static InvalidOperationException DocumentDoesNotHaveIdDataEntry(this EX.QueryBuilder _, string documentType)
		=> new InvalidOperationException($"Document '{documentType}' does not have an id data entry.");
	
	public static InvalidOperationException DataEntryDoesNotHaveSetter(this EX.QueryBuilder _, string documentType, string dataEntry)
		=> new InvalidOperationException($"Data entry '{documentType}.{dataEntry}' does not have a setter.");

	public static InvalidOperationException DocumentDoesNotHaveDeletedDataEntry(this EX.QueryBuilder _, string documentType)
		=> new InvalidOperationException($"Document '{documentType}' does not have a date deletion data entry.");
}