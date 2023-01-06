using System.Linq.Expressions;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public interface IMongoInsertQBBuilder<TDoc, TCreate> : IQBBuilder
{
	Func<IDSIdGenerator>? IdGenerator { get; set; }

	IMongoInsertQBBuilder<TDoc, TCreate> AutoBuild(string? tableName);

	IMongoInsertQBBuilder<TDoc, TCreate> Insert(string? tableName = null);
}

public interface IMongoSelectQBBuilder<TDoc, TSelect> : IQBBuilder
{
	IMongoSelectQBBuilder<TDoc, TSelect> AutoBuild(string? tableName);

	IMongoSelectQBBuilder<TDoc, TSelect> Select(string? tableName = null, string? alias = null);
	IMongoSelectQBBuilder<TDoc, TSelect> LeftJoin<TRef>(string? tableName = null, string? alias = null);
	IMongoSelectQBBuilder<TDoc, TSelect> Join<TRef>(string? tableName = null, string? alias = null);
	IMongoSelectQBBuilder<TDoc, TSelect> CrossJoin<TRef>(string? tableName = null, string? alias = null);

	IMongoSelectQBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Connect<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IMongoSelectQBBuilder<TDoc, TSelect> Connect<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);

	IMongoSelectQBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(Expression<Func<TLocal, object?>> field, Expression<Func<TRef, object?>> refField, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition<TLocal, TRef>(string alias, Expression<Func<TLocal, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, object? constValue, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition<TLocal>(string alias, Expression<Func<TLocal, object?>> field, FO operation, string paramName);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IMongoSelectQBBuilder<TDoc, TSelect> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IMongoSelectQBBuilder<TDoc, TSelect> Begin();
	IMongoSelectQBBuilder<TDoc, TSelect> End();
	
	IMongoSelectQBBuilder<TDoc, TSelect> And();
	IMongoSelectQBBuilder<TDoc, TSelect> Or();

	IMongoSelectQBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, Expression<Func<TRef, object?>> refField);
	IMongoSelectQBBuilder<TDoc, TSelect> Include<TRef>(Expression<Func<TSelect, object?>> field, string refAlias, Expression<Func<TRef, object?>> refField);
	IMongoSelectQBBuilder<TDoc, TSelect> Exclude(Expression<Func<TSelect, object?>> field);
	IMongoSelectQBBuilder<TDoc, TSelect> Optional(Expression<Func<TSelect, object?>> field);

	IMongoSelectQBBuilder<TDoc, TSelect> SortBy(Expression<Func<TSelect, object?>> field, SO sortOrder = SO.Ascending);
	IMongoSelectQBBuilder<TDoc, TSelect> SortBy<TLocal>(string alias, Expression<Func<TLocal, object?>> field, SO sortOrder = SO.Ascending);
}

public interface IMongoUpdateQBBuilder<TDoc, TUpdate> : IQBBuilder
{
	IMongoUpdateQBBuilder<TDoc, TUpdate> AutoBuild(string? tableName);

	IMongoUpdateQBBuilder<TDoc, TUpdate> Update(string? tableName = null);

	IMongoUpdateQBBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IMongoUpdateQBBuilder<TDoc, TUpdate> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IMongoUpdateQBBuilder<TDoc, TUpdate> Begin();
	IMongoUpdateQBBuilder<TDoc, TUpdate> End();

	IMongoUpdateQBBuilder<TDoc, TUpdate> And();
	IMongoUpdateQBBuilder<TDoc, TUpdate> Or();
}

public interface IMongoSoftDelQBBuilder<TDoc, TDelete> : IQBBuilder
{
	IMongoSoftDelQBBuilder<TDoc, TDelete> AutoBuild(string? tableName);

	IMongoSoftDelQBBuilder<TDoc, TDelete> Update(string? tableName = null);

	IMongoSoftDelQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IMongoSoftDelQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IMongoSoftDelQBBuilder<TDoc, TDelete> Begin();
	IMongoSoftDelQBBuilder<TDoc, TDelete> End();

	IMongoSoftDelQBBuilder<TDoc, TDelete> And();
	IMongoSoftDelQBBuilder<TDoc, TDelete> Or();
}

public interface IMongoDeleteQBBuilder<TDoc, TDelete> : IQBBuilder
{
	IMongoDeleteQBBuilder<TDoc, TDelete> AutoBuild(string? tableName);

	IMongoDeleteQBBuilder<TDoc, TDelete> Delete(string? tableName = null);

	IMongoDeleteQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IMongoDeleteQBBuilder<TDoc, TDelete> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IMongoDeleteQBBuilder<TDoc, TDelete> Begin();
	IMongoDeleteQBBuilder<TDoc, TDelete> End();

	IMongoDeleteQBBuilder<TDoc, TDelete> And();
	IMongoDeleteQBBuilder<TDoc, TDelete> Or();
}

public interface IMongoRestoreQBBuilder<TDoc, TRestore> : IQBBuilder
{
	IMongoRestoreQBBuilder<TDoc, TRestore> AutoBuild(string? tableName);

	IMongoRestoreQBBuilder<TDoc, TRestore> Update(string? tableName = null);

	IMongoRestoreQBBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, object? constValue, FO operation);
	IMongoRestoreQBBuilder<TDoc, TRestore> Condition(Expression<Func<TDoc, object?>> field, FO operation, string paramName);

	IMongoRestoreQBBuilder<TDoc, TRestore> Begin();
	IMongoRestoreQBBuilder<TDoc, TRestore> End();

	IMongoRestoreQBBuilder<TDoc, TRestore> And();
	IMongoRestoreQBBuilder<TDoc, TRestore> Or();
}