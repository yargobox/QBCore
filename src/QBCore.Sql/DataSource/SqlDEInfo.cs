using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace QBCore.DataSource;

internal sealed class SqlDEInfo : DEInfo
{
	public string DBSideName { get; }

	public SqlDEInfo(SqlDocumentInfo document, MemberInfo memberInfo, DataEntryFlags flags)
		: base(document, memberInfo, flags)
	{
		var dataEntryAttr = memberInfo.GetCustomAttribute<ColumnAttribute>(false);

		DBSideName = dataEntryAttr?.Name ?? memberInfo.Name;
	}
}