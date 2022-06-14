using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

/// <summary>
/// Renames controllers to DataSourceDesc.Name from their generic type names (DataSourceController`7).
/// Applies RouteAttribute for each of them according to RouteTemplate.
/// Builds index of controller names.
/// </summary>
public class DataSourceControllerRouteConvention : IControllerModelConvention
{
	public string RoutePrefix
	{
		get => AddQBCoreExtensions.RoutePrefix;
		init => AddQBCoreExtensions.RoutePrefix = value;
	}

	public virtual void Apply(ControllerModel controller)
	{
		if (!AddQBCoreExtensions.IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(AddQBCoreExtensions.AddQBCore) + " must be called first.");
		}
		AddQBCoreExtensions.IsDataSourceControllerRouteConventionCreated = true;

		var controllerSubclass = controller.ControllerType.GetSubclassOf(typeof(DataSourceController<,,,,,,>));

		if (controllerSubclass != null)
		{
			// Get generic argument 'TDataSource' as the last one
			var dataSourceServiceType = controller.ControllerType.GetGenericArguments().Last();

			// A service type may be not a concrete type. If it is not, we have to find it :(
			var desc = StaticFactory.DataSources.GetValueOrDefault(dataSourceServiceType) ??
				StaticFactory.DataSources.Values.First(x => x.DataSourceServiceType == dataSourceServiceType);

			if (controllerSubclass == controller.ControllerType)
			{
				ApplyToAutoDataSourceController(controller, desc);
			}
			else
			{
				ApplyToCustomDataSourceController(controller, desc);
			}
		}
		else
		{
			ApplyToController(controller);
		}
	}

	public virtual void ApplyToAutoDataSourceController(ControllerModel controller, IDataSourceDesc desc)
	{
		if (string.IsNullOrEmpty(desc.ControllerName))
		{
			throw new InvalidOperationException($"No controller name was specified for data source '{desc.Name}'.");
		}

		controller.ControllerName = desc.ControllerName;

		var attributeRouteModel = new AttributeRouteModel(new RouteAttribute(RoutePrefix + "[controller]"));
		var nullRouteSelector = controller.Selectors.FirstOrDefault(x => x.AttributeRouteModel == null);
		if (nullRouteSelector != null)
		{
			nullRouteSelector.AttributeRouteModel = attributeRouteModel;
		}
		else
		{
			controller.Selectors.Add(new SelectorModel { AttributeRouteModel = attributeRouteModel });
		}
	}

	protected virtual void ApplyToCustomDataSourceController(ControllerModel controller, IDataSourceDesc desc)
	{
		if (!string.IsNullOrEmpty(desc?.ControllerName))
		{
			if (desc.DataSourceConcreteType.GetCustomAttribute<DsApiControllerAttribute>(false)?.AutoBuild == false)
			{
				controller.ControllerName = desc.ControllerName;
			}
		}
	}

	protected virtual void ApplyToController(ControllerModel controller)
	{
	}
}