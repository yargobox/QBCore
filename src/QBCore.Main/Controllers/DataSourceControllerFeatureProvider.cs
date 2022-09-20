using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

/// <summary>
/// Populates a ControllerFeature collection with generic type controllers
/// </summary>
public class DataSourceControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
	public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
	{
		if (!ExtensionsForAddQBCore.IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(ExtensionsForAddQBCore.AddQBCore) + " must be called first.");
		}

		foreach (var info in StaticFactory.DataSources.Values.Where(x => x.BuildAutoController))
		{
			var dataSourceControllerType = typeof(DataSourceController<,,,,,,,>).MakeGenericType(
				info.DSTypeInfo.TKey,
				info.DSTypeInfo.TDocument,
				info.DSTypeInfo.TCreate,
				info.DSTypeInfo.TSelect,
				info.DSTypeInfo.TUpdate,
				info.DSTypeInfo.TDelete,
				info.DSTypeInfo.TRestore,
				info.DataSourceServiceType);

			feature.Controllers.Add(dataSourceControllerType.GetTypeInfo());
		}
	}
}