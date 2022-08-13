using System.Reflection;
using QBCore.Extensions.Linq.Expressions;

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

public abstract class DataEntry
{
	public readonly string Name;
	public readonly DSDocumentInfo Document;
	public readonly DataEntryFlags Flags;
	public readonly Type DataEntryType;
	public readonly Type UnderlyingType;
	public readonly bool IsNullable;
	public readonly int Order;
	public Func<object, object?> Getter => _getter ?? (_getter = MakeGetter());
	protected Func<object, object?>? _getter;
	public Action<object, object?>? Setter => _setter ?? (_setter = MakeSetter());
	protected Action<object, object?>? _setter;

	protected DataEntry(DSDocumentInfo document, MemberInfo memberInfo, DataEntryFlags flags)
	{
		if (document == null)
		{
			throw new ArgumentNullException(nameof(document));
		}
		if (memberInfo == null)
		{
			throw new ArgumentNullException(nameof(memberInfo));
		}

		Name = memberInfo.Name;
		Flags = flags;
		Document = document;

		if (memberInfo is PropertyInfo propertyInfo)
		{
			DataEntryType = propertyInfo.PropertyType;
			UnderlyingType = propertyInfo.PropertyType.GetUnderlyingSystemType();
			IsNullable = propertyInfo.IsNullable();
		}
		else if (memberInfo is FieldInfo fieldInfo)
		{
			DataEntryType = fieldInfo.FieldType;
			UnderlyingType = fieldInfo.FieldType.GetUnderlyingSystemType();
			IsNullable = fieldInfo.IsNullable();
		}
		else
		{
			throw new ArgumentException(nameof(memberInfo));
		}

		Order = memberInfo.GetCustomAttribute<DeOrderAttribute>()?.Order ?? 0;
	}

	protected virtual Func<object, object?> MakeGetter(MemberInfo? memberInfo = null)
	{
		if (memberInfo == null)
		{
			memberInfo = (MemberInfo?)Document.DocumentType.GetProperty(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
									?? Document.DocumentType.GetField(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		if (memberInfo is PropertyInfo propertyInfo)
		{
			return propertyInfo.MakeCommonGetter().Compile();
		}
		else if (memberInfo is FieldInfo fieldInfo)
		{
			return fieldInfo.MakeCommonGetter().Compile();
		}

		throw new ArgumentException(nameof(memberInfo));
	}

	protected virtual Action<object, object?>? MakeSetter(MemberInfo? memberInfo = null)
	{
		if (memberInfo == null)
		{
			memberInfo = (MemberInfo?)Document.DocumentType.GetProperty(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
									?? Document.DocumentType.GetField(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		if (memberInfo is PropertyInfo propertyInfo)
		{
			return propertyInfo.MakeCommonSetter()?.Compile();
		}
		else if (memberInfo is FieldInfo fieldInfo)
		{
			return fieldInfo.MakeCommonSetter()?.Compile();
		}
		
		throw new ArgumentException(nameof(memberInfo));
	}
}