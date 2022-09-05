using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IQBMongoInsertBuilder<TDoc, TCreate> : IQBBuilder
{
	Func<IDSIdGenerator>? IdGenerator { get; set; }

	IQBMongoInsertBuilder<TDoc, TCreate> Insert(string? tableName = null);
}

public interface IQBMongoSelectBuilder<TDoc, TSelect> : IQBBuilder
{
	IQBMongoSelectBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null);
	IQBMongoSelectBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null);
	IQBMongoSelectBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null);
	IQBMongoSelectBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null);

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

	IQBMongoSelectBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending);
	IQBMongoSelectBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending);
}

public interface IQBMongoUpdateBuilder<TDoc, TUpdate> : IQBBuilder
{
	IQBMongoUpdateBuilder<TDoc, TUpdate> Update(string? tableName = null);

	IQBMongoUpdateBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoUpdateBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBMongoUpdateBuilder<TDoc, TUpdate> Begin();
	IQBMongoUpdateBuilder<TDoc, TUpdate> End();

	IQBMongoUpdateBuilder<TDoc, TUpdate> And();
	IQBMongoUpdateBuilder<TDoc, TUpdate> Or();
}

public interface IQBMongoSoftDelBuilder<TDoc, TDelete> : IQBBuilder
{
	IQBMongoSoftDelBuilder<TDoc, TDelete> Update(string? tableName = null);

	IQBMongoSoftDelBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoSoftDelBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBMongoSoftDelBuilder<TDoc, TDelete> Begin();
	IQBMongoSoftDelBuilder<TDoc, TDelete> End();

	IQBMongoSoftDelBuilder<TDoc, TDelete> And();
	IQBMongoSoftDelBuilder<TDoc, TDelete> Or();
}

public interface IQBMongoDeleteBuilder<TDoc, TDelete> : IQBBuilder
{
	IQBMongoDeleteBuilder<TDoc, TDelete> Delete(string? tableName = null);

	IQBMongoDeleteBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoDeleteBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBMongoDeleteBuilder<TDoc, TDelete> Begin();
	IQBMongoDeleteBuilder<TDoc, TDelete> End();

	IQBMongoDeleteBuilder<TDoc, TDelete> And();
	IQBMongoDeleteBuilder<TDoc, TDelete> Or();
}

public interface IQBMongoRestoreBuilder<TDoc, TRestore> : IQBBuilder
{
	IQBMongoRestoreBuilder<TDoc, TRestore> Update(string? tableName = null);

	IQBMongoRestoreBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBMongoRestoreBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBMongoRestoreBuilder<TDoc, TRestore> Begin();
	IQBMongoRestoreBuilder<TDoc, TRestore> End();

	IQBMongoRestoreBuilder<TDoc, TRestore> And();
	IQBMongoRestoreBuilder<TDoc, TRestore> Or();
}