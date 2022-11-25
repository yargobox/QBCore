using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EntityFramework;

public interface IQbEfInsertBuilder<TDoc, TCreate> : IQBBuilder where TDoc : class
{
	IQbEfInsertBuilder<TDoc, TCreate> Insert(string? tableName = null);
}

public interface IQbEfSelectBuilder<TDoc, TSelect> : IQBBuilder where TDoc : class
{
	IQbEfSelectBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null);
	IQbEfSelectBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null);
	IQbEfSelectBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null);
	IQbEfSelectBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null);

	IQbEfSelectBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQbEfSelectBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IQbEfSelectBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQbEfSelectBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IQbEfSelectBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQbEfSelectBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQbEfSelectBuilder<TDoc, TSelect> Begin();
	IQbEfSelectBuilder<TDoc, TSelect> End();
	
	IQbEfSelectBuilder<TDoc, TSelect> And();
	IQbEfSelectBuilder<TDoc, TSelect> Or();

	IQbEfSelectBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	IQbEfSelectBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IQbEfSelectBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	IQbEfSelectBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field);

	IQbEfSelectBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending);
	IQbEfSelectBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending);
}

public interface IQbEfUpdateBuilder<TDoc, TUpdate> : IQBBuilder where TDoc : class
{
	IQbEfUpdateBuilder<TDoc, TUpdate> Update(string? tableName = null);

	IQbEfUpdateBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQbEfUpdateBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQbEfUpdateBuilder<TDoc, TUpdate> Begin();
	IQbEfUpdateBuilder<TDoc, TUpdate> End();

	IQbEfUpdateBuilder<TDoc, TUpdate> And();
	IQbEfUpdateBuilder<TDoc, TUpdate> Or();
}

public interface IQbEfSoftDelBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	IQbEfSoftDelBuilder<TDoc, TDelete> Update(string? tableName = null);

	IQbEfSoftDelBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQbEfSoftDelBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQbEfSoftDelBuilder<TDoc, TDelete> Begin();
	IQbEfSoftDelBuilder<TDoc, TDelete> End();

	IQbEfSoftDelBuilder<TDoc, TDelete> And();
	IQbEfSoftDelBuilder<TDoc, TDelete> Or();
}

public interface IQbEfDeleteBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	IQbEfDeleteBuilder<TDoc, TDelete> Delete(string? tableName = null);

	IQbEfDeleteBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQbEfDeleteBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQbEfDeleteBuilder<TDoc, TDelete> Begin();
	IQbEfDeleteBuilder<TDoc, TDelete> End();

	IQbEfDeleteBuilder<TDoc, TDelete> And();
	IQbEfDeleteBuilder<TDoc, TDelete> Or();
}

public interface IQbEfRestoreBuilder<TDoc, TRestore> : IQBBuilder where TDoc : class
{
	IQbEfRestoreBuilder<TDoc, TRestore> Update(string? tableName = null);

	IQbEfRestoreBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IQbEfRestoreBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IQbEfRestoreBuilder<TDoc, TRestore> Begin();
	IQbEfRestoreBuilder<TDoc, TRestore> End();

	IQbEfRestoreBuilder<TDoc, TRestore> And();
	IQbEfRestoreBuilder<TDoc, TRestore> Or();
}