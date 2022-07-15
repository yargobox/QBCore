using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Stores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Orders;

public class OrderSelectDto
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? StoreId { get; set; }

	public string? StoreName { get; set; }
	public StoreSelectDto? Store { get; set; }
	public StoreSelectDto? Store3 { get; set; }

	public List<OrderPositionSelectDto>? OrderPositions { get; set; }

	public decimal? Total { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	static void Builder(IQBMongoSelectBuilder<Order, OrderSelectDto> builder)
	{
		/*
		SELECT * FROM orders AS orders
		LEFT JOIN stores AS stores ON stores.Id = orders.StoreId
		LEFT JOIN stores AS stores2 ON stores2.Id = orders.StoreId
		LEFT JOIN stores AS stores3 ON stores3.Id = stores2.Id
		WHERE stores3.Deleted = orders.Deleted
		*/

		builder
			.SelectFromTable("orders")
				//.Optional(sel => sel.Updated)

  			.LeftJoinTable<Store>("stores")
				.Connect<Store, Order>(store => store.Id, order => order.StoreId, ConditionOperations.Equal)
				.Include<Store>(sel => sel.StoreName, "stores", store => store.Name)

			.LeftJoinTable<Store>("stores2", "stores")
				.Connect<Store, Order>("stores2", store => store.Id, "orders", order => order.StoreId, ConditionOperations.Equal)
				.Connect<Store, Store>("stores2", store => store.Created, "stores", store => store.Deleted, ConditionOperations.NotEqual)
				.Connect<Store>("stores2", store => store.Updated, null, ConditionOperations.IsNull)
				.Include<Store>(sel => sel.Store, "stores2", store => store)
				//.Exclude(sel => sel.Store!.Created)
				//.Optional(sel => sel.Store!.Updated)

/*			.LeftJoinTable<Store>("stores3", "stores")
				.Connect<Store, Store>("stores3", store => store.Id, "stores2", store2 => store2.Id, ConditionOperations.Equal)
				.Include<Store>(sel => sel.Store3!.Name, "stores3", store => store.Name)

			.Condition<Store, Order>("stores3", store => store.Deleted, "orders", order => order.Deleted, ConditionOperations.Equal)
			.And()
			.Condition(x => x.Id, 999, ConditionOperations.Equal)
			.Or()
			.Condition<Store>("stores2", store => store.Deleted, null, ConditionOperations.Equal) */

		;
	}
}