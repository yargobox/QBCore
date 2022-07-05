using MongoDB.Bson;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal sealed class BuiltCondition
{
	public BsonDocument BsonDocument { get; private set; }

	public BuiltCondition(BsonDocument builtCondition)
	{
		BsonDocument = new BsonDocument();
		BsonDocument.AddRange(builtCondition);
	}

	public void AppendByAnd(BuiltCondition other)
	{
		BsonElement elem;
		if (BsonDocument.ElementCount == 1 && BsonDocument.TryGetElement("$and", out elem))
		{
			elem.Value.AsBsonArray.Add(other.BsonDocument);
		}
		else if (BsonDocument.Names.Any(x => other.BsonDocument.Contains(x)))
		{
			var array = new BsonArray { BsonDocument, other.BsonDocument };
			BsonDocument = new BsonDocument() { { "$and", array } };
		}
		else
		{
			BsonDocument.AddRange(other.BsonDocument);
		}
	}

	public void AppendByOr(BuiltCondition other)
	{
		BsonElement elem;
		if (BsonDocument.ElementCount == 1 && BsonDocument.TryGetElement("$or", out elem))
		{
			elem.Value.AsBsonArray.Add(other.BsonDocument);
		}
		else
		{
			var array = new BsonArray { BsonDocument, other.BsonDocument };
			BsonDocument = new BsonDocument() { { "$or", array } };
		}
	}
}