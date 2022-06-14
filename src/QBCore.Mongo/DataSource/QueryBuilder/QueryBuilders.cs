using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

public static class QueryBuilders
{
	public static void RegisterInsert<TDocument, TInsert>(Action<IQBBuilder<TDocument, TInsert>> builder)
		=> QueryBuilders.RegisterInsert<TDocument, TInsert>(StaticFactory.QueryBuilders, builder);

	public static void RegisterInsert<TDocument, TInsert>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Action<IQBBuilder<TDocument, TInsert>> builder)
	{
		var registry = (IFactoryObjectRegistry<Type, Func<IQueryBuilder>>)factoryObjects;

		var building = new QBBuilder<TDocument, TInsert>();
		builder(building);

		registry.RegisterObject(typeof(IInsertQueryBuilder<TDocument, TInsert>), () => new InsertQueryBuilder<TDocument, TInsert>(new QBBuilder<TDocument, TInsert>(building)));
	}


	public static void RegisterSelect<TDocument, TSelect>(Action<IQBBuilder<TDocument, TSelect>> builder)
		=> QueryBuilders.RegisterSelect<TDocument, TSelect>(StaticFactory.QueryBuilders, builder);

	public static void RegisterSelect<TDocument, TSelect>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Action<IQBBuilder<TDocument, TSelect>> builder)
	{
		var registry = (IFactoryObjectRegistry<Type, Func<IQueryBuilder>>)factoryObjects;
		
		var building = new QBBuilder<TDocument, TSelect>();
		builder(building);

		registry.RegisterObject(typeof(ISelectQueryBuilder<TDocument, TSelect>), () => new SelectQueryBuilder<TDocument, TSelect>(new QBBuilder<TDocument, TSelect>(building)));
	}


	public static void RegisterUpdate<TDocument, TUpdate>(Action<IQBBuilder<TDocument, TUpdate>> builder)
		=> QueryBuilders.RegisterUpdate<TDocument, TUpdate>(StaticFactory.QueryBuilders, builder);

	public static void RegisterUpdate<TDocument, TUpdate>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Action<IQBBuilder<TDocument, TUpdate>> builder)
	{
		var registry = (IFactoryObjectRegistry<Type, Func<IQueryBuilder>>)factoryObjects;
		
		var building = new QBBuilder<TDocument, TUpdate>();
		builder(building);

		registry.RegisterObject(typeof(IUpdateQueryBuilder<TDocument, TUpdate>), () => new UpdateQueryBuilder<TDocument, TUpdate>(new QBBuilder<TDocument, TUpdate>(building)));
	}


	public static void RegisterDelete<TDocument, TDelete>(Action<IQBBuilder<TDocument, TDelete>> builder)
		=> QueryBuilders.RegisterDelete<TDocument, TDelete>(StaticFactory.QueryBuilders, builder);

	public static void RegisterDelete<TDocument, TDelete>(this IFactoryObjectDictionary<Type, Func<IQueryBuilder>> factoryObjects, Action<IQBBuilder<TDocument, TDelete>> builder)
	{
		var registry = (IFactoryObjectRegistry<Type, Func<IQueryBuilder>>)factoryObjects;
		
		var building = new QBBuilder<TDocument, TDelete>();
		builder(building);

		registry.RegisterObject(typeof(IDeleteQueryBuilder<TDocument, TDelete>), () => new DeleteQueryBuilder<TDocument, TDelete>(new QBBuilder<TDocument, TDelete>(building)));
	}
}