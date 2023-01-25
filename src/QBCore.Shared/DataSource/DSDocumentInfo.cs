using System.ComponentModel.DataAnnotations.Schema;
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
			
			if (de.Value.Flags.HasFlag(DataEntryFlags.CreatedAtField))
			{
				DateCreatedField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.ModifiedAtField))
			{
				DateModifiedField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.UpdatedAtField))
			{
				DateUpdatedField = de.Value;
			}
			else if (de.Value.Flags.HasFlag(DataEntryFlags.DeletedAtField))
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
			if (propertyInfo.GetMethod == null)
			{
				continue;
			}

			flags = DataEntryFlags.None;

			if (propertyInfo.IsDefined(typeof(DeIdAttribute), false))
			{
				flags |= DataEntryFlags.IdField;
			}
			if (propertyInfo.IsDefined(typeof(DeReadOnlyAttribute), false))
			{
				flags |= DataEntryFlags.ReadOnly;
			}
			if (propertyInfo.IsDefined(typeof(DeCreatedAttribute), false))
			{
				flags |= DataEntryFlags.CreatedAtField;
			}
			else if (propertyInfo.IsDefined(typeof(DeModifiedAttribute), false))
			{
				flags |= DataEntryFlags.ModifiedAtField;
			}
			else if (propertyInfo.IsDefined(typeof(DeUpdatedAttribute), false))
			{
				flags |= DataEntryFlags.UpdatedAtField;
			}
			else if (propertyInfo.IsDefined(typeof(DeDeletedAttribute), false))
			{
				flags |= DataEntryFlags.DeletedAtField;
			}
			if (propertyInfo.IsDefined(typeof(DeForeignIdAttribute), false))
			{
				flags |= DataEntryFlags.ForeignId;
			}
			if (propertyInfo.IsDefined(typeof(NotMappedAttribute), false))
			{
				flags |= DataEntryFlags.NotMapped;
			}
			if (propertyInfo.IsDefined(typeof(DeNameAttribute), false))
			{
				flags |= DataEntryFlags.DocumentName;
			}
			if (propertyInfo.IsDefined(typeof(DeDependsOnAttribute), false))
			{
				flags |= DataEntryFlags.Dependent;
			}
			if (propertyInfo.IsDefined(typeof(DeHiddenAttribute), false))
			{
				flags |= DataEntryFlags.Hidden;
			}

			if (flags == DataEntryFlags.None)
			{
				if (propertyInfo.SetMethod == null && !propertyInfo.IsDefined(typeof(ColumnAttribute), false))
				{
					continue;
				}
				if (propertyInfo.IsDefined(typeof(DeIgnoreAttribute), false))
				{
					continue;
				}
			}

			yield return (propertyInfo, flags);
		}

		foreach (var fieldInfo in documentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField))
		{
			flags = DataEntryFlags.None;

			if (fieldInfo.IsDefined(typeof(DeIdAttribute), false))
			{
				flags |= DataEntryFlags.IdField;
			}
			if (fieldInfo.IsDefined(typeof(DeReadOnlyAttribute), false))
			{
				flags |= DataEntryFlags.ReadOnly;
			}
			if (fieldInfo.IsDefined(typeof(DeCreatedAttribute), false))
			{
				flags |= DataEntryFlags.CreatedAtField;
			}
			else if (fieldInfo.IsDefined(typeof(DeModifiedAttribute), false))
			{
				flags |= DataEntryFlags.ModifiedAtField;
			}
			else if (fieldInfo.IsDefined(typeof(DeUpdatedAttribute), false))
			{
				flags |= DataEntryFlags.UpdatedAtField;
			}
			else if (fieldInfo.IsDefined(typeof(DeDeletedAttribute), false))
			{
				flags |= DataEntryFlags.DeletedAtField;
			}
			if (fieldInfo.IsDefined(typeof(DeForeignIdAttribute), false))
			{
				flags |= DataEntryFlags.ForeignId;
			}
			if (fieldInfo.IsDefined(typeof(NotMappedAttribute), false))
			{
				flags |= DataEntryFlags.NotMapped;
			}
			if (fieldInfo.IsDefined(typeof(DeNameAttribute), false))
			{
				flags |= DataEntryFlags.DocumentName;
			}
			if (fieldInfo.IsDefined(typeof(DeDependsOnAttribute), false))
			{
				flags |= DataEntryFlags.Dependent;
			}
			if (fieldInfo.IsDefined(typeof(DeHiddenAttribute), false))
			{
				flags |= DataEntryFlags.Hidden;
			}

			if (flags == DataEntryFlags.None && !fieldInfo.IsDefined(typeof(ColumnAttribute), false))
			{
				continue;
			}

			yield return (fieldInfo, flags);
		}
	}
}