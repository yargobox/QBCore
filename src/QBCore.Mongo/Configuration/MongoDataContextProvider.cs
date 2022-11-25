using MongoDB.Driver;
using QBCore.ObjectFactory;

namespace QBCore.Configuration;

/// <summary>
/// Data context interface of the Mongo data layer
/// </summary>
public interface IMongoDataContext : IDataContext, IDisposable
{
}

/// <summary>
/// Data context provider interfaace of the Mongo data layer
/// </summary>
/// <remarks>
/// Client code must implement this interface as a <see cref="MongoDataContext" /> object factory
/// and add it to a DI container with a singleton lifecycle.
/// </remarks>
public interface IMongoDataContextProvider : IDataContextProvider, ITransient<IMongoDataContextProvider>, IDisposable
{
}

/// <summary>
/// Implementation of the data context interface of the Mongo data layer
/// </summary>
public class MongoDataContext : DataContext, IMongoDataContext
{
	public MongoDataContext(IMongoDatabase context, string dataContextName = "default", IReadOnlyDictionary<string, object?>? args = null)
		: base(context, dataContextName, args)
	{
	}
}

public static class ExtensionsForMongoDataContext
{
	/// <summary>
	/// Convert the IDataContext.Context property to <see cref="IMongoDatabase" />.
	/// </summary>
	/// <param name="dataContext"></param>
	/// <returns><see cref="IMongoDatabase" /></returns>
	/// <exception cref="ArgumentException">when dataContext or dataContext.Context is null or no conversion is possible</exception>
	public static IMongoDatabase AsMongoDatabase(this IDataContext dataContext)
	{
		return dataContext?.Context as IMongoDatabase ?? throw new ArgumentException(nameof(dataContext));
	}
}