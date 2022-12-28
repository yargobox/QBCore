using System.Reflection;

namespace QBCore.DataSource;

internal sealed class SqlDocumentInfo : DSDocumentInfo
{
	public SqlDocumentInfo(Type documentType) : base(documentType) { }

	protected override DEInfo CreateDataEntryInfo(MemberInfo memberInfo, DataEntryFlags flags, ref object? methodSharedContext)
	{
		return new SqlDEInfo(this, memberInfo, flags);
	}
}