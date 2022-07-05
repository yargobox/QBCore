using System.Linq.Expressions;
using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Stores;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;
using QBCore.Extensions.Linq.Expressions;

namespace Example1.DAL.Entities.Orders;

public class OrderSelectDto
{
	public int? Id { get; set; }
	public string? Name { get; set; }

	public int? StoreId { get; set; }
	public virtual StoreSelectDto? Store { get; set; }

	public List<OrderPositionSelectDto>? OrderPositions { get; set; }

	public decimal? Total { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	static void Builder(IQBMongoSelectBuilder<Order, OrderSelectDto> builder)
	{
		builder
			.SelectFromTable("orders")
				.Include(doc => doc.Id)
				.Include(doc => doc.Name)
				.Include(doc => doc.StoreId)
				.Include(doc => doc.OrderPositions)
				.Include(doc => doc.Total)
				.Include(doc => doc.Created)
				.Include(doc => doc.Updated)
				.Include(doc => doc.Deleted)

			.LeftJoinTable<Store>("stores")
				.Connect<Store, Order>(store => store.Id, order => order.StoreId, ConditionOperations.Equal)
				.Connect<Store>(store => store.Orders, null, ConditionOperations.IsNotNull)
				.Connect<Store>(store => store.Name, ConditionOperations.Like, "@storeName")

				.Include<Store>(doc => doc.Name, dto => dto.Store!.Name)

				/* .Condition(x => x.Id, 1, ConditionOperations.Equal)
				.And()
				.Condition(x => x.Id, 2, ConditionOperations.Equal)
				.Or()
				.Condition(x => x.Id, 3, ConditionOperations.Equal)
				.Or()
				.Condition(x => x.Id, 4, ConditionOperations.Equal) */

				//.Condition<OrderSelectDto>(doc => doc.StoreId, 2, ConditionOperations.Equal)
				//.And()
/* 				.Begin()
					.Begin()
						.Condition(doc => doc.Id, 1, ConditionOperations.Equal)
						.Or()
						.Condition(doc => doc.Id, 2, ConditionOperations.Equal)
					.End()
					.And()
					.Begin()
						.Condition(doc => doc.Id, 3, ConditionOperations.Equal)
						.Or()
						.Condition(doc => doc.Id, 4, ConditionOperations.Equal)
					.End()
				.End()
				.Or()
				.Condition(doc => doc.Id, 5, ConditionOperations.Equal) */

/* 				.Condition(doc => doc.Id, 1, ConditionOperations.Equal)
				.Or()
				.Condition(doc => doc.Id, 2, ConditionOperations.Equal)
				.Or()
				.Condition(doc => doc.Id, 3, ConditionOperations.Equal) */

				// Id = 1 AND ((Id = 2 AND Id = 3) OR Id = 4)
				.Condition(doc => doc.Code, Guid.NewGuid(), ConditionOperations.Equal)
				.And()
				.Condition<Order>(doc => doc.Id, 1, ConditionOperations.Equal)
				.And()
				.Begin()
					.Begin()
						.Condition<Order>(doc => doc.Id, 2, ConditionOperations.Equal)
						.And()
						.Condition<Order>(doc => doc.Id, 3, ConditionOperations.Equal)
					.End()
					.Or()
					.Condition<Order>(doc => doc.Id, 4, ConditionOperations.Equal)
				.End()
				.And()
				// ((Id = 5 AND Id = 6) OR (Id = 7 AND Id = 8)) AND Id = 9
				.Begin()
					.Begin()
						.Condition<Order>(doc => doc.Id, 5, ConditionOperations.Equal)
						.And()
						.Condition<Order>(doc => doc.Id, 6, ConditionOperations.Equal)
					.End()
					.Or()
					.Begin()
						.Condition<Order>(doc => doc.Id, 7, ConditionOperations.Equal)
						.And()
						.Condition<Order>(doc => doc.Id, 8, ConditionOperations.Equal)
					.End()
				.End()
				.And()
				.Condition<Order>(doc => doc.Id, 9, ConditionOperations.Equal)
		;

		/* 		builder
					.SelectFromTable<Order>("t0", "orders")
						.Include<Order>("t0", doc => doc.Id, dto => dto.Id)
						.Include<Order>("t0", doc => doc.Name, dto => dto.Name)
						.Include<Order>("t0", doc => doc.StoreId, dto => dto.StoreId)
						.Include<Order>("t0", doc => doc.OrderPositions, dto => dto.OrderPositions)
						.Include<Order>("t0", doc => doc.Total, dto => dto.Total)
						.Include<Order>("t0", doc => doc.Created, dto => dto.Created)
						.Include<Order>("t0", doc => doc.Updated, dto => dto.Updated)
						.Include<Order>("t0", doc => doc.Deleted, dto => dto.Deleted)

					.LeftJoinTable<Store>("t1", "stores")
						.AddConnect<Store, Order>("t1", store => store.Id, "t0", order => order.StoreId, ConditionOperations.Equal)
						.AddConnect<Store>("t1", store => store.Orders, null, ConditionOperations.IsNotNull)
						.AddConnectParam<Store>("t1", store => store.Name, "@storeName", ConditionOperations.Like)
						.Include<Store>("t1", doc => doc.Name, dto => dto.Store!.Name)

					.AddCondition<Order>("t0", doc => doc.Deleted, null, ConditionOperations.IsNull); */
	}
}