using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QBCore.DataSource;

internal sealed class EfCoreDEInfo : DEInfo
{
	public readonly IProperty? Property;
	public string? DBSideName => Property?.GetColumnName();

	public EfCoreDEInfo(EfCoreDocumentInfo document, MemberInfo memberInfo, DataEntryFlags flags, IEntityType? entityType)
		: base(document, memberInfo, flags)
	{
		Property = entityType?.FindProperty(memberInfo);
	}

	protected override Func<object, object?> MakeGetter(MemberInfo? memberInfo)
	{
		if (Property?.IsShadowProperty() == false)
		{
			var getter = Property.GetGetter();
			return (entity) => getter.GetClrValue(entity);
		}
		return base.MakeGetter(memberInfo);
	}

	protected override Action<object, object?>? MakeSetter(MemberInfo? memberInfo)
	{
		return base.MakeSetter(memberInfo);
	}
}