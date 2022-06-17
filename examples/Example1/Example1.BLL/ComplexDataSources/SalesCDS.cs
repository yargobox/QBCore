using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Orders;
using Example1.DAL.Entities.Stores;
using QBCore.DataSource;
using QBCore.DataSource.Builders;

namespace Example1.BLL.Services;

//[ComplexDataSource]
public class SalesCDS : ComplexDataSource<SalesCDS>
{
	private static void CDSBuilder(ICDSBuilder builder)
	{
		builder
			.SetName("sales")

			.NodeBuilder

			.AddNode<StoreService>("stores")

				.AddNode<OrderService>("orders")
					.AddCondition<Order, Store>(order => order.StoreId, store => store.Id, ConditionOperations.Equal)

					.AddNode<OrderPositionService>("positions")
						.AddCondition<OrderPosition, Order>(position => position.Id, order => order.Id, ConditionOperations.Equal);
	}
}