using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QBCore.ObjectFactory;

namespace QBCore.Configuration;

/// <summary>
/// Data context interface of the Entity Framework data layer
/// </summary>
public interface IEfDataContext : IDataContext, IDisposable
{
}

/// <summary>
/// Data context provider interface of the Entity Framework data layer
/// </summary>
/// <remarks>
/// Client code must implement this interface as a <see cref="EfDataContext" /> object factory
/// and add it to a DI container with a scope lifecycle.
/// </remarks>
public interface IEfDataContextProvider : IDataContextProvider, ITransient<IEfDataContextProvider>, IDisposable
{
	/// <summary>
    /// Creates a database context for model discovery purposes only.
    /// Setting up a database connection is not required in this case.
    /// </summary>
    /// <param name="dataContextName">a data context name to create</param>
    /// <returns>The caller is responsible for disposing the returned database context.</returns>
    /// <exception cref="InvalidOperationException">Unknown data context name</exception>
	DbContext CreateModelOrientedDbContext(string dataContextName = "default");
}

/// <summary>
/// Implementation of the data context interface of the Entity Framework data layer
/// </summary>
public class EfDataContext : DataContext, IEfDataContext
{
	public EfDataContext(DbContext context, string dataContextName = "default", IReadOnlyDictionary<string, object?>? args = null)
		: base(context, dataContextName, args)
	{
	}
}

public class EfDataContextModel : IModel, IDisposable
{
	private DbContext _dbContext;

	public EfDataContextModel(DbContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	void IDisposable.Dispose()
	{
		var temp = _dbContext as IDisposable;
		_dbContext = null!;
		temp?.Dispose();
	}

	public object? this[string name] => ((IReadOnlyAnnotatable)_dbContext.Model)[name];

	public IAnnotation AddRuntimeAnnotation(string name, object? value)
	{
		return _dbContext.Model.AddRuntimeAnnotation(name, value);
	}

	public IAnnotation? FindAnnotation(string name)
	{
		return _dbContext.Model.FindAnnotation(name);
	}

	public IEntityType? FindEntityType(string name)
	{
		return _dbContext.Model.FindEntityType(name);
	}

	public IEntityType? FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
	{
		return _dbContext.Model.FindEntityType(name, definingNavigationName, definingEntityType);
	}

	public IEntityType? FindEntityType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] Type type)
	{
		return _dbContext.Model.FindEntityType(type);
	}

	public IReadOnlyEntityType? FindEntityType(string name, string definingNavigationName, IReadOnlyEntityType definingEntityType)
	{
		return _dbContext.Model.FindEntityType(name, definingNavigationName, definingEntityType);
	}

	public IReadOnlyEntityType? FindEntityType(Type type, string definingNavigationName, IReadOnlyEntityType definingEntityType)
	{
		return _dbContext.Model.FindEntityType(type, definingNavigationName, definingEntityType);
	}

	public IEnumerable<IEntityType> FindEntityTypes(Type type)
	{
		return _dbContext.Model.FindEntityTypes(type);
	}

	public IAnnotation? FindRuntimeAnnotation(string name)
	{
		return _dbContext.Model.FindRuntimeAnnotation(name);
	}

	public ITypeMappingConfiguration? FindTypeMappingConfiguration(Type scalarType)
	{
		return _dbContext.Model.FindTypeMappingConfiguration(scalarType);
	}

	public IEnumerable<IAnnotation> GetAnnotations()
	{
		return _dbContext.Model.GetAnnotations();
	}

	public ChangeTrackingStrategy GetChangeTrackingStrategy()
	{
		return _dbContext.Model.GetChangeTrackingStrategy();
	}

	public IEnumerable<IEntityType> GetEntityTypes()
	{
		return _dbContext.Model.GetEntityTypes();
	}

	public TValue GetOrAddRuntimeAnnotationValue<TValue, TArg>(string name, Func<TArg?, TValue> valueFactory, TArg? factoryArgument)
	{
		return _dbContext.Model.GetOrAddRuntimeAnnotationValue(name, valueFactory, factoryArgument);
	}

	public PropertyAccessMode GetPropertyAccessMode()
	{
		return _dbContext.Model.GetPropertyAccessMode();
	}

	public IEnumerable<IAnnotation> GetRuntimeAnnotations()
	{
		return _dbContext.Model.GetRuntimeAnnotations();
	}

	public IEnumerable<ITypeMappingConfiguration> GetTypeMappingConfigurations()
	{
		return _dbContext.Model.GetTypeMappingConfigurations();
	}

	public bool IsIndexerMethod(MethodInfo methodInfo)
	{
		return _dbContext.Model.IsIndexerMethod(methodInfo);
	}

	public bool IsShared([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
	{
		return _dbContext.Model.IsShared(type);
	}

	public IAnnotation? RemoveRuntimeAnnotation(string name)
	{
		return _dbContext.Model.RemoveRuntimeAnnotation(name);
	}

	public IAnnotation SetRuntimeAnnotation(string name, object? value)
	{
		return _dbContext.Model.SetRuntimeAnnotation(name, value);
	}

	IReadOnlyEntityType? IReadOnlyModel.FindEntityType(string name)
	{
		return ((IReadOnlyModel)_dbContext.Model).FindEntityType(name);
	}

	IReadOnlyEntityType? IReadOnlyModel.FindEntityType(Type type)
	{
		return ((IReadOnlyModel)_dbContext.Model).FindEntityType(type);
	}

	IEnumerable<IReadOnlyEntityType> IReadOnlyModel.FindEntityTypes(Type type)
	{
		return ((IReadOnlyModel)_dbContext.Model).FindEntityTypes(type);
	}

	IEnumerable<IReadOnlyEntityType> IReadOnlyModel.GetEntityTypes()
	{
		return ((IReadOnlyModel)_dbContext.Model).GetEntityTypes();
	}
}

public static class ExtensionsFoEfDataContext
{
	/// <summary>
	/// Converts the IDataContext.Context property to <see cref="DbContext" />.
	/// </summary>
	/// <exception cref="ArgumentException">when dataContext or dataContext.Context is null or no conversion is possible</exception>
	public static DbContext AsDbContext(this IDataContext dataContext)
	{
		return dataContext?.Context as DbContext ?? throw new ArgumentException(nameof(dataContext));
	}

	/// <summary>
	/// Converts the IDataContext.Context property to the given parameter type of <see cref="DbContext" />.
	/// </summary>
	/// <exception cref="ArgumentException">when dataContext or dataContext.Context is null or no conversion is possible</exception>
	public static TDbContext AsDbContext<TDbContext>(this IDataContext dataContext) where TDbContext : DbContext
	{
		return dataContext?.Context as TDbContext ?? throw new ArgumentException(nameof(dataContext));
	}
}