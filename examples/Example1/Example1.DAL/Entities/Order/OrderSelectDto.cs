using System.ComponentModel.DataAnnotations;
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
	[DeId] public int? Id { get; set; }
	
	[Required, MinLength(4), MaxLength(32)]
	[DeViewName] public string? Name { get; set; }

	[DeForeignId] public int? StoreId { get; set; }

	public string? StoreName { get; set; }
	public StoreSelectDto? Store { get; set; }
	public StoreSelectDto? Store3 { get; set; }

	public List<OrderPositionSelectDto>? OrderPositions { get; set; }

	public decimal? Total { get; set; }

	[DeCreated] public DateTimeOffset? Created { get; set; }
	[DeUpdated] public DateTimeOffset? Updated { get; set; }
	[DeDeleted] public DateTimeOffset? Deleted { get; set; }

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
			.Select("orders")
				//.Optional(sel => sel.Updated)

  			.Join<Store>("stores")
				.Connect<Store, Order>(store => store.Id, order => order.StoreId, FO.Equal)
				.Include<Store>(sel => sel.StoreName, "stores", store => store.Name)

			.LeftJoin<Store>("stores", "stores2")
				.Connect<Store, Order>("stores2", store => store.Id, "orders", order => order.StoreId, FO.Equal)
				//.Connect<Store, Store>("stores2", store => store.Created, "stores", store => store.Deleted, ConditionOperations.NotEqual)
				//.Connect<Store>("stores2", store => store.Updated, null, ConditionOperations.IsNull)
				.Include<Store>(sel => sel.Store, "stores2", store => store)
				//.Exclude(sel => sel.Store!.Created)
				//.Optional(sel => sel.Store!.Updated)

			.LeftJoin<Store>("stores", "stores3")
				.Connect<Store, Store>("stores3", store => store.Id, "stores2", store2 => store2.Id, FO.Equal)
				.Include<Store>(sel => sel.Store3!.Name, "stores3", store => store.Name)

			.Condition<Store, Order>("stores3", store => store.Deleted, "orders", order => order.Deleted, FO.Equal)
			.And()
			.Condition(x => x.Id, 999, FO.Equal)
			.Or()
			.Condition<Store>("stores2", store => store.Deleted, null, FO.Equal)

		;
	}
}