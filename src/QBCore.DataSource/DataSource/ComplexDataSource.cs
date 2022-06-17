using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract class ComplexDataSource<TComplexDataSource> : IComplexDataSource where TComplexDataSource : notnull, IComplexDataSource
{
	private static readonly CDSDefinition _definition;

	public ICDSDefinition Definition => _definition;

	static ComplexDataSource()
	{
		_definition = (CDSDefinition) StaticFactory.ComplexDataSources[typeof(TComplexDataSource)];
	}
}