using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public interface IDataSourceListener
{
	OKeyName KeyName { get; }
	ValueTask OnAttachAsync(IDataSource dataSource);
	ValueTask OnDetachAsync(IDataSource dataSource);
}