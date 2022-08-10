using MongoDB.Bson.Serialization;

namespace QBCore.DataSource;

internal sealed class DataEntry : IDataEntry
{
	public string Name { get; init; } = null!;
	public DataEntryFlags Flags { get; init; }
	public Type DocumentType { get; init; } = null!;
	public Type DataEntryType { get; init; } = null!;
	public Type UnderlyingType { get; init; } = null!;
	public bool IsNullable { get; init; }
	public int Order { get; init; }
	public Func<object, object?> Getter { get; init; } = null!;
	public Action<object, object?>? Setter { get; init; }
	public string? DBSideName => MemberMap?.ElementName;
	public BsonMemberMap? MemberMap { get; init; }
}