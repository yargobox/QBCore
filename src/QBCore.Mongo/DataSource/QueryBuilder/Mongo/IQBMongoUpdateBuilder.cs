using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoUpdateBuilder<TDoc, TCreate> : IQBBuilder<TDoc, TCreate>
{
}