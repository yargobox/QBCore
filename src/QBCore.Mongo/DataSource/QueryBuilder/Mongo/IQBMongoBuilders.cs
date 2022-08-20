using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoInsertBuilder<TDoc, TCreate> : IQBBuilder
{
	Func<IDSIdGenerator>? IdGenerator { get; set; }

	IQBMongoInsertBuilder<TDoc, TCreate> InsertTo(string tableName);
}

public interface IQBMongoSelectBuilder<TDoc, TSelect> : IQBBuilder
{
	IQBMongoSelectBuilder<TDoc, TSelect> SelectFrom(string tableName);
	IQBMongoSelectBuilder<TDoc, TSelect> SelectFrom(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TSelect> LeftJoin<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TSelect> LeftJoin<TRef>(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TSelect> Join<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TSelect> Join<TRef>(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TSelect> CrossJoin<TRef>(string tableName);
	IQBMongoSelectBuilder<TDoc, TSelect> CrossJoin<TRef>(string alias, string tableName);

	IQBMongoSelectBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IQBMongoSelectBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoSelectBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBMongoSelectBuilder<TDoc, TSelect> Begin();
	IQBMongoSelectBuilder<TDoc, TSelect> End();
	
	IQBMongoSelectBuilder<TDoc, TSelect> And();
	IQBMongoSelectBuilder<TDoc, TSelect> Or();

	IQBMongoSelectBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IQBMongoSelectBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	IQBMongoSelectBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field);
}

public interface IQBMongoUpdateBuilder<TDoc, TUpdate> : IQBBuilder
{
}

public interface IQBMongoSoftDelBuilder<TDoc, TDelete> : IQBBuilder
{
	IQBMongoSoftDelBuilder<TDoc, TDelete> Update(string tableName);
}

public interface IQBMongoDeleteBuilder<TDoc, TDelete> : IQBBuilder
{
	IQBMongoDeleteBuilder<TDoc, TDelete> DeleteFrom(string tableName);
}

public interface IQBMongoRestoreBuilder<TDoc, TRestore> : IQBBuilder
{
	IQBMongoRestoreBuilder<TDoc, TRestore> Update(string tableName);
}