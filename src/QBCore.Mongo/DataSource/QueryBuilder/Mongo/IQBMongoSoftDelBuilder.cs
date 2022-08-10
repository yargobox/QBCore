using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoSoftDelBuilder<TDoc, TCreate> : IQBBuilder<TDoc, TCreate>
{
	Expression<Func<TDoc, object?>>? DateDeleteField { get; set; }
}