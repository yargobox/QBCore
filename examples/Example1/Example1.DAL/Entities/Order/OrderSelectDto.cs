using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Stores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
		builder
			.SelectFromTable("orders")
			.LeftJoinTable<Store>("stores")
				.Connect<Store, Order>(store => store.Id, order => order.StoreId, ConditionOperations.Equal)
			.LeftJoinTable<Store>("stores2", "stores")
				.Connect<Store, Order>("stores2", store => store.Id, order => order.StoreId, ConditionOperations.Equal)
			.LeftJoinTable<Store>("stores3", "stores")
				.Connect<Store, Order>("stores3", store => store.Id, order => order.StoreId, ConditionOperations.Equal)

			.Include(sel => sel.Id, doc => doc.Id)
			.Include(sel => sel.Name, doc => doc.Name)
			.Include(sel => sel.StoreId, doc => doc.StoreId)
			.Include(sel => sel.OrderPositions)
			.Include(sel => sel.Total)
			.Include(sel => sel.Created)
			.Include(sel => sel.Updated)
			.Include(sel => sel.Deleted)
			.Include<Store>(sel => sel.StoreName, "stores", store => store.Name)
			.Include<Store>(sel => sel.Store, "stores2", store => store)
			.Include<Store>(sel => sel.Store3!.Name, "stores3", store => store.Name)
		;
	}
}