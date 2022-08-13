using System.Reflection;
using MongoDB.Bson.Serialization;

namespace QBCore.DataSource;

internal sealed class MongoDataEntry : DataEntry
{
	private readonly BsonMemberMap? MemberMap;
	public string? DBSideName => MemberMap?.ElementName;

	public MongoDataEntry(MongoDocumentInfo document, MemberInfo memberInfo, DataEntryFlags flags, BsonClassMap classMap)
		: base(document, memberInfo, flags)
		=> MemberMap = classMap.GetMemberMap(Name);

	protected override Func<object, object?> MakeGetter(MemberInfo? memberInfo)
		=> MemberMap?.Getter ?? base.MakeGetter(memberInfo);

	protected override Action<object, object?>? MakeSetter(MemberInfo? memberInfo)
		=> MemberMap?.Setter ?? base.MakeSetter(memberInfo);
}