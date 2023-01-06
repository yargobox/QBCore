using Example1.DAL.Entities.OrderPositions;
using MongoDB.Bson.Serialization;
using QBCore.DataSource;
using QBCore.DataSource.QueryBuilder.Mongo;

namespace Example1.DAL.Entities.Orders;

public class OrderCreateDto
{
	public string? Name { get; set; }

	public List<OrderPosition>? OrderPositions { get; set; }

	private static void DocumentBuilder(IDSDocumentBuilder builder)
	{
	}

	private static void MongoDocumentBuilder(BsonClassMap<OrderCreateDto> builder)
	{
		builder.AutoMap();
	}

	private static void QBBuilder(IMongoInsertQBBuilder<Order, OrderCreateDto> builder)
	{
		builder.Insert("orders");

		builder.IdGenerator = () => new PesemisticSequentialIdGenerator<Order>(1, 1, 8);
	}
}