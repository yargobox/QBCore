using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder;

public interface ISqlInsertQBBuilder<TDoc, TCreate> : IQBBuilder where TDoc : class
{
	ISqlInsertQBBuilder<TDoc, TCreate> Insert(string? tableName = null);
}

public interface ISqlSelectQBBuilder<TDoc, TSelect> : IQBBuilder where TDoc : class
{
	ISqlSelectQBBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null);
	ISqlSelectQBBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null);
	ISqlSelectQBBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null);
	ISqlSelectQBBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null);

	ISqlSelectQBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	ISqlSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	ISqlSelectQBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	ISqlSelectQBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	ISqlSelectQBBuilder<TDoc, TSelect> Begin();
	ISqlSelectQBBuilder<TDoc, TSelect> End();
	
	ISqlSelectQBBuilder<TDoc, TSelect> And();
	ISqlSelectQBBuilder<TDoc, TSelect> Or();

	ISqlSelectQBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	ISqlSelectQBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	ISqlSelectQBBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	ISqlSelectQBBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field);

	ISqlSelectQBBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending);
	ISqlSelectQBBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending);
}

public interface ISqlUpdateQBBuilder<TDoc, TUpdate> : IQBBuilder where TDoc : class
{
	ISqlUpdateQBBuilder<TDoc, TUpdate> Update(string? tableName = null);

	ISqlUpdateQBBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	ISqlUpdateQBBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	ISqlUpdateQBBuilder<TDoc, TUpdate> Begin();
	ISqlUpdateQBBuilder<TDoc, TUpdate> End();

	ISqlUpdateQBBuilder<TDoc, TUpdate> And();
	ISqlUpdateQBBuilder<TDoc, TUpdate> Or();
}

public interface ISqlSoftDelQBBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	ISqlSoftDelQBBuilder<TDoc, TDelete> Update(string? tableName = null);

	ISqlSoftDelQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	ISqlSoftDelQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	ISqlSoftDelQBBuilder<TDoc, TDelete> Begin();
	ISqlSoftDelQBBuilder<TDoc, TDelete> End();

	ISqlSoftDelQBBuilder<TDoc, TDelete> And();
	ISqlSoftDelQBBuilder<TDoc, TDelete> Or();
}

public interface ISqlDeleteQBBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	ISqlDeleteQBBuilder<TDoc, TDelete> Delete(string? tableName = null);

	ISqlDeleteQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	ISqlDeleteQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	ISqlDeleteQBBuilder<TDoc, TDelete> Begin();
	ISqlDeleteQBBuilder<TDoc, TDelete> End();

	ISqlDeleteQBBuilder<TDoc, TDelete> And();
	ISqlDeleteQBBuilder<TDoc, TDelete> Or();
}

public interface ISqlRestoreQBBuilder<TDoc, TRestore> : IQBBuilder where TDoc : class
{
	ISqlRestoreQBBuilder<TDoc, TRestore> Update(string? tableName = null);

	ISqlRestoreQBBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	ISqlRestoreQBBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	ISqlRestoreQBBuilder<TDoc, TRestore> Begin();
	ISqlRestoreQBBuilder<TDoc, TRestore> End();

	ISqlRestoreQBBuilder<TDoc, TRestore> And();
	ISqlRestoreQBBuilder<TDoc, TRestore> Or();
}