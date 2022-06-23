using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract class ComplexDataSource<TComplexDataSource> : IComplexDataSource where TComplexDataSource : notnull, IComplexDataSource
{
	public ICDSDefinition Definition { get; }

	public ComplexDataSource()
	{
		Definition = StaticFactory.ComplexDataSources[typeof(TComplexDataSource)];
	}
}