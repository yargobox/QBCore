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
		if (!AddQBCoreExtensions.IsAddQBCoreCalled)
		{
			throw new InvalidOperationException(nameof(AddQBCoreExtensions.AddQBCore) + " must be called first.");
		}

		foreach (var definition in StaticFactory.DataSources.Values.Where(x => x.IsAutoController == true))
		{
			var dataSourceControllerType = typeof(DataSourceController<,,,,,,,>).MakeGenericType(
				definition.KeyType,
				definition.DocumentType,
				definition.CreateType,
				definition.SelectType,
				definition.UpdateType,
				definition.DeleteType,
				definition.RestoreType,
				definition.DataSourceService);

			feature.Controllers.Add(dataSourceControllerType.GetTypeInfo());
		}
	}
}