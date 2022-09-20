using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

/// <summary>
/// Renames controllers to DSDefinition.Name from their generic type names (DataSourceController`7).
/// Applies RouteAttribute for each of them according to RouteTemplate.
/// Builds index of controller names.
/// </summary>
public class DataSourceControllerRouteConvention : IControllerModelConvention
{
	public string RoutePrefix
	{
		get => ExtensionsForAddQBCore.RoutePrefix;
		init => ExtensionsForAddQBCore.RoutePrefix = value;
	}

	public virtual void Apply(ControllerModel controller)
	{
		if (!ExtensionsForAddQBCore.IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(ExtensionsForAddQBCore.AddQBCore) + " must be called first.");
		}
		ExtensionsForAddQBCore.IsDataSourceControllerRouteConventionCreated = true;

		var controllerSubclass = controller.ControllerType.GetSubclassOf(typeof(DataSourceController<,,,,,,,>));

		if (controllerSubclass != null)
		{
			// Get generic argument 'TDataSource' as the last one
			var dataSourceServiceType = controller.ControllerType.GetGenericArguments().Last();

			// A service type may be not a concrete type. If it is not, we have to find it :(
			var definition = StaticFactory.DataSources.GetValueOrDefault(dataSourceServiceType) ??
				StaticFactory.DataSources.Values.First(x => x.DataSourceServiceType == dataSourceServiceType);

			if (controllerSubclass == controller.ControllerType)
			{
				ApplyToAutoDataSourceController(controller, definition);
			}
			else
			{
				ApplyToCustomDataSourceController(controller, definition);
			}
		}
		else
		{
			ApplyToController(controller);
		}
	}

	public virtual void ApplyToAutoDataSourceController(ControllerModel controller, IDSInfo definition)
	{
		if (string.IsNullOrEmpty(definition.ControllerName))
		{
			throw new InvalidOperationException($"No controller name was specified for datasource '{definition.Name}'.");
		}

		controller.ControllerName = definition.ControllerName;

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

	protected virtual void ApplyToCustomDataSourceController(ControllerModel controller, IDSInfo info)
	{
		if (!string.IsNullOrEmpty(info?.ControllerName))
		{
			if (info.ConcreteType.GetCustomAttribute<DsApiControllerAttribute>(false)?.BuildAutoController == false)
			{
				controller.ControllerName = info.ControllerName;
			}
		}
	}

	protected virtual void ApplyToController(ControllerModel controller)
	{
	}
}