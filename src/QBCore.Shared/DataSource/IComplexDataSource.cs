namespace QBCore.DataSource;

public interface IComplexDataSource
{
	ICDSInfo CDSInfo { get; }
	IReadOnlyDictionary<string, IDataSource> Nodes { get; }
	IDataSource Root { get; }
}