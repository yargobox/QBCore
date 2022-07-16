using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoSelectBuilder<TDoc, TDto> : IQBSelectBuilder<TDoc, TDto>
{
	IQBMongoSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, Expression<Func<TRef, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TDto> Include<TRef>(Expression<Func<TDto, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TDto> Exclude(Expression<Func<TDto, object?>> field);
	IQBMongoSelectBuilder<TDoc, TDto> Optional(Expression<Func<TDto, object?>> field);

	IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> SelectFromTable(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> LeftJoinTable<TRef>(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> JoinTable<TRef>(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TDto> CrossJoinTable<TRef>(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(string alias, Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TDto> Condition(string alias, Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBMongoSelectBuilder<TDoc, TDto> Begin();
	IQBMongoSelectBuilder<TDoc, TDto> End();
	
	IQBMongoSelectBuilder<TDoc, TDto> And();
	IQBMongoSelectBuilder<TDoc, TDto> Or();
}