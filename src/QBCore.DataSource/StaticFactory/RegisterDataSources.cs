using QBCore.DataSource;

namespace QBCore.ObjectFactory;

public static class RegisterDataSources
{
	public static void FromTypes(IEnumerable<Type> types)
	{
		var registry = (IFactoryObjectRegistry<Type, IDataSourceDesc>)StaticFactory.DataSources;

		foreach (var type in types.Where(x => x.IsClass && !x.IsAbstract && !x.IsInterface && x.IsDefined(typeof(DataSourceAttribute), false)))
		{
			var desc = new DataSourceDesc(type);
			registry.TryRegisterObject(type, desc);
		}
	}
}