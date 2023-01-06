using AutoMapper;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

public class DataSourceMappings : Profile
{
	public DataSourceMappings()
	{
		foreach (var info in StaticFactory.DataSources.Values)
		{
			if (info.DSTypeInfo.TCreate != info.DSTypeInfo.TDoc && info.DSTypeInfo.TCreate != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanInsert))
			{
				CreateMap(info.DSTypeInfo.TCreate, info.DSTypeInfo.TDoc);
			}
			if (info.DSTypeInfo.TUpdate != info.DSTypeInfo.TDoc && info.DSTypeInfo.TUpdate != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanUpdate))
			{
				CreateMap(info.DSTypeInfo.TUpdate, info.DSTypeInfo.TDoc);
			}
			if (info.DSTypeInfo.TSelect != info.DSTypeInfo.TSelect && info.DSTypeInfo.TSelect != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanSelect))
			{
				CreateMap(info.DSTypeInfo.TDoc, info.DSTypeInfo.TSelect);
			}
		}
	}

	public DataSourceMappings(Func<Type, Type, bool> createDefaultMap)
	{
		if (createDefaultMap == null)
		{
			throw new ArgumentNullException(nameof(createDefaultMap));
		}

		foreach (var info in StaticFactory.DataSources.Values)
		{
			if (info.DSTypeInfo.TCreate != info.DSTypeInfo.TDoc && info.DSTypeInfo.TCreate != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanInsert))
			{
				if (createDefaultMap(info.DSTypeInfo.TCreate, info.DSTypeInfo.TDoc))
				{
					CreateMap(info.DSTypeInfo.TCreate, info.DSTypeInfo.TDoc);
				}
			}
			if (info.DSTypeInfo.TUpdate != info.DSTypeInfo.TDoc && info.DSTypeInfo.TUpdate != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanUpdate))
			{
				if (createDefaultMap(info.DSTypeInfo.TUpdate, info.DSTypeInfo.TDoc))
				{
					CreateMap(info.DSTypeInfo.TUpdate, info.DSTypeInfo.TDoc);
				}
			}
			if (info.DSTypeInfo.TSelect != info.DSTypeInfo.TDoc && info.DSTypeInfo.TSelect != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanSelect))
			{
				if (createDefaultMap(info.DSTypeInfo.TDoc, info.DSTypeInfo.TSelect))
				{
					CreateMap(info.DSTypeInfo.TDoc, info.DSTypeInfo.TSelect);
				}
			}
		}
	}
}