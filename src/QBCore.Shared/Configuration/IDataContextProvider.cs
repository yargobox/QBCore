using QBCore.DataSource;

namespace QBCore.Configuration;

public interface IDataContextProvider
{
	IDataContext GetContext(Type databaseContextType, string dataContextName = "default");
	IDataContext GetContext<TDatabaseContext>(string dataContextName = "default");
}