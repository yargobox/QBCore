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

		foreach (var desc in StaticFactory.DataSources.Values
			.Where(x => x.DataSourceConcreteType.GetCustomAttribute<DsApiControllerAttribute>(false)?.AutoBuild == true))
		{
			var dataSourceControllerType = typeof(DataSourceController<,,,,,,>).MakeGenericType(
				desc.IdType,
				desc.DocumentType,
				desc.CreateDocumentType,
				desc.SelectDocumentType,
				desc.UpdateDocumentType,
				desc.DeleteDocumentType,
				desc.DataSourceServiceType);

			feature.Controllers.Add(dataSourceControllerType.GetTypeInfo());
		}
	}
}