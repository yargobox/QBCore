namespace QBCore.DataSource;

public interface IDataSourceListener
{
	abstract ValueTask OnAttachAsync(IDataSource dataSource);
	abstract ValueTask OnDetachAsync(IDataSource dataSource);
}