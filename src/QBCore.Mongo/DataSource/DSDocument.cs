using System.Reflection;
using MongoDB.Bson.Serialization;
using QBCore.Extensions.Linq.Expressions;

namespace QBCore.DataSource;

internal sealed class DSDocument : IDSDocument
{
	public Type DocumentType { get; init; }
	public IReadOnlyDictionary<string, IDataEntry> DataEntries { get; init; }
	public IDataEntry? IdField { get; init; }
	public IDataEntry? DateCreatedField { get; init; }
	public IDataEntry? DateModifiedField { get; init; }
	public IDataEntry? DateUpdatedField { get; init; }
	public IDataEntry? DateDeletedField { get; init; }

	public DSDocument(Type documentType)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}

		DocumentType = documentType;
		DataEntries = LoadDataEntries(documentType).ToDictionary(x => x.Name);
		foreach (var de in DataEntries)
		{
			if (de.Value.Flags.HasFlag(DataEntryFlags.IdField))
			{
				IdField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.DateCreatedField))
			{
				DateCreatedField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.DateModifiedField))
			{
				DateModifiedField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.DateUpdatedField))
			{
				DateUpdatedField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.DateDeletedField))
			{
				DateDeletedField = de.Value;
			}
		}
	}

	private static List<IDataEntry> LoadDataEntries(Type documentType)
	{
		var classMap = BsonClassMap.LookupClassMap(documentType);
		var list = new List<IDataEntry>();
		DataEntryFlags flags;
		int order;
		Func<object, object?> getter;
		Action<object, object?>? setter;
		BsonMemberMap? memberMap;
		

		foreach (var propertyInfo in documentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty))
		{
			flags = DataEntryFlags.None;

			if (propertyInfo.IsDefined(typeof(DeIdAttribute), true))
			{
				flags |= DataEntryFlags.IdField;
			}
			if (propertyInfo.IsDefined(typeof(DeCreatedAttribute), true))
			{
				flags |= DataEntryFlags.DateCreatedField;
			}
			else if (propertyInfo.IsDefined(typeof(DeModifiedAttribute), true))
			{
				flags |= DataEntryFlags.DateModifiedField;
			}
			else if (propertyInfo.IsDefined(typeof(DeUpdatedAttribute), true))
			{
				flags |= DataEntryFlags.DateUpdatedField;
			}
			else if (propertyInfo.IsDefined(typeof(DeDeletedAttribute), true))
			{
				flags |= DataEntryFlags.DateDeletedField;
			}
			if (propertyInfo.IsDefined(typeof(DeForeignIdAttribute), true))
			{
				flags |= DataEntryFlags.ForeignId;
			}

			if (flags == DataEntryFlags.None)
			{
				if (propertyInfo.SetMethod == null && !propertyInfo.IsDefined(typeof(DeDataEntryAttribute), true))
				{
					continue;
				}
				if (propertyInfo.IsDefined(typeof(DeIgnoreAttribute), true))
				{
					continue;
				}
			}

			order = propertyInfo.GetCustomAttribute<DeOrderAttribute>()?.Order ?? 0;
			memberMap = classMap.GetMemberMap(propertyInfo.Name);
			getter = memberMap?.Getter ?? propertyInfo.MakeCommonGetter().Compile();
			setter = memberMap?.Setter ?? propertyInfo.MakeCommonSetter()?.Compile();

			list.Add(new DataEntry
			{
				Name = propertyInfo.Name,
				Flags = flags,
				DocumentType = documentType,
				DataEntryType = propertyInfo.PropertyType,
				UnderlyingType = propertyInfo.PropertyType.GetUnderlyingSystemType(),
				IsNullable = propertyInfo.IsNullable(),
				Order = order,
				Getter = getter,
				Setter = setter,
				MemberMap = memberMap
			});
		}

		foreach (var fieldInfo in documentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField))
		{
			flags = DataEntryFlags.None;

			if (fieldInfo.IsDefined(typeof(DeIdAttribute), true))
			{
				flags |= DataEntryFlags.IdField;
			}
			if (fieldInfo.IsDefined(typeof(DeCreatedAttribute), true))
			{
				flags |= DataEntryFlags.DateCreatedField;
			}
			else if (fieldInfo.IsDefined(typeof(DeModifiedAttribute), true))
			{
				flags |= DataEntryFlags.DateModifiedField;
			}
			else if (fieldInfo.IsDefined(typeof(DeUpdatedAttribute), true))
			{
				flags |= DataEntryFlags.DateUpdatedField;
			}
			else if (fieldInfo.IsDefined(typeof(DeDeletedAttribute), true))
			{
				flags |= DataEntryFlags.DateDeletedField;
			}
			if (fieldInfo.IsDefined(typeof(DeForeignIdAttribute), true))
			{
				flags |= DataEntryFlags.ForeignId;
			}

			if (flags == DataEntryFlags.None && !fieldInfo.IsDefined(typeof(DeDataEntryAttribute), true))
			{
				continue;
			}

			order = fieldInfo.GetCustomAttribute<DeOrderAttribute>()?.Order ?? 0;
			memberMap = classMap.GetMemberMap(fieldInfo.Name);
			getter = memberMap?.Getter ?? fieldInfo.MakeCommonGetter().Compile();
			setter = memberMap?.Setter ?? fieldInfo.MakeCommonSetter()?.Compile();

			list.Add(new DataEntry
			{
				Name = fieldInfo.Name,
				Flags = flags,
				DocumentType = documentType,
				DataEntryType = fieldInfo.FieldType,
				UnderlyingType = fieldInfo.FieldType.GetUnderlyingSystemType(),
				IsNullable = fieldInfo.IsNullable(),
				Order = order,
				MemberMap = classMap.GetMemberMap(fieldInfo.Name)
			});
		}

		list.OrderBy(x => x.Order);
		return list;
	}
}