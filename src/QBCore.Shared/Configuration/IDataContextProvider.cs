using QBCore.ObjectFactory;

namespace QBCore.Configuration;

public interface IDataContextProvider : ITransient<IDataContextProvider>
{
	IDataContext GetDataContext(Type databaseContextType, string dataContextName = "default");
}