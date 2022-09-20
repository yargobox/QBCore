using System.Reflection;
using QBCore.Extensions.Text;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal class CDSInfo : ICDSInfo
{
	public string Name { get; }
	public string Tech => "CDS";
	public Type ConcreteType { get; }
	public string? ControllerName { get; }

	public IReadOnlyDictionary<string, ICDSNodeInfo> Nodes { get; }

	public CDSInfo(Type concreteType)
	{
		if (!concreteType.IsClass || concreteType.IsAbstract || concreteType.IsGenericType || concreteType.IsGenericTypeDefinition
				|| concreteType.GetSubclassOf(typeof(ComplexDataSource<>)) == null)
		{
			throw new InvalidOperationException($"Invalid complex datasource type {concreteType.ToPretty()}.");
		}
		ConcreteType = concreteType;

		// Our building
		//
		var building = new CDSBuilder(ConcreteType);

		// Load fields from [ComplexDataSource]
		//
		var attr = ConcreteType.GetCustomAttribute<ComplexDataSourceAttribute>(false);
		if (attr != null)
		{
			building.Name = attr.Name;
		}

		// Load fields from [CdsApiController]
		//
		var controllerAttr = ConcreteType.GetCustomAttribute<CdsApiControllerAttribute>(false);
		if (controllerAttr != null)
		{
			building.ControllerName = controllerAttr.Name;
		}

		// Find a builder and build
		//
		var builder = FactoryHelper.FindRequiredBuilder<ICDSBuilder>(attr?.Builder ?? ConcreteType, attr?.BuilderMethod);
		builder(building);

		// Name
		//
		if (building.Name != null)
		{
			var name = building.Name.Trim();
			
			if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('*'))
			{
				throw new ArgumentException($"{nameof(ICDSBuilder)}.{nameof(ICDSBuilder.Name)}");
			}

			if (name.Contains("[CDS]", StringComparison.OrdinalIgnoreCase))
			{
				Name = name.Replace("[CDS]", MakeCDSNameFromType(ConcreteType), StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				Name = name;
			}
		}
		else
		{
			Name = MakeCDSNameFromType(ConcreteType);
		}

		Name = string.Intern(Name);

		if (DSInfo.ReservedNames.Contains(Name, StringComparer.OrdinalIgnoreCase))
		{
			throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", DSInfo.ReservedNames));
		}

		// ControllerName
		//
		if (building.ControllerName != null)
		{
			var controllerName = building.ControllerName;

			if (string.IsNullOrWhiteSpace(controllerName) || controllerName.Contains('/') || controllerName.Contains('*'))
			{
				throw new ArgumentException(nameof(ControllerName));
			}

			if (controllerName.Contains("[CDS:guessPlural]", StringComparison.OrdinalIgnoreCase))
			{
				ControllerName = controllerName.Replace("[CDS:guessPlural]", DSInfo.GuessPluralName(Name), StringComparison.OrdinalIgnoreCase);
			}
			else if (controllerName.Contains("[CDS]", StringComparison.OrdinalIgnoreCase))
			{
				ControllerName = controllerName.Replace("[CDS]", Name, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				ControllerName = controllerName;
			}

			ControllerName = string.Intern(ControllerName);

			if (DSInfo.ReservedNames.Contains(ControllerName, StringComparer.OrdinalIgnoreCase))
			{
				throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", DSInfo.ReservedNames));
			}
		}

		// Nodes
		//
		Nodes = ((CDSNodeBuilder)building.NodeBuilder).AsNode().All;
		if (Nodes.Count == 0)
		{
			throw new InvalidOperationException($"Complex datasource '{Name}' must have at least one node.");
		}
	}

	private static string MakeCDSNameFromType(Type concreteType)
	{
		var fromClassName = concreteType.Name.ReplaceEnding("CDS").ReplaceEnding("Cds");
		if (string.IsNullOrEmpty(fromClassName))
		{
			fromClassName = concreteType.Name;
		}
		return fromClassName;
	}
}