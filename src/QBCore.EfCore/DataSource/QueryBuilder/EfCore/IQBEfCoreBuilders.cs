using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EfCore;

public interface IQBEfCoreInsertBuilder<TDoc, TCreate> : IQBBuilder where TDoc : class
{
	IQBEfCoreInsertBuilder<TDoc, TCreate> Insert(string? tableName = null);
}

public interface IQBEfCoreSelectBuilder<TDoc, TSelect> : IQBBuilder where TDoc : class
{
	IQBEfCoreSelectBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null);
	IQBEfCoreSelectBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null);
	IQBEfCoreSelectBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null);

	IQBEfCoreSelectBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBEfCoreSelectBuilder<TDoc, TSelect> Begin();
	IQBEfCoreSelectBuilder<TDoc, TSelect> End();
	
	IQBEfCoreSelectBuilder<TDoc, TSelect> And();
	IQBEfCoreSelectBuilder<TDoc, TSelect> Or();

	IQBEfCoreSelectBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	IQBEfCoreSelectBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field);

	IQBEfCoreSelectBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending);
	IQBEfCoreSelectBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending);
}

public interface IQBEfCoreUpdateBuilder<TDoc, TUpdate> : IQBBuilder where TDoc : class
{
	IQBEfCoreUpdateBuilder<TDoc, TUpdate> Update(string? tableName = null);

	IQBEfCoreUpdateBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBEfCoreUpdateBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBEfCoreUpdateBuilder<TDoc, TUpdate> Begin();
	IQBEfCoreUpdateBuilder<TDoc, TUpdate> End();

	IQBEfCoreUpdateBuilder<TDoc, TUpdate> And();
	IQBEfCoreUpdateBuilder<TDoc, TUpdate> Or();
}

public interface IQBEfCoreSoftDelBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	IQBEfCoreSoftDelBuilder<TDoc, TDelete> Update(string? tableName = null);

	IQBEfCoreSoftDelBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBEfCoreSoftDelBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBEfCoreSoftDelBuilder<TDoc, TDelete> Begin();
	IQBEfCoreSoftDelBuilder<TDoc, TDelete> End();

	IQBEfCoreSoftDelBuilder<TDoc, TDelete> And();
	IQBEfCoreSoftDelBuilder<TDoc, TDelete> Or();
}

public interface IQBEfCoreDeleteBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	IQBEfCoreDeleteBuilder<TDoc, TDelete> Delete(string? tableName = null);

	IQBEfCoreDeleteBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBEfCoreDeleteBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBEfCoreDeleteBuilder<TDoc, TDelete> Begin();
	IQBEfCoreDeleteBuilder<TDoc, TDelete> End();

	IQBEfCoreDeleteBuilder<TDoc, TDelete> And();
	IQBEfCoreDeleteBuilder<TDoc, TDelete> Or();
}

public interface IQBEfCoreRestoreBuilder<TDoc, TRestore> : IQBBuilder where TDoc : class
{
	IQBEfCoreRestoreBuilder<TDoc, TRestore> Update(string? tableName = null);

	IQBEfCoreRestoreBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQBEfCoreRestoreBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQBEfCoreRestoreBuilder<TDoc, TRestore> Begin();
	IQBEfCoreRestoreBuilder<TDoc, TRestore> End();

	IQBEfCoreRestoreBuilder<TDoc, TRestore> And();
	IQBEfCoreRestoreBuilder<TDoc, TRestore> Or();
}