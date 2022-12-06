using System.Reflection;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.ComponentModel;
using QBCore.Extensions.Text;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal sealed class DSInfo : IDSInfo
{
	public string Name { get; }
	public string Tech => "DS";
	public Type ConcreteType => DSTypeInfo.Concrete;
	public string? ControllerName { get; }

	public DSTypeInfo DSTypeInfo { get; }

	public Lazy<DSDocumentInfo> DocumentInfo { get; }
	public Lazy<DSDocumentInfo>? CreateInfo { get; }
	public Lazy<DSDocumentInfo>? SelectInfo { get; }
	public Lazy<DSDocumentInfo>? UpdateInfo { get; }
	public Lazy<DSDocumentInfo>? DeleteInfo { get; }
	public Lazy<DSDocumentInfo>? RestoreInfo { get; }

	public Type DataSourceServiceType { get; }

	public DataSourceOptions Options { get; }

	public string DataContextName { get; }

	public IQueryBuilderFactory QBFactory { get; }
	public Func<IServiceProvider, IDataSourceListener>[]? Listeners { get; }

	public bool IsServiceSingleton { get; }

	public bool BuildAutoController { get; }

	internal static readonly string[] ReservedNames = { "area", "controller", "action", "page", "filter", "cell", "id" };
	internal const DataSourceOptions AllDSOperations = DataSourceOptions.CanInsert | DataSourceOptions.CanSelect | DataSourceOptions.CanUpdate | DataSourceOptions.CanDelete | DataSourceOptions.CanRestore;

	public DSInfo(Type dataSourceConcrete)
	{
		if (!dataSourceConcrete.IsClass || dataSourceConcrete.IsAbstract || dataSourceConcrete.IsGenericType || dataSourceConcrete.IsGenericTypeDefinition
				|| dataSourceConcrete.GetSubclassOf(typeof(DataSource<,,,,,,,>)) == null || Nullable.GetUnderlyingType(dataSourceConcrete) != null)
		{
			throw new ArgumentException($"Invalid datasource type {dataSourceConcrete.ToPretty()}.", nameof(dataSourceConcrete));
		}

		// Get document types from a generic interface IDataSource<,,,,,,,>
		//
		DSTypeInfo = new DSTypeInfo(dataSourceConcrete);

		// Our building
		//
		var building = new DSBuilder(DSTypeInfo.Concrete);

		// Load fields from [DataSource]
		//
		var dataSourceAttr = DSTypeInfo.Concrete.GetCustomAttribute<DataSourceAttribute>(false);
		if (dataSourceAttr != null)
		{
			building.Name = dataSourceAttr.Name;
			building.Options = dataSourceAttr.Options ?? DataSourceOptions.None;
			building.DataContextName = dataSourceAttr.DataContextName;
			building.IsServiceSingleton = dataSourceAttr.IsServiceSingleton;
			building.ServiceInterface = dataSourceAttr.ServiceInterface;
			building.DataLayer = dataSourceAttr.DataLayer;
		}

		// Load fields from [DsListeners]
		//
		var listenersAttr = DSTypeInfo.Concrete.GetCustomAttribute<DsListenersAttribute>(false);
		if (listenersAttr != null)
		{
			foreach (var listenerType in listenersAttr.Types)
			{
				building.Listeners.Add(listenerType);
			}
		}

		// Load fields from [DsApiController]
		//
		var controllerAttr = DSTypeInfo.Concrete.GetCustomAttribute<DsApiControllerAttribute>(false);
		if (controllerAttr != null)
		{
			building.ControllerName = controllerAttr.Name;
			building.BuildAutoController = controllerAttr.BuildAutoController;
		}

		// Find a builder and build if any
		//
		var builder = FactoryHelper.FindBuilder<IDSBuilder>(dataSourceAttr?.Builder ?? DSTypeInfo.Concrete, dataSourceAttr?.BuilderMethod);
		if (builder != null)
		{
			builder(building);
		}

		// Name
		//
		if (building.Name != null)
		{
			var name = building.Name.Trim();
			
			if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('*'))
			{
				throw new ArgumentException($"{nameof(IDSBuilder)}.{nameof(IDSBuilder.Name)}");
			}

			if (name.Contains("[DS]", StringComparison.OrdinalIgnoreCase))
			{
				Name = name.Replace("[DS]", MakeDSNameFromType(DSTypeInfo.Concrete), StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				Name = name;
			}
		}
		else
		{
			Name = MakeDSNameFromType(DSTypeInfo.Concrete);
		}

		Name = string.Intern(Name);

		if (ReservedNames.Contains(Name, StringComparer.OrdinalIgnoreCase))
		{
			throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", ReservedNames));
		}

		// Determine or validate supported datasource operations
		//
		Options = building.Options;
		if ((Options & AllDSOperations) == DataSourceOptions.None)
		{
			if (DSTypeInfo.TCreate != typeof(NotSupported)) Options |= DataSourceOptions.CanInsert;
			if (DSTypeInfo.TSelect != typeof(NotSupported)) Options |= DataSourceOptions.CanSelect;
			if (DSTypeInfo.TUpdate != typeof(NotSupported)) Options |= DataSourceOptions.CanUpdate;
			if (DSTypeInfo.TDelete != typeof(NotSupported)) Options |= DataSourceOptions.CanDelete;
			if (DSTypeInfo.TRestore != typeof(NotSupported)) Options |= DataSourceOptions.CanRestore;
		}
		else if (
			(Options.HasFlag(DataSourceOptions.CanInsert) && DSTypeInfo.TCreate == typeof(NotSupported)) ||
			(Options.HasFlag(DataSourceOptions.CanSelect) && DSTypeInfo.TSelect == typeof(NotSupported)) ||
			(Options.HasFlag(DataSourceOptions.CanUpdate) && DSTypeInfo.TUpdate == typeof(NotSupported)) ||
			(Options.HasFlag(DataSourceOptions.CanDelete) && DSTypeInfo.TDelete == typeof(NotSupported)) ||
			(Options.HasFlag(DataSourceOptions.CanRestore) && DSTypeInfo.TRestore == typeof(NotSupported)))
		{
			throw new InvalidOperationException($"DataSource {DSTypeInfo.Concrete.ToPretty()} operation cannot be set on type '{nameof(NotSupported)}'.");
		}

		// Validate options
		//
		if ((Options & AllDSOperations) == DataSourceOptions.None)
		{
			throw new InvalidOperationException($"DataSource {DSTypeInfo.Concrete.ToPretty()} must have at least one supported operation.");
		}

		if ((Options.HasFlag(DataSourceOptions.RefreshAfterInsert) && !Options.HasFlag(DataSourceOptions.CanInsert))
			|| (Options.HasFlag(DataSourceOptions.RefreshAfterUpdate) && !Options.HasFlag(DataSourceOptions.CanUpdate))
			|| (Options.HasFlag(DataSourceOptions.RefreshAfterDelete) && !Options.HasFlag(DataSourceOptions.CanDelete))
			|| (Options.HasFlag(DataSourceOptions.RefreshAfterRestore) && !Options.HasFlag(DataSourceOptions.CanRestore)))
		{
			throw new InvalidOperationException($"DataSource {DSTypeInfo.Concrete.ToPretty()} cannot be refreshed after the operation if the operation itself is not supported.");
		}

		if (Options.HasFlag(DataSourceOptions.CompositeId | DataSourceOptions.CompoundId)
			|| Options.HasFlag(DataSourceOptions.SingleRecord | DataSourceOptions.FewRecords))
		{
			throw new InvalidOperationException($"DataSource {DSTypeInfo.Concrete.ToPretty()} is configured inproperly.");
		}

		// DataContextName
		//
		DataContextName = building.DataContextName ?? "default";
		if (string.IsNullOrWhiteSpace(DataContextName))
		{
			throw new ArgumentNullException(nameof(DataContextName));
		}
		DataContextName = string.Intern(DataContextName);

		// ControllerName
		//
		if (building.ControllerName != null || building.BuildAutoController != null)
		{
			var controllerName = building.ControllerName?.Trim() ?? "[DS]";
			
			if (string.IsNullOrWhiteSpace(controllerName) || controllerName.Contains('/') || controllerName.Contains('*'))
			{
				throw new ArgumentException(nameof(ControllerName));
			}

			if (controllerName.Contains("[DS:guessPlural]", StringComparison.OrdinalIgnoreCase))
			{
				ControllerName = controllerName.Replace("[DS:guessPlural]", GuessPluralName(Name), StringComparison.OrdinalIgnoreCase);
			}
			else if (controllerName.Contains("[DS]", StringComparison.OrdinalIgnoreCase))
			{
				ControllerName = controllerName.Replace("[DS]", Name, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				ControllerName = controllerName;
			}

			ControllerName = string.Intern(ControllerName);

			if (ReservedNames.Contains(ControllerName, StringComparer.OrdinalIgnoreCase))
			{
				throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", ReservedNames));
			}

			BuildAutoController = building.BuildAutoController ?? true;
		}

		// DataSourceService
		//
		if (building.ServiceInterface != null)
		{
			if (building.ServiceInterface == typeof(NotSupported))
			{
				DataSourceServiceType = DSTypeInfo.Concrete;
			}
			else if (building.ServiceInterface.GetInterfaces().Contains(DSTypeInfo.Interface))
			{
				DataSourceServiceType = building.ServiceInterface;
			}
			else
			{
				throw new InvalidOperationException($"Invalid datasource servive interface {building.ServiceInterface.ToPretty()}.");
			}
		}
		else
		{
			DataSourceServiceType = TryFindDataSourceServiceInterfaceType() ?? DSTypeInfo.Concrete;
		}

		// Get DataSource's data layer info
		//
		if (building.DataLayer == null)
		{
			throw new InvalidOperationException($"DataSource {DSTypeInfo.Concrete.ToPretty()} must have a specified data layer.");
		}
		if (building.DataLayer.GetInterfaceOf(typeof(IDataLayerInfo)) == null)
		{
			throw new InvalidOperationException($"Invalid data layer specified '{building.DataLayer.ToPretty()}'.");
		}
		var dataLayer =
			(IDataLayerInfo?)building.DataLayer.GetField("Default", BindingFlags.Static | BindingFlags.Public)?.GetValue(null)
			?? (IDataLayerInfo?)building.DataLayer.GetProperty("Default", BindingFlags.Static | BindingFlags.Public)?.GetValue(null)
			?? throw new InvalidOperationException($"Invalid data layer specified '{building.DataLayer.ToPretty()}'.");

		// Register DataSource's document types, including nested ones.
		//
		DocumentInfo = StaticFactory.Internals.GetOrRegisterDocument(DSTypeInfo.TDocument, dataLayer);
		CreateInfo = DSTypeInfo.TCreate != typeof(NotSupported) ? StaticFactory.Internals.GetOrRegisterDocument(DSTypeInfo.TCreate, dataLayer) : null;
		SelectInfo = DSTypeInfo.TSelect != typeof(NotSupported) ? StaticFactory.Internals.GetOrRegisterDocument(DSTypeInfo.TSelect, dataLayer) : null;
		UpdateInfo = DSTypeInfo.TUpdate != typeof(NotSupported) ? StaticFactory.Internals.GetOrRegisterDocument(DSTypeInfo.TUpdate, dataLayer) : null;
		DeleteInfo = DSTypeInfo.TDelete != typeof(NotSupported) ? StaticFactory.Internals.GetOrRegisterDocument(DSTypeInfo.TDelete, dataLayer) : null;
		RestoreInfo = DSTypeInfo.TRestore != typeof(NotSupported) ? StaticFactory.Internals.GetOrRegisterDocument(DSTypeInfo.TRestore, dataLayer) : null;

		// Create a query builder factory
		//
		QBFactory = dataLayer.CreateQBFactory(
			DSTypeInfo,
			Options,
			building.InsertBuilder,
			building.SelectBuilder,
			building.UpdateBuilder,
			building.DeleteBuilder,
			building.SoftDelBuilder,
			building.RestoreBuilder,
#if DEBUG
			false
#else
			true
#endif
		);

		// Listener
		//
		if (building.Listeners.Count > 0)
		{
			Listeners = new Func<IServiceProvider, IDataSourceListener>[building.Listeners.Count];
			for (int i = 0; i < building.Listeners.Count; i++)
			{
				Listeners[i] = MakeListenerFactoryMethod(building.Listeners[i]);
			}
		}
	}

	private static string MakeDSNameFromType(Type dataSourceConcrete)
	{
		var fromClassName = dataSourceConcrete.Name.ReplaceEnding("Service").ReplaceEnding("DS").ReplaceEnding("Ds");
		if (string.IsNullOrEmpty(fromClassName))
		{
			fromClassName = dataSourceConcrete.Name;
		}
		return fromClassName;
	}

	private Type? TryFindDataSourceServiceInterfaceType()
	{
		return DSTypeInfo.Concrete
			.GetInterfaces()
			.Where(x => x.GetInterfaces().Contains(DSTypeInfo.Interface))
			.FirstOrDefault();
	}

	private Func<IServiceProvider, IDataSourceListener> MakeListenerFactoryMethod(Type listenerType)
	{
		if (!listenerType.IsClass || listenerType.IsAbstract ||
			!typeof(IDataSourceListener).IsAssignableFrom(listenerType) || Nullable.GetUnderlyingType(listenerType) != null)
		{
			throw new InvalidOperationException($"Invalid datasource listener type {listenerType.ToPretty()}.");
		}

		var type = listenerType.GetSubclassOf(typeof(DataSourceListener<,,,,,,>));
		if (type == null)
		{
			throw new InvalidOperationException($"Invalid datasource listener type {listenerType.ToPretty()}.");
		}

		var genericArgs = type.GetGenericArguments();
		if (listenerType.IsGenericTypeDefinition)
		{
			if (genericArgs.Length != 7)
			{
				throw new InvalidOperationException($"Incompatible datasource listener type {listenerType.ToPretty()}.");
			}

			listenerType = listenerType.MakeGenericType(
				DSTypeInfo.TKey,
				DSTypeInfo.TDocument,
				DSTypeInfo.TCreate,
				DSTypeInfo.TSelect,
				DSTypeInfo.TUpdate,
				DSTypeInfo.TDelete,
				DSTypeInfo.TRestore
			);
		}
		else
		{
			if (DSTypeInfo.TKey != genericArgs[0]
				|| DSTypeInfo.TDocument != genericArgs[1]
				|| DSTypeInfo.TCreate != genericArgs[2]
				|| DSTypeInfo.TSelect != genericArgs[3]
				|| DSTypeInfo.TUpdate != genericArgs[4]
				|| DSTypeInfo.TDelete != genericArgs[5]
				|| DSTypeInfo.TRestore != genericArgs[6])
			{
				throw new InvalidOperationException($"Incompatible datasource listener type {listenerType.ToPretty()}.");
			}
		}

		var ctors = listenerType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		if (ctors.Length != 1)
		{
			throw new InvalidOperationException($"DataSource listener {listenerType.ToPretty()} must have a single public constructor.");
		}

		var ctor = ctors[0];
		var parameters = ctor
			.GetParameters()
			.Select(x => (
				x.ParameterType,
				IsNullable: Nullable.GetUnderlyingType(x.ParameterType) != null
			))
			.ToArray();

		return IDataSourceListener (IServiceProvider provider) =>
		{
			var args = parameters
				.Select(x => x.IsNullable ? provider.GetService(x.ParameterType) : provider.GetRequiredInstance(x.ParameterType))
				.ToArray();

			return (IDataSourceListener)ctor.Invoke(args);
		};
	}

	private static readonly string[] _pluralEndingsType1 = { "s", "ss", "sh", "ch", "x", "z" };
	private static readonly char[] _pluralEndingsType2 = { 'a', 'e', 'i', 'o', 'u' };
	internal static string GuessPluralName(string name)
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