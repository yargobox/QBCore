using System.Diagnostics;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;

namespace QBCore.DataSource.QueryBuilder.Mongo;

[DebuggerDisplay("{FullName}")]
internal sealed class MongoFieldPath : FieldPath
{
	public MongoFieldPath(LambdaExpression propertyOrFieldSelector, bool allowPointToSelf)
	: base(propertyOrFieldSelector, allowPointToSelf)
	{ }

	protected override string GetDBSideName(Type declaringType, string propertyOrFieldName)
		=> BsonClassMap.LookupClassMap(declaringType).GetMemberMap(propertyOrFieldName).ElementName;
}