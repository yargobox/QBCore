using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoSelectBuilder<TDoc, TDto> : IQBSelectBuilder<TDoc, TDto>
{
	IQBMongoSelectBuilder<TDoc, TDto> Include(Expression<Func<TDto, object?>> field);
	IQBMongoSelectBuilder<TDoc, TDto> Include(Expression<Func<TDto, object?>> field, Expression<Func<TDoc, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, string refName, Expression<Func<TRef, object?>> refField);

	IQBMongoSelectBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field);
	IQBMongoSelectBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field, Expression<Func<TDoc, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TDto> Exclude<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TDto> Exclude<TRef>(Expression<Func<TDto, object?>> field, string refName, Expression<Func<TRef, object?>> refField);

	IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string name, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TRef>(string name, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TRef>(string name, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TRef>(string name, string tableName);

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