using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface ICDSInfo : IAppObjectInfo
{
	IReadOnlyDictionary<string, ICDSNodeInfo> Nodes { get; }
}