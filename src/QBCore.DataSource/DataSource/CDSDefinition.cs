using System.Reflection;
using QBCore.DataSource.Builders;

namespace QBCore.DataSource;

internal class CDSDefinition : ICDSDefinition
{
	public string Name { get; }
	public Type ComplexDataSourceType { get; }
	public IReadOnlyDictionary<string, ICDSNode> Nodes { get; }

	public CDSDefinition(Type concreteType)
	{
		if (!concreteType.IsClass || concreteType.IsAbstract || concreteType.IsGenericType || concreteType.IsGenericTypeDefinition)
		{
			throw new InvalidOperationException($"Invalid complex datasource type {concreteType.ToPretty()}.");
		}

		ComplexDataSourceType = concreteType;

		var building = new CDSBuilder();

		// Get attr [ComplexDataSource], get name
		//
		var attr = ComplexDataSourceType.GetCustomAttribute<ComplexDataSourceAttribute>(false);
		if (attr?.Name != null)
		{
			building.Name = attr.Name;
		}

		// Find builder
		//
		var builderType = attr?.Builder ?? ComplexDataSourceType;
		var builderMethod = attr?.BuilderMethod;
		MethodInfo? methodInfo = null;
		FieldInfo? fieldInfo = null;

		if (builderMethod != null)
		{
			methodInfo = builderType.GetMethod(builderMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[] { typeof(ICDSBuilder) });
			if (methodInfo == null)
			{
				fieldInfo = builderType.GetField(builderMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					if (!fieldInfo.IsInitOnly || fieldInfo.FieldType != typeof(Action<ICDSBuilder>))
					{
						fieldInfo = null;
					}
				}
			}

			if (methodInfo == null && fieldInfo == null)
			{
				throw new InvalidOperationException($"Could not find CDS builder '{builderType.ToPretty()}.{builderMethod}({nameof(ICDSBuilder)})'.");
			}
		}
		else
		{
			var methodInfos =
				builderType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(x =>
					!x.IsGenericMethod &&
					x.ReturnType == typeof(void) &&
					IsSingleICDSBuilderParameter(x.GetParameters()))
				.ToArray();

			if (methodInfos.Length > 1)
			{
				throw new InvalidOperationException($"There is more than one CDS builder '{builderType.ToPretty()}.{{many}}.({nameof(ICDSBuilder)})'.");
			}

			var fieldInfos =
				builderType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(x =>
					x.IsInitOnly &&
					x.FieldType == typeof(Action<ICDSBuilder>))
				.ToArray();
			
			if (fieldInfos.Length > 1 || (fieldInfos.Length > 0 && methodInfos.Length > 0))
			{
				throw new InvalidOperationException($"There is more than one CDS builder '{builderType.ToPretty()}.{{many}}.({nameof(ICDSBuilder)})'.");
			}
			
			if (methodInfos.Length < 1 && fieldInfos.Length < 1)
			{
				throw new InvalidOperationException($"There is no CDS builder '{builderType.ToPretty()}.{{none}}.({nameof(ICDSBuilder)})'.");
			}
			else if (methodInfos.Length == 1)
			{
				methodInfo = methodInfos[0];
			}
			else/* if (fieldInfos.Length == 1) */
			{
				fieldInfo = fieldInfos[0];
			}
		}

		// Call builder
		//
		if (methodInfo != null)
		{
			methodInfo.Invoke(null, new object[] { building });
		}
		else
		{
			var builder = (Action<ICDSBuilder>?) fieldInfo!.GetValue(null);
			if (builder == null)
			{
				throw new InvalidOperationException($"CDS builder '{builderType.ToPretty()}.{fieldInfo.Name}.({nameof(ICDSBuilder)})' is null.");
			}
			builder(building);
		}

		// Name
		//
		if (string.IsNullOrWhiteSpace(building.Name))
		{
			throw new ArgumentNullException($"{nameof(ICDSBuilder)}.{nameof(ICDSBuilder.Name)}");
		}
		if (building.Name.Contains("[CDS]", StringComparison.OrdinalIgnoreCase))
		{
			var name = ComplexDataSourceType.Name;
			if (name.EndsWith("CDS"))
			{
				name = name.Substring(0, name.Length - 3);
			}
			Name = building.Name.Replace("[CDS]", name);
		}
		else
		{
			Name = building.Name;
		}
		Name = string.Intern(Name);
		if (DataSourceDesc.ReservedNames.Contains(Name, StringComparer.OrdinalIgnoreCase))
		{
			throw new ArgumentException("These names are reserved and cannot be used as names for a datasource, CDS, or controller: " + string.Join(", ", DataSourceDesc.ReservedNames));
		}

		// Nodes
		//
		Nodes = ((CDSNodeBuilder)building.NodeBuilder).AsNode().All;
		if (Nodes.Count == 0)
		{
			throw new InvalidOperationException($"Complex datasource '{Name}' must have at least one node.");
		}
	}

	private static bool IsSingleICDSBuilderParameter(ParameterInfo[] parameters)
	{
		return parameters.Length == 1 && parameters[0].ParameterType == typeof(ICDSBuilder);
	}
}