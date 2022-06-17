using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder;

namespace QBCore.ObjectFactory;

public static class StaticFactory
{
	public static IFactoryObjectDictionary<Type, Func<IQueryBuilder>> QueryBuilders { get; } = new FactoryObjectRegistry<Type, Func<IQueryBuilder>>();
	public static IFactoryObjectDictionary<Type, IDataSourceDesc> DataSources { get; } = new FactoryObjectRegistry<Type, IDataSourceDesc>();
	public static IFactoryObjectDictionary<Type, ICDSDefinition> ComplexDataSources { get; } = new FactoryObjectRegistry<Type, ICDSDefinition>();
	public static IFactoryObjectDictionary<string, BusinessObject> BusinessObjects { get; } = new FactoryObjectRegistry<string, BusinessObject>();




	public static Func<IInsertQueryBuilder<TDocument, TCreate>>? TryGetInsert<TDocument, TCreate>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		Func<IQueryBuilder>? qb;
		return factoryObjects.TryGetValue(typeof(IInsertQueryBuilder<TDocument, TCreate>), out qb) ?
			() => (IInsertQueryBuilder<TDocument, TCreate>)qb() : null;
	}
	public static Func<IQueryBuilder>? TryGetInsert(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type createDocumentType)
	{
		var type = typeof(IInsertQueryBuilder<,>).MakeGenericType(documentType, createDocumentType);
		return factoryObjects.GetValueOrDefault(type);
	}
	public static Func<IInsertQueryBuilder<TDocument, TCreate>> GetInsert<TDocument, TCreate>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		return TryGetInsert<TDocument, TCreate>(factoryObjects) ??
			throw new InvalidOperationException($"Query builder {typeof(IInsertQueryBuilder<TDocument, TCreate>).ToPretty()} is not registered.");
	}
	public static Func<IQueryBuilder> GetInsert(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type createDocumentType)
	{
		var type = typeof(IInsertQueryBuilder<,>).MakeGenericType(documentType, createDocumentType);
		return factoryObjects.GetValueOrDefault(type) ??
			throw new InvalidOperationException($"Query builder {type.ToPretty()} is not registered.");
	}

	public static Func<ISelectQueryBuilder<TDocument, TSelect>>? TryGetSelect<TDocument, TSelect>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		Func<IQueryBuilder>? qb;
		return factoryObjects.TryGetValue(typeof(ISelectQueryBuilder<TDocument, TSelect>), out qb) ?
			() => (ISelectQueryBuilder<TDocument, TSelect>)qb() : null;
	}
	public static Func<IQueryBuilder>? TryGetSelect(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type selectDocumentType)
	{
		var type = typeof(ISelectQueryBuilder<,>).MakeGenericType(documentType, selectDocumentType);
		return factoryObjects.GetValueOrDefault(type);
	}
	public static Func<ISelectQueryBuilder<TDocument, TSelect>> GetSelect<TDocument, TSelect>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		return TryGetSelect<TDocument, TSelect>(factoryObjects) ??
			throw new InvalidOperationException($"Query builder {typeof(ISelectQueryBuilder<TDocument, TSelect>).ToPretty()} is not registered.");
	}
	public static Func<IQueryBuilder> GetSelect(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type selectDocumentType)
	{
		var type = typeof(ISelectQueryBuilder<,>).MakeGenericType(documentType, selectDocumentType);
		return factoryObjects.GetValueOrDefault(type) ??
			throw new InvalidOperationException($"Query builder {type.ToPretty()} is not registered.");
	}

	public static Func<IUpdateQueryBuilder<TDocument, TUpdate>>? TryGetUpdate<TDocument, TUpdate>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		Func<IQueryBuilder>? qb;
		return factoryObjects.TryGetValue(typeof(IUpdateQueryBuilder<TDocument, TUpdate>), out qb) ?
			() => (IUpdateQueryBuilder<TDocument, TUpdate>)qb() : null;
	}
	public static Func<IQueryBuilder>? TryGetUpdate(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type updateDocumentType)
	{
		var type = typeof(IUpdateQueryBuilder<,>).MakeGenericType(documentType, updateDocumentType);
		return factoryObjects.GetValueOrDefault(type);
	}
	public static Func<IUpdateQueryBuilder<TDocument, TUpdate>> GetUpdate<TDocument, TUpdate>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		return TryGetUpdate<TDocument, TUpdate>(factoryObjects) ??
			throw new InvalidOperationException($"Query builder {typeof(IUpdateQueryBuilder<TDocument, TUpdate>).ToPretty()} is not registered.");
	}
	public static Func<IQueryBuilder> GetUpdate(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type updateDocumentType)
	{
		var type = typeof(IUpdateQueryBuilder<,>).MakeGenericType(documentType, updateDocumentType);
		return factoryObjects.GetValueOrDefault(type) ??
			throw new InvalidOperationException($"Query builder {type.ToPretty()} is not registered.");
	}

	public static Func<IDeleteQueryBuilder<TDocument, TDelete>>? TryGetDelete<TDocument, TDelete>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		Func<IQueryBuilder>? qb;
		return factoryObjects.TryGetValue(typeof(IDeleteQueryBuilder<TDocument, TDelete>), out qb) ?
			() => (IDeleteQueryBuilder<TDocument, TDelete>)qb() : null;
	}
	public static Func<IQueryBuilder>? TryGetDelete(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type deleteDocumentType)
	{
		var type = typeof(IDeleteQueryBuilder<,>).MakeGenericType(documentType, deleteDocumentType);
		return factoryObjects.GetValueOrDefault(type);
	}
	public static Func<IDeleteQueryBuilder<TDocument, TDelete>> GetDelete<TDocument, TDelete>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects)
	{
		return TryGetDelete<TDocument, TDelete>(factoryObjects) ??
			throw new InvalidOperationException($"Query builder {typeof(IDeleteQueryBuilder<TDocument, TDelete>).ToPretty()} is not registered.");
	}
	public static Func<IQueryBuilder> GetDelete(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Type documentType, Type deleteDocumentType)
	{
		var type = typeof(IDeleteQueryBuilder<,>).MakeGenericType(documentType, deleteDocumentType);
		return factoryObjects.GetValueOrDefault(type) ??
			throw new InvalidOperationException($"Query builder {type.ToPretty()} is not registered.");
	}
}