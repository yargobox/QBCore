namespace QBCore.DataSource;

[Flags]
public enum DataEntryFlags
{
	None = 0,
	IdField = 1,
	DateCreatedField = 2,
	DateModifiedField = 4,
	DateUpdatedField = 8,
	DateDeletedField = 0x10,
	ForeignId = 0x20
}

public interface IDataEntry
{
	string Name { get; }
	DataEntryFlags Flags { get; }
	Type DocumentType { get; }
	Type DataEntryType { get; }
	Type UnderlyingType { get; }
	bool IsNullable { get; }
	int Order { get; }
	Func<object, object?> Getter { get; }
	Action<object, object?>? Setter { get; }
	string? DBSideName { get; }
}