using System.Reflection;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSDocumentBuilder { }

public abstract class DSDocumentInfo
{
	protected sealed class DSDocumentBuilder : IDSDocumentBuilder { }

	public readonly Type DocumentType;
	public readonly IReadOnlyDictionary<string, DataEntry> DataEntries;
	public readonly DataEntry? IdField;
	public readonly DataEntry? DateCreatedField;
	public readonly DataEntry? DateModifiedField;
	public readonly DataEntry? DateUpdatedField;
	public readonly DataEntry? DateDeletedField;

	public DSDocumentInfo(Type documentType)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}

		DocumentType = documentType;

		PreBuild();

		DataEntry dataEntry;
		object? methodSharedContext = null;
		var dataEntries = new Dictionary<string, DataEntry>();

		foreach (var candidate in GetDataEntryCandidates(documentType))
		{
			dataEntry = CreateDataEntry(candidate.memberInfo, candidate.flags, ref methodSharedContext);
			dataEntries.Add(dataEntry.Name, dataEntry);
		}

		DataEntries = dataEntries;

		foreach (var de in DataEntries)
		{
			if (de.Value.Flags.HasFlag(DataEntryFlags.IdField))
			{
				IdField = de.Value;
			}
			
			if (de.Value.Flags.HasFlag(DataEntryFlags.DateCreatedField))
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

		PostBuild();
	}

	protected abstract DataEntry CreateDataEntry(MemberInfo propertyInfo, DataEntryFlags flags, ref object? methodSharedContext);
	
	protected virtual void PreBuild()
	{
		var builder = (Action<IDSDocumentBuilder>?) FactoryHelper.FindBuilder(typeof(IDSDocumentBuilder), DocumentType, null);
		if (builder != null)
		{
			builder(new DSDocumentBuilder());
		}
	}

	protected virtual void PostBuild() { }

	public static IEnumerable<(MemberInfo memberInfo, DataEntryFlags flags)> GetDataEntryCandidates(Type documentType)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}

		DataEntryFlags flags;

		foreach (var propertyInfo in documentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty))
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

			yield return (propertyInfo, flags);
		}

		foreach (var fieldInfo in documentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField))
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

			yield return (fieldInfo, flags);
		}
	}

	public static IEnumerable<Type> GetReferencingTypes(Type documentType, Func<Type, bool> documentTypesSelector, bool includeThisOne = false)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}
		if (documentTypesSelector == null)
		{
			throw new ArgumentNullException(nameof(documentTypesSelector));
		}

		if (!documentTypesSelector(documentType))
		{
			return Enumerable.Empty<Type>();
		}

		var pool = new Dictionary<Type, bool>();
		if (includeThisOne)
		{
			pool.Add(documentType, true);
		}

		GetReferencingTypes(documentType, documentTypesSelector, pool);

		return pool.Where(x => x.Value).Select(x => x.Key);
	}

	private static void GetReferencingTypes(Type documentType, Func<Type, bool> documentTypesSelector, Dictionary<Type, bool> pool)
	{
		bool isDocumentType;
		Type type;

		foreach (var candidate in DSDocumentInfo.GetDataEntryCandidates(documentType))
		{
			if (candidate.memberInfo is PropertyInfo propertyInfo)
			{
				type = propertyInfo.PropertyType;
			}
			else if (candidate.memberInfo is FieldInfo fieldInfo)
			{
				type = fieldInfo.FieldType;
			}
			else
			{
				continue;
			}

			isDocumentType = documentTypesSelector(type);
			if (pool.TryAdd(type, isDocumentType))
			{
				if (isDocumentType)
				{
					GetReferencingTypes(type, documentTypesSelector, pool);
				}
				else
				{
					foreach (var genericEnumerable in type.GetInterfacesOf(typeof(IEnumerable<>)))
					{
						type = genericEnumerable.GetGenericArguments()[0];
						isDocumentType = documentTypesSelector(type);
						if (pool.TryAdd(type, isDocumentType) && isDocumentType)
						{
							GetReferencingTypes(type, documentTypesSelector, pool);
						}
					}
				}
			}
		}
	}
}