using System.Collections.Generic;
using System.Diagnostics;
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

public record AddQBCoreOptions
{
	public Func<AssemblyPart, bool> AssemblyPartSelector { get; set; } = _ => true;
	public Func<Assembly, bool> AssemblySelector { get; set; } = _ => true;
	public Func<Type, bool> TypeSelector { get; set; } = _ => true;
	public Func<Type, bool> DocumentExclusionSelector { get; set; } = _ => false;
	public Func<Type, bool> DataSourceSelector { get; set; } = type
		=> type.IsClass && !type.IsAbstract && !type.IsGenericType && !type.IsGenericTypeDefinition && type.GetSubclassOf(typeof(DataSource<,,,,,,,>)) != null;
	public Func<Type, bool> ComplexDataSourceSelector { get; set; } = type
		=> type.IsClass && !type.IsAbstract && !type.IsGenericType && !type.IsGenericTypeDefinition && type.GetSubclassOf(typeof(ComplexDataSource<>)) != null;
}

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

	public static IMvcBuilder AddQBCore(this IMvcBuilder builder, Action<AddQBCoreOptions>? setupOptions = null)
	{
		var options = new AddQBCoreOptions();
		if (setupOptions != null)
		{
			setupOptions(options);
		}

		AddQBCoreRegisters(builder.Services, options, builder.PartManager);
		return builder;
	}
	public static IMvcCoreBuilder AddQBCore(this IMvcCoreBuilder builder, Action<AddQBCoreOptions>? setupOptions = null)
	{
		var options = new AddQBCoreOptions();
		if (setupOptions != null)
		{
			setupOptions(options);
		}

		AddQBCoreRegisters(builder.Services, options, builder.PartManager);
		return builder;
	}
	public static IServiceCollection AddQBCore(this IServiceCollection services, Action<AddQBCoreOptions>? setupOptions = null, params Assembly[] assemblies)
	{
		var options = new AddQBCoreOptions();
		if (setupOptions != null)
		{
			setupOptions(options);
		}

		AddQBCoreRegisters(services, options, assemblies);
		return services;
	}

	private static IServiceCollection AddQBCoreRegisters(this IServiceCollection services, AddQBCoreOptions options, params Assembly[] assemblies)
	{
		var types = assemblies
			.Where(x => options.AssemblySelector(x))
			.SelectMany(x => x.DefinedTypes)
			.Select(x => x.AsType())
			.Where(x => options.TypeSelector(x))
			.Distinct();

		return services.AddQBCoreRegisters(options, types);
	}
	private static IServiceCollection AddQBCoreRegisters(this IServiceCollection services, AddQBCoreOptions options, ApplicationPartManager partManager)
	{
		var types = partManager
			.ApplicationParts
			.Where(x => x is AssemblyPart)
			.Cast<AssemblyPart>()
			.Where(x => options.AssemblyPartSelector(x) && options.AssemblySelector(x.Assembly))
			.SelectMany(x => x.Types.Where(x => options.TypeSelector(x)))
			.Distinct();

		return services.AddQBCoreRegisters(options, types);
	}
	private static IServiceCollection AddQBCoreRegisters(this IServiceCollection services, AddQBCoreOptions options, IEnumerable<Type> types)
	{
		if (IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(AddQBCore) + " is already called.");
		}
		IsAddQBCoreCalled = true;

		var atypes = (types as Type[]) ?? types.ToArray();

		DataSourceDocuments.DocumentExclusionSelector = options.DocumentExclusionSelector;

#if DEBUG
		foreach (var type in types.Where(x => options.DataSourceSelector(x)))
		{
			DataSources.TryRegister(type);
		}
		foreach (var type in types.Where(x => options.ComplexDataSourceSelector(x)))
		{
			ComplexDataSources.TryRegister(type);
		}
#else
		types.AsParallel().Where(x => options.DataSourceSelector(x)).ForAll(x => DataSources.TryRegister(x));
		types.AsParallel().Where(x => options.ComplexDataSourceSelector(x)).ForAll(x => ComplexDataSources.TryRegister(x));
#endif

		AddBusinessObjects();

		services.TryAddSingleton<DataSourceRouteValueTransformer>();
		AddDataSourcesAsServices(services);

		return services;
	}

	private static void AddBusinessObjects()
	{
		var registry = (IFactoryObjectRegistry<string, BusinessObject>)StaticFactory.BusinessObjects;
		BusinessObject bo;
		
		foreach (var definition in StaticFactory.DataSources)
		{
			bo = new BusinessObject("DS", string.Intern(definition.Value.Name), definition.Value.DSTypeInfo.Concrete);
			
			registry.TryRegisterObject(bo.Key, bo);
		}
	}
	
	private static void AddDataSourcesAsServices(IServiceCollection services)
	{
		Type transientInterface, transientType = typeof(ITransient<>);
		Func<IServiceProvider, object> implementationFactory;

		foreach (var definition in StaticFactory.DataSources)
		{
			var dataSourceType = definition.Value.DSTypeInfo.Concrete;
			implementationFactory = sp => ActivatorUtilities.CreateInstance(sp, dataSourceType, sp, sp.GetRequiredService<IDataContextProvider>());

			if (definition.Value.DataSourceServiceType == dataSourceType)
			{
				if (definition.Value.IsServiceSingleton)
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
				if (definition.Value.IsServiceSingleton)
				{
					services.TryAddSingleton(definition.Value.DataSourceServiceType, implementationFactory);
				}
				else
				{
					services.TryAddScoped(definition.Value.DataSourceServiceType, implementationFactory);
				}

				transientInterface = transientType.MakeGenericType(definition.Value.DataSourceServiceType);
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