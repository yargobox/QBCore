using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QBCore.Configuration;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

public static class AddQBCoreExtensions
{
	internal static volatile bool IsAddQBCoreCalled;
	internal static volatile bool IsDataSourceControllerRouteConventionCreated;
	internal static string RoutePrefix
	{
		get => _routePrefix;
		set
		{
			value = (value ?? string.Empty).Trim();
			if (value.Length == 0 || value.EndsWith("/"))
			{
				_routePrefix = value;
			}
			else
			{
				_routePrefix = value + "/";
			}
		}
	}
	private static string _routePrefix = string.Empty;

	public static IMvcBuilder AddQBCore(this IMvcBuilder builder)
	{
		AddQBCoreRegisters(builder.Services, builder.PartManager);
		return builder;
	}
	public static IMvcCoreBuilder AddQBCore(this IMvcCoreBuilder builder)
	{
		AddQBCoreRegisters(builder.Services, builder.PartManager);
		return builder;
	}
	public static IServiceCollection AddQBCore(this IServiceCollection services, params Assembly[] assemblies)
	{
		AddQBCoreRegisters(services, assemblies);
		return services;
	}

	private static IServiceCollection AddQBCoreRegisters(this IServiceCollection services, params Assembly[] assemblies)
	{
		var types = assemblies
			.SelectMany(x => x.DefinedTypes)
			.Select(x => x.AsType())
			.Distinct()
			.ToArray();

		return services.AddQBCoreRegisters(types);
	}
	private static IServiceCollection AddQBCoreRegisters(this IServiceCollection services, ApplicationPartManager partManager)
	{
		var types = partManager
			.ApplicationParts
			.Where(x => x is AssemblyPart)
			.Cast<AssemblyPart>()
			.SelectMany(x => x.Types)
			.ToArray();
		
		return services.AddQBCoreRegisters(types);
	}
	private static IServiceCollection AddQBCoreRegisters(this IServiceCollection services, IEnumerable<Type> types)
	{
		if (IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(AddQBCore) + " is already called.");
		}
		IsAddQBCoreCalled = true;

		RegisterDataSources.FromTypes(types);
		AddBusinessObjects();
		AddDataSourcesAsServices(services);
		services.TryAddSingleton<DataSourceRouteValueTransformer>();
		return services;
	}

	private static void AddBusinessObjects()
	{
		var registry = (IFactoryObjectRegistry<string, BusinessObject>)StaticFactory.BusinessObjects;
		BusinessObject bo;
		
		foreach (var desc in StaticFactory.DataSources)
		{
			bo = new BusinessObject("DS", string.Intern(desc.Value.Name), desc.Value.DataSourceConcreteType);
			
			registry.TryRegisterObject(bo.Key, bo);
		}
	}
	
	private static void AddDataSourcesAsServices(IServiceCollection services)
	{
		Type transientInterface, transientType = typeof(ITransient<>);
		Func<IServiceProvider, object> implementationFactory;

		foreach (var desc in StaticFactory.DataSources)
		{
			var dataSourceType = desc.Value.DataSourceConcreteType;
			implementationFactory = sp => ActivatorUtilities.CreateInstance(sp, dataSourceType, sp, sp.GetRequiredService<IDataContextProvider>());

			if (desc.Value.DataSourceServiceType == dataSourceType)
			{
				if (desc.Value.IsServiceSingleton)
				{
					services.TryAddSingleton(dataSourceType, implementationFactory);
				}
				else
				{
					services.TryAddScoped(dataSourceType, implementationFactory);
				}

				transientInterface = transientType.MakeGenericType(dataSourceType);
				if (transientInterface.IsAssignableFrom(dataSourceType))
				{
					services.TryAddTransient(transientInterface, implementationFactory);
				}
			}
			else
			{
				if (desc.Value.IsServiceSingleton)
				{
					services.TryAddSingleton(desc.Value.DataSourceServiceType, implementationFactory);
				}
				else
				{
					services.TryAddScoped(desc.Value.DataSourceServiceType, implementationFactory);
				}

				transientInterface = transientType.MakeGenericType(desc.Value.DataSourceServiceType);
				if (transientInterface.IsAssignableFrom(dataSourceType))
				{
					services.TryAddTransient(transientInterface, implementationFactory);
				}
			}
		}
	}

	public static IEndpointRouteBuilder MapDataSourceControllers(this IEndpointRouteBuilder endpoints)
	{
		int orderStart = 1;
		return MapDataSourceControllers(endpoints, ref orderStart);
	}
	public static IEndpointRouteBuilder MapDataSourceControllers(this IEndpointRouteBuilder endpoints, ref int orderStart)
	{
/* 		var pipeline = endpoints.CreateApplicationBuilder().UseMiddleware<RandomNumberMiddleware>().Build();
		return endpoints.Map(pattern, pipeline).WithDisplayName("Random Number");
		//endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>("{ds}/{target}/{ds_id}/{controller}/{id?}");
		//endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>(RouteTemplate + "/{bo}/{**slug}", null!, 1);
*/
/*
(default)	{controller}/{id?}
			{c0}/{field}/filter/{controller}/{id?}
			{c0}/{field}/cell/{controller}/{id?}

			{c0}/{i0}/{controller}/{id?}
			{c0}/{i0}/{c1}/{field}/filter/{controller}/{id?}
			{c0}/{i0}/{c1}/{field}/cell/{controller}/{id?}

			{c0}/{i0}/{c1}/{i1}/{controller}/{id?}
			{c0}/{i0}/{c1}/{i1}/{c2}/{field}/filter/{controller}/{id?}
			{c0}/{i0}/{c1}/{i1}/{c2}/{field}/cell/{controller}/{id?}

			{c0}/{i0}/{c1}/{i1}/{c2}/{i2}/{controller}/{id?}
			{c0}/{i0}/{c1}/{i1}/{c2}/{i2}/{c3}/{field}/filter/{controller}/{id?}
			{c0}/{i0}/{c1}/{i1}/{c2}/{i2}/{c3}/{field}/filter/{controller}/{id?}
*/

		int nestedNodeLevel = 6;

		endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>(RoutePrefix + "{c0}/{field}/filter/{controller}/{id?}", null!, orderStart++);
		endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>(RoutePrefix + "{c0}/{field}/cell/{controller}/{id?}", null!, orderStart++);

		var br = new StringBuilder();
		for (int i = 0; i < nestedNodeLevel; i++)
		{
			br.Append("{c").Append(i).Append("}/{i").Append(i).Append("}/");

			endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>(
				string.Concat(RoutePrefix, br.ToString(), "{controller}/{id?}"),
				null!,
				orderStart++);

			endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>(
				string.Concat(RoutePrefix, br.ToString(), "{c", (i + 1).ToString(), "}/{field}/filter/{controller}/{id?}"),
				null!,
				orderStart++);
			
			endpoints.MapDynamicControllerRoute<DataSourceRouteValueTransformer>(
				string.Concat(RoutePrefix, br.ToString(), "{c", (i + 1).ToString(), "}/{field}/cell/{controller}/{id?}"),
				null!,
				orderStart++);
		}

		return endpoints;
	}
}