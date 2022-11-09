using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDataSourceListener
{
	abstract OKeyName KeyName { get; }
	abstract ValueTask OnAttachAsync(IDataSource dataSource);
	abstract ValueTask OnDetachAsync(IDataSource dataSource);
}