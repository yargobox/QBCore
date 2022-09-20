using Example1.DAL.Entities.OrderPositions;
using Example1.DAL.Entities.Orders;
using Example1.DAL.Entities.Stores;
using QBCore.DataSource;

namespace Example1.BLL.Services;

[CdsApiController("sales")]
[ComplexDataSource]
public class SalesCDS : ComplexDataSource<SalesCDS>
{
	public SalesCDS(IServiceProvider serviceProvider) : base(serviceProvider) { }

	static void CDSBuilder(ICDSBuilder builder)
	{
//		builder.Name = "Sales";
//		builder.ControllerName = "sales";

		builder.NodeBuilder

			.AddNode<StoreService>("stores")

				.AddNode<OrderService>("orders")
					.AddCondition<Order, Store>(order => order.StoreId, store => store.Id, FO.Equal)

					.AddNode<OrderPositionService>("positions")
						.AddCondition<OrderPosition, Order>(position => position.Id, order => order.Id, FO.Equal);
	}
}