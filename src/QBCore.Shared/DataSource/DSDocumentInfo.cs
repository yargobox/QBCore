using System.Reflection;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDSDocumentBuilder { }

public abstract class DSDocumentInfo
{
	protected sealed class DSDocumentBuilder : IDSDocumentBuilder { }

	public readonly Type DocumentType;
	public readonly IReadOnlyDictionary<string, DEInfo> DataEntries;
	public readonly DEInfo? IdField;
	public readonly DEInfo? DateCreatedField;
	public readonly DEInfo? DateModifiedField;
	public readonly DEInfo? DateUpdatedField;
	public readonly DEInfo? DateDeletedField;

	public DSDocumentInfo(Type documentType)
	{
		if (documentType == null)
		{
			throw new ArgumentNullException(nameof(documentType));
		}

		DocumentType = documentType;

		PreBuild();

		DEInfo dataEntryInfo;
		object? methodSharedContext = null;
		var dataEntries = new Dictionary<string, DEInfo>();

		foreach (var candidate in GetDataEntryCandidates(documentType))
		{
			dataEntryInfo = CreateDataEntryInfo(candidate.memberInfo, candidate.flags, ref methodSharedContext);
			dataEntries.Add(dataEntryInfo.Name, dataEntryInfo);
		}

		var notFoundDependancyName = dataEntries
			.Values
			.Where(x => x.DependsOn != null)
			.SelectMany(x => x.DependsOn!)
			.FirstOrDefault(x => !dataEntries.ContainsKey(x));
		if (notFoundDependancyName != null)
		{
			throw new InvalidOperationException($"Data entry '{notFoundDependancyName}' not found in document {DocumentType.ToPretty()} specified in one of its attributes {nameof(DeDependsOnAttribute)}.");
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

	protected abstract DEInfo CreateDataEntryInfo(MemberInfo propertyInfo, DataEntryFlags flags, ref object? methodSharedContext);
	
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
			if (propertyInfo.IsDefined(typeof(DeReadOnlyAttribute), true))
			{
				flags |= DataEntryFlags.ReadOnly;
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
			if (propertyInfo.IsDefined(typeof(DeNoStorageAttribute), true))
			{
				flags |= DataEntryFlags.NoStorage;
			}
			if (propertyInfo.IsDefined(typeof(DeViewNameAttribute), true))
			{
				flags |= DataEntryFlags.DocumentName;
			}
			if (propertyInfo.IsDefined(typeof(DeDependsOnAttribute), true))
			{
				flags |= DataEntryFlags.Dependent;
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
			if (fieldInfo.IsDefined(typeof(DeReadOnlyAttribute), true))
			{
				flags |= DataEntryFlags.ReadOnly;
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
			if (fieldInfo.IsDefined(typeof(DeNoStorageAttribute), true))
			{
				flags |= DataEntryFlags.NoStorage;
			}
			if (fieldInfo.IsDefined(typeof(DeViewNameAttribute), true))
			{
				flags |= DataEntryFlags.DocumentName;
			}
			if (fieldInfo.IsDefined(typeof(DeDependsOnAttribute), true))
			{
				flags |= DataEntryFlags.Dependent;
			}

			if (flags == DataEntryFlags.None && !fieldInfo.IsDefined(typeof(DeDataEntryAttribute), true))
			{
				continue;
			}

			yield return (fieldInfo, flags);
		}
	}
}