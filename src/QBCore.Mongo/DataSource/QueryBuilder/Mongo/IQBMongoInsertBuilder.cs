using System.Data;
using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoInsertBuilder<TDoc, TCreate> : IQBBuilder<TDoc, TCreate>
{
	Func<IDSIdGenerator>? CustomIdGenerator { get; set; }

	IQBMongoInsertBuilder<TDoc, TCreate> InsertTo(string tableName);
}