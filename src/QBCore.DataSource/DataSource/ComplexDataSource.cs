using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract class ComplexDataSource<TComplexDataSource> : IComplexDataSource where TComplexDataSource : notnull, IComplexDataSource
{
	public ICDSInfo CDSInfo { get; }

	public ComplexDataSource()
	{
		CDSInfo = StaticFactory.ComplexDataSources[typeof(TComplexDataSource)];
	}
}