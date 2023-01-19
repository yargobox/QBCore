using AutoMapper;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

public class DataSourceMappings : Profile
{
	public DataSourceMappings() : this((_, _) => true)
	{
	}

	public DataSourceMappings(Func<Type, Type, bool> createMapSelector)
	{
		if (createMapSelector == null) throw new ArgumentNullException(nameof(createMapSelector));

		foreach (var info in StaticFactory.DataSources.Values)
		{
			if (info.DSTypeInfo.TCreate != info.DSTypeInfo.TDoc && info.DSTypeInfo.TCreate != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanInsert))
			{
				if (createMapSelector(info.DSTypeInfo.TCreate, info.DSTypeInfo.TDoc))
				{
					CreateMap(info.DSTypeInfo.TCreate, info.DSTypeInfo.TDoc);
				}
			}
		}
	}
}