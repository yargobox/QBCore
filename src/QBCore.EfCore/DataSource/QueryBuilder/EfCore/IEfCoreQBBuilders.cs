using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.EfCore;

public interface IEfCoreInsertQBBuilder<TDoc, TCreate> : IQBBuilder where TDoc : class
{
	IEfCoreInsertQBBuilder<TDoc, TCreate> AutoBuild(string? tableName = null);

	IEfCoreInsertQBBuilder<TDoc, TCreate> Insert(string? tableName = null);
}

public interface IEfCoreSelectQBBuilder<TDoc, TSelect> : IQBBuilder where TDoc : class
{
	IEfCoreSelectQBBuilder<TDoc, TSelect> AutoBuild(string? tableName = null);

	IEfCoreSelectQBBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null);
	IEfCoreSelectQBBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null);
	IEfCoreSelectQBBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null);

	IEfCoreSelectQBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IEfCoreSelectQBBuilder<TDoc, TSelect> Begin();
	IEfCoreSelectQBBuilder<TDoc, TSelect> End();
	
	IEfCoreSelectQBBuilder<TDoc, TSelect> And();
	IEfCoreSelectQBBuilder<TDoc, TSelect> Or();

	IEfCoreSelectQBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	IEfCoreSelectQBBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field);

	IEfCoreSelectQBBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending);
	IEfCoreSelectQBBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending);
}

public interface IEfCoreUpdateQBBuilder<TDoc, TUpdate> : IQBBuilder where TDoc : class
{
	IEfCoreUpdateQBBuilder<TDoc, TUpdate> AutoBuild(string? tableName = null);

	IEfCoreUpdateQBBuilder<TDoc, TUpdate> Update(string? tableName = null);

	IEfCoreUpdateQBBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IEfCoreUpdateQBBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IEfCoreUpdateQBBuilder<TDoc, TUpdate> Begin();
	IEfCoreUpdateQBBuilder<TDoc, TUpdate> End();

	IEfCoreUpdateQBBuilder<TDoc, TUpdate> And();
	IEfCoreUpdateQBBuilder<TDoc, TUpdate> Or();
}

public interface IEfCoreSoftDelQBBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> AutoBuild(string? tableName = null);

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> Update(string? tableName = null);

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> Begin();
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> End();

	IEfCoreSoftDelQBBuilder<TDoc, TDelete> And();
	IEfCoreSoftDelQBBuilder<TDoc, TDelete> Or();
}

public interface IEfCoreDeleteQBBuilder<TDoc, TDelete> : IQBBuilder where TDoc : class
{
	IEfCoreDeleteQBBuilder<TDoc, TDelete> AutoBuild(string? tableName = null);

	IEfCoreDeleteQBBuilder<TDoc, TDelete> Delete(string? tableName = null);

	IEfCoreDeleteQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IEfCoreDeleteQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IEfCoreDeleteQBBuilder<TDoc, TDelete> Begin();
	IEfCoreDeleteQBBuilder<TDoc, TDelete> End();

	IEfCoreDeleteQBBuilder<TDoc, TDelete> And();
	IEfCoreDeleteQBBuilder<TDoc, TDelete> Or();
}

public interface IEfCoreRestoreQBBuilder<TDoc, TRestore> : IQBBuilder where TDoc : class
{
	IEfCoreRestoreQBBuilder<TDoc, TRestore> AutoBuild(string? tableName = null);

	IEfCoreRestoreQBBuilder<TDoc, TRestore> Update(string? tableName = null);

	IEfCoreRestoreQBBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IEfCoreRestoreQBBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IEfCoreRestoreQBBuilder<TDoc, TRestore> Begin();
	IEfCoreRestoreQBBuilder<TDoc, TRestore> End();

	IEfCoreRestoreQBBuilder<TDoc, TRestore> And();
	IEfCoreRestoreQBBuilder<TDoc, TRestore> Or();
}