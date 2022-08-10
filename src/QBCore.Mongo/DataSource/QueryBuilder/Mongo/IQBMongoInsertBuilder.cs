using System.Data;
using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoInsertBuilder<TDoc, TCreate> : IQBBuilder<TDoc, TCreate>
{
	Expression<Func<TDoc, object?>>? DateCreateField { get; set; }
	Expression<Func<TDoc, object?>>? DateModifyField { get; set; }

	Func<IDSIdGenerator>? CustomIdGenerator { get; set; }

	IQBMongoInsertBuilder<TDoc, TCreate> InsertTo(string tableName);
	IQBMongoInsertBuilder<TDoc, TCreate> ExecProcedure(string tableName);
	IQBMongoInsertBuilder<TDoc, TCreate> AutoBindParameters();
	IQBMongoInsertBuilder<TDoc, TCreate> BindParameter(Expression<Func<TCreate, object?>> field, ParameterDirection direction, bool isErrorCode = false);
	IQBMongoInsertBuilder<TDoc, TCreate> BindParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false);
	IQBMongoInsertBuilder<TDoc, TCreate> BindReturnValueToErrorCode();
	IQBMongoInsertBuilder<TDoc, TCreate> BindParameterToErrorMessage(string name);
}