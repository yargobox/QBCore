using AutoMapper;
using AutoMapper.Configuration;
using QBCore.DataSource;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

public class DataSourceMappings : Profile
{
	public DataSourceMappings()
	{
		foreach (var info in StaticFactory.DataSources.Values)
		{
			if (info.CreateType != info.DocumentType && info.CreateType != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanInsert))
			{
				CreateMap(info.CreateType, info.DocumentType);
			}
			if (info.UpdateType != info.DocumentType && info.UpdateType != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanUpdate))
			{
				CreateMap(info.UpdateType, info.DocumentType);
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
			if (info.CreateType != info.DocumentType && info.CreateType != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanInsert))
			{
				if (createDefaultMap(info.CreateType, info.DocumentType))
				{
					CreateMap(info.CreateType, info.DocumentType);
				}
			}
			if (info.UpdateType != info.DocumentType && info.UpdateType != typeof(NotSupported) && info.Options.HasFlag(DataSourceOptions.CanUpdate))
			{
				if (createDefaultMap(info.CreateType, info.DocumentType))
				{
					CreateMap(info.UpdateType, info.DocumentType);
				}
			}
		}
	}
}