using System.Reflection;
using QBCore.DataSource.Builders;
using QBCore.Extensions.Text;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

internal class CDSDefinition : ICDSDefinition
{
	public string Name { get; }
	public Type ComplexDataSourceConcrete { get; }
	public IReadOnlyDictionary<string, ICDSNode> Nodes { get; }

	public CDSDefinition(Type concreteType)
	{
		if (!concreteType.IsClass || concreteType.IsAbstract || concreteType.IsGenericType || concreteType.IsGenericTypeDefinition
				|| concreteType.GetSubclassOf(typeof(ComplexDataSource<>)) == null)
		{
			throw new InvalidOperationException($"Invalid complex datasource type {concreteType.ToPretty()}.");
		}
		ComplexDataSourceConcrete = concreteType;

		// Our building
		//
		var building = new CDSBuilder(ComplexDataSourceConcrete);

		// Load fields from [ComplexDataSource]
		//
		var attr = ComplexDataSourceConcrete.GetCustomAttribute<ComplexDataSourceAttribute>(false);
		if (attr != null)
		{
			building.Name = attr.Name;
		}

		// Find a builder and build
		//
		var builder = FactoryHelper.FindRequiredBuilder<ICDSBuilder>(attr?.Builder ?? ComplexDataSourceConcrete, attr?.BuilderMethod);
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
				Name = name.Replace("[CDS]", MakeCDSNameFromType(ComplexDataSourceConcrete), StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				Name = name;
			}
		}
		else
		{
			Name = MakeCDSNameFromType(ComplexDataSourceConcrete);
		}
		Name = string.Intern(Name);
		if (DSDefinition.ReservedNames.Contains(Name, StringComparer.OrdinalIgnoreCase))
		{
			throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", DSDefinition.ReservedNames));
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