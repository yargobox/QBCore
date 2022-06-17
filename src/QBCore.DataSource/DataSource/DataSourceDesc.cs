using System.Reflection;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class DataSourceDesc : IDataSourceDesc
{
	public string Name { get; }
	public Type IdType { get; }
	public Type DocumentType { get; }
	public Type CreateDocumentType { get; }
	public Type SelectDocumentType { get; }
	public Type UpdateDocumentType { get; }
	public Type DeleteDocumentType { get; }
	public Type DataSourceConcreteType { get; }
	public Type DataSourceInterfaceType { get; }
	public Type DataSourceServiceType { get; }
	public DataSourceOptions Options { get; }
	public bool IsServiceSingleton => DataSourceAttribute.IsServiceSingleton;
	public Type DatabaseContextInterfaceType { get; }
	public string DataContextName { get; }
	public string? ControllerName { get; }

	public DataSourceAttribute DataSourceAttribute { get; }

	internal static readonly string[] ReservedNames = { "area", "controller", "action", "page", "filter", "cell", "id" };

	public DataSourceDesc(Type concreteType)
	{
		if (!concreteType.IsClass || concreteType.IsAbstract || concreteType.IsGenericType || concreteType.IsGenericTypeDefinition)
		{
			throw new InvalidOperationException($"Invalid complex datasource type {concreteType.ToPretty()}.");
		}

		DataSourceConcreteType = concreteType;

		DataSourceAttribute = DataSourceConcreteType.GetCustomAttribute<DataSourceAttribute>(false) ??
			throw new InvalidOperationException($"Data source {DataSourceConcreteType.ToPretty()} is not configured with attribute {nameof(DataSourceAttribute)}.");

		Name = DataSourceAttribute.Name;
		if (string.IsNullOrWhiteSpace(Name))
		{
			throw new ArgumentNullException(nameof(Name));
		}
		Name = string.Intern(Name);
		if (ReservedNames.Contains(Name, StringComparer.OrdinalIgnoreCase))
		{
			throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", ReservedNames));
		}

		DataContextName = DataSourceAttribute.DataContextName ?? "default";
		if (string.IsNullOrWhiteSpace(DataContextName))
		{
			throw new ArgumentNullException(nameof(DataContextName));
		}
		DataContextName = string.Intern(DataContextName);


		//
		// ControllerName
		//

		var controllerAttr = DataSourceConcreteType.GetCustomAttribute<DsApiControllerAttribute>(false);
		if (controllerAttr != null)
		{
			if (controllerAttr.Name.Contains("[DS:guessPlural]", StringComparison.OrdinalIgnoreCase))
			{
				ControllerName = controllerAttr.Name.Replace("[DS:guessPlural]", GuessPluralName(Name), StringComparison.OrdinalIgnoreCase);
			}
			else if (controllerAttr.Name.Contains("[DS]", StringComparison.OrdinalIgnoreCase))
			{
				ControllerName = controllerAttr.Name.Replace("[DS]", Name, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				ControllerName = controllerAttr.Name;
			}

			ControllerName = string.Intern(ControllerName);

			if (ReservedNames.Contains(ControllerName, StringComparer.OrdinalIgnoreCase))
			{
				throw new ArgumentException("These names are reserved and cannot be used as names for a data source or controller: " + string.Join(", ", ReservedNames));
			}
		};


		//
		// Get document types from a generic interface IDataSource<,,,,,>
		//

		DataSourceInterfaceType = DataSourceConcreteType.GetInterfaceOf(typeof(IDataSource<,,,,,>)) ??
			throw new InvalidOperationException($"Invalid data source type {DataSourceConcreteType.ToPretty()}.");

		var genericArgs = DataSourceInterfaceType.GetGenericArguments();
		IdType = genericArgs[0];
		DocumentType = genericArgs[1];
		CreateDocumentType = genericArgs[2];
		SelectDocumentType = genericArgs[3];
		UpdateDocumentType = genericArgs[4];
		DeleteDocumentType = genericArgs[5];


		//
		// Find DataSourceServiceType if it has not been set in the attribute, except the NotSupported type
		//

		if (DataSourceAttribute.ServiceInterface != null)
		{
			if (DataSourceAttribute.ServiceInterface == typeof(NotSupported))
			{
				DataSourceServiceType = DataSourceConcreteType;
			}
			else if (DataSourceAttribute.ServiceInterface.GetInterfaces().Contains(DataSourceInterfaceType))
			{
				DataSourceServiceType = DataSourceAttribute.ServiceInterface;
			}
			else
			{
				throw new InvalidOperationException($"Invalid data source servive interface {DataSourceAttribute.ServiceInterface.ToPretty()}.");
			}
		}
		else
		{
			DataSourceServiceType = TryFindDataSourceServiceInterfaceType() ?? DataSourceConcreteType;
		}


		//
		// Set other properties and options
		//

		Options = DataSourceAttribute.Options;
		if (CreateDocumentType != typeof(NotSupported)) Options |= DataSourceOptions.CanInsert;
		if (SelectDocumentType != typeof(NotSupported)) Options |= DataSourceOptions.CanSelect;
		if (UpdateDocumentType != typeof(NotSupported)) Options |= DataSourceOptions.CanUpdate;
		if (DeleteDocumentType != typeof(NotSupported)) Options |= DataSourceOptions.CanDelete;

		if (Options.HasFlag(DataSourceOptions.CanInsert))
			DatabaseContextInterfaceType = StaticFactory.QueryBuilders.GetInsert(DocumentType, CreateDocumentType)().DatabaseContextInterface;
		else if (Options.HasFlag(DataSourceOptions.CanSelect))
			DatabaseContextInterfaceType = StaticFactory.QueryBuilders.GetSelect(DocumentType, SelectDocumentType)().DatabaseContextInterface;
		else if (Options.HasFlag(DataSourceOptions.CanUpdate))
			DatabaseContextInterfaceType = StaticFactory.QueryBuilders.GetUpdate(DocumentType, UpdateDocumentType)().DatabaseContextInterface;
		else if (Options.HasFlag(DataSourceOptions.CanDelete))
			DatabaseContextInterfaceType = StaticFactory.QueryBuilders.GetDelete(DocumentType, DeleteDocumentType)().DatabaseContextInterface;
		else
			throw new InvalidOperationException($"Data source {DataSourceConcreteType.ToPretty()} must have at least one defined operation.");


		//
		// Validate some options
		//

		if ((Options.HasFlag(DataSourceOptions.CanTestInsert) && !Options.HasFlag(DataSourceOptions.CanInsert))
			|| (Options.HasFlag(DataSourceOptions.CanTestUpdate) && !Options.HasFlag(DataSourceOptions.CanUpdate))
			|| (Options.HasFlag(DataSourceOptions.CanTestDelete) && !Options.HasFlag(DataSourceOptions.CanDelete))
			|| (Options.HasFlag(DataSourceOptions.CanTestRestore) && !Options.HasFlag(DataSourceOptions.CanRestore)))
		{
			throw new InvalidOperationException($"Data source {DataSourceConcreteType.ToPretty()} cannot have just a test QB.");
		}

		if ((Options.HasFlag(DataSourceOptions.RefreshAfterInsert) && !Options.HasFlag(DataSourceOptions.CanInsert))
			|| (Options.HasFlag(DataSourceOptions.RefreshAfterUpdate) && !Options.HasFlag(DataSourceOptions.CanUpdate))
			|| (Options.HasFlag(DataSourceOptions.RefreshAfterDelete) && !Options.HasFlag(DataSourceOptions.CanDelete))
			|| (Options.HasFlag(DataSourceOptions.RefreshAfterRestore) && !Options.HasFlag(DataSourceOptions.CanRestore)))
		{
			throw new InvalidOperationException($"Data source {DataSourceConcreteType.ToPretty()} cannot be refreshed after the operation if the operation itself is not supported.");
		}

		if ((Options.HasFlag(DataSourceOptions.CompositeId | DataSourceOptions.CompoundId))
			|| (Options.HasFlag(DataSourceOptions.SingleRecord | DataSourceOptions.FewRecords)))
		{
			throw new InvalidOperationException($"Data source {DataSourceConcreteType.ToPretty()} is configured inproperly.");
		}


		//
		// Validate native data source listener if any
		// This validation does not violate the single responsibility principle, as DataSourceDesk is responsible not only for the description
		// but also for checking the entire model from the data source and down to cause an exception at loading time, not at runtime.
		// Half of this validation is already here, so it makes no sense to place the other part somewhere else.
		//

		ValidateNativeListener();
	}

	private Type? TryFindDataSourceServiceInterfaceType()
	{
		return DataSourceConcreteType
			.GetInterfaces()
			.Where(x => x.GetInterfaces().Contains(DataSourceInterfaceType))
			.FirstOrDefault();
	}

	private void ValidateNativeListener()
	{
		if (DataSourceAttribute.Listener != null)
		{
			Type listenerType = DataSourceAttribute.Listener;

			if (!listenerType.IsClass || listenerType.IsAbstract || !typeof(IDataSourceListener).IsAssignableFrom(listenerType) || Nullable.GetUnderlyingType(listenerType) != null)
			{
				throw new InvalidOperationException($"Incompatible data source listener type '{listenerType.ToPretty()}'.");
			}

			var type = listenerType.GetSubclassOf(typeof(DataSourceListener<,,,,,>));
			if (type == null)
			{
				throw new InvalidOperationException($"Incompatible data source listener type {listenerType.ToPretty()}.");
			}

			var genericArgs = type.GetGenericArguments();
			if (IdType != genericArgs[0]
				|| DocumentType != genericArgs[1]
				|| CreateDocumentType != genericArgs[2]
				|| SelectDocumentType != genericArgs[3]
				|| UpdateDocumentType != genericArgs[4]
				|| DeleteDocumentType != genericArgs[5])
			{
				throw new InvalidOperationException($"Invalid data source listener type {listenerType.ToPretty()}.");
			}

			if (listenerType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length != 1)
			{
				throw new InvalidOperationException($"Data source listener '{listenerType.ToPretty()} must have a single public constructor'.");
			}
		}
	}

	private static readonly string[] _pluralEndingsType1 = { "s", "ss", "sh", "ch", "x", "z" };
	private static readonly char[] _pluralEndingsType2 = { 'a', 'e', 'i', 'o', 'u' };
	private static string GuessPluralName(string name)
	{
		if (name.Length <= 2)
		{
			return name;
		}
		if (_pluralEndingsType1.Any(x => name.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
		{
			return name + "es";
		}
		if (name.EndsWith('f'))
		{
			return name.Substring(0, name.Length - 1) + "ves";
		}
		if (name.EndsWith("fe"))
		{
			return name.Substring(0, name.Length - 2) + "ves";
		}
		if (name.EndsWith('y'))
		{
			var lower = new String(name[name.Length - 2], 1).ToLower()[0];

			return _pluralEndingsType2.Any(x => x == lower)
				? name.Substring(0, name.Length - 1) + 's'
				: name.Substring(0, name.Length - 1) + "ies";
		}
		if (name.EndsWith("is"))
		{
			return name.Substring(0, name.Length - 2) + "es";
		}

		return char.IsDigit(name[name.Length - 2]) ? name : name + 's';
	}
}