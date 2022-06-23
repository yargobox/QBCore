using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Orders;
using Example1.DAL.Entities.Stores;
using QBCore.DataSource;

namespace Example1.BLL.Services;

[ComplexDataSource]
public class SalesCDS : ComplexDataSource<SalesCDS>
{
	static void CDSBuilder(ICDSBuilder builder)
	{
//		builder.Name = "sales";
		builder.NodeBuilder

			.AddNode<StoreService>("stores")

				.AddNode<OrderService>("orders")
					.AddCondition<Order, Store>(order => order.StoreId, store => store.Id, ConditionOperations.Equal)

					.AddNode<OrderPositionService>("positions")
						.AddCondition<OrderPosition, Order>(position => position.Id, order => order.Id, ConditionOperations.Equal);
	}
}