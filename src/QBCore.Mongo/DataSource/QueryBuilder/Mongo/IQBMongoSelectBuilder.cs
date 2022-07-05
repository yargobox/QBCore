using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoSelectBuilder<TDoc, TDto> : IQBSelectBuilder<TDoc, TDto>
{
	IQBMongoSelectBuilder<TDoc, TDto> Include(Expression<Func<TDoc, object?>> docNavPath);
	IQBMongoSelectBuilder<TDoc, TDto> Include(Expression<Func<TDoc, object?>> docNavPath, Expression<Func<TDto, object?>> dtoNavPath);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(Expression<Func<TOther, object?>> docNavPath);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(string otherName, Expression<Func<TOther, object?>> docNavPath);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(Expression<Func<TOther, object?>> docNavPath, Expression<Func<TDto, object?>> dtoNavPath);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TOther>(string otherName, Expression<Func<TOther, object?>> docNavPath, Expression<Func<TDto, object?>> dtoNavPath);

	IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string name, string tableName, string? conditionTemplate = null);

	IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TOther>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TOther>(string name, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TOther>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TOther>(string name, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TOther>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TOther>(string name, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string name, Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string name, Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName);

	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string name, Expression<Func<TLocal, object?>> field, string refName, Expression<Func<TRef, object?>> refField, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string name, Expression<Func<TLocal, object?>> field, object? constValue, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string name, Expression<Func<TLocal, object?>> field, ConditionOperations operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(string name, Expression<Func<TDoc, object?>> field, object? constValue, ConditionOperations operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, ConditionOperations operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(string name, Expression<Func<TDoc, object?>> field, ConditionOperations operation, string paramName);

	IQBMongoSelectBuilder<TDoc, TDto> Begin();
	IQBMongoSelectBuilder<TDoc, TDto> End();
	
	IQBMongoSelectBuilder<TDoc, TDto> And();
	IQBMongoSelectBuilder<TDoc, TDto> Or();
}