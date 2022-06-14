using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

public class DataSourceRouteValueTransformer : DynamicRouteValueTransformer
{
	protected HashSet<string> DataSourceControllerNames;

	public DataSourceRouteValueTransformer()
	{
		if (!AddQBCoreExtensions.IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(AddQBCoreExtensions.AddQBCore) + " must be called first.");
		}
		if (!AddQBCoreExtensions.IsDataSourceControllerRouteConventionCreated)
		{
			throw new InvalidOperationException(nameof(DataSourceControllerRouteConvention) + " must be applied first.");
		}

		var dataSourceControllerNames = StaticFactory.DataSources.Values
			.Where(x => !string.IsNullOrEmpty(x.ControllerName))
			.Select(x => x.ControllerName!);

		DataSourceControllerNames = new HashSet<string>(dataSourceControllerNames, StringComparer.OrdinalIgnoreCase);
	}

	public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
	{
		// check controller
		var controllerName = values.GetValueOrDefault("controller") as string;
		if (string.IsNullOrEmpty(controllerName))
		{
			return values;
		}
		if (!DataSourceControllerNames.Contains(controllerName))
		{
			return values;
		}

		// check c0 (root) controller
		var rootControllerName = values.GetValueOrDefault("c0") as string;
		if (rootControllerName != null && !DataSourceControllerNames.Contains(rootControllerName))
		{
			return values;
		}

		var method = httpContext.Request.Method.ToUpper();

		if (method != "GET" && (values.ContainsKey("filter") || values.ContainsKey("cell")))
		{// filter and cell requests are readonly
			values["action"] = "bad_request";
			return values;
		}

		switch (httpContext.Request.Method.ToUpper())
		{
			case "GET": values["action"] = string.IsNullOrEmpty(values.GetValueOrDefault("id") as string) ? "index" : "get"; break;
			case "POST": values["action"] = "create"; break;
			case "PUT": values["action"] = "update"; break;
			case "DELETE": values["action"] = "delete"; break;
			case "PATCH": values["action"] = "restore"; break;
			default: values["action"] = "bad_request"; break;
		};

		return await Task.FromResult(values);
	}
}