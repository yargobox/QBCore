using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoUpdateBuilder<TDoc, TCreate> : IQBBuilder<TDoc, TCreate>
{
	Expression<Func<TDoc, object?>>? DateUpdateField { get; set; }
	Expression<Func<TDoc, object?>>? DateModifyField { get; set; }
}