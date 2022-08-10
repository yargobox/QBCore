namespace QBCore.DataSource;

public interface IDSDocument
{
	Type DocumentType { get; }
	
	IReadOnlyDictionary<string, IDataEntry> DataEntries { get; }

	IDataEntry? IdField { get; }
	IDataEntry? DateCreatedField { get; }
	IDataEntry? DateModifiedField { get; }
	IDataEntry? DateUpdatedField { get; }
	IDataEntry? DateDeletedField { get; }
}