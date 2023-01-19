using System.Reflection;

namespace QBCore.DataSource;

internal sealed class SqlDEInfo : DEInfo
{
	public string DBSideName { get; }

	public SqlDEInfo(SqlDocumentInfo document, MemberInfo memberInfo, DataEntryFlags flags)
		: base(document, memberInfo, flags)
	{
		var dataEntryAttr = memberInfo.GetCustomAttribute<DeDataEntryAttribute>(false);

		DBSideName = dataEntryAttr?.DBSideName ?? memberInfo.Name;
	}
}