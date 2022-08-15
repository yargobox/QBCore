using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoRestoreBuilder<TDoc, TCreate> : IQBBuilder<TDoc, TCreate>
{
}