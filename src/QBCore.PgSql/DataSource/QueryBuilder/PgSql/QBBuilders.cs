namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class InsertQBBuilder<TDoc, TCreate> : SqlInsertQBBuilder<TDoc, TCreate> where TDoc : class
{
	public override IDataLayerInfo DataLayer => PgSqlDataLayer.Default;

	public InsertQBBuilder() { }
	public InsertQBBuilder(InsertQBBuilder<TDoc, TCreate> other) : base(other) { }
	public InsertQBBuilder(IQBBuilder other) : base(other) { }
}

internal sealed class SelectQBBuilder<TDoc, TSelect> : SqlSelectQBBuilder<TDoc, TSelect> where TDoc : class
{
	public override IDataLayerInfo DataLayer => PgSqlDataLayer.Default;

	public SelectQBBuilder() { }
	public SelectQBBuilder(SelectQBBuilder<TDoc, TSelect> other) : base(other) { }
	public SelectQBBuilder(IQBBuilder other) : base(other) { }
}

internal sealed class UpdateQBBuilder<TDoc, TUpdate> : SqlUpdateQBBuilder<TDoc, TUpdate> where TDoc : class
{
	public override IDataLayerInfo DataLayer => PgSqlDataLayer.Default;

	public UpdateQBBuilder() { }
	public UpdateQBBuilder(UpdateQBBuilder<TDoc, TUpdate> other) : base(other) { }
	public UpdateQBBuilder(IQBBuilder other) : base(other) { }
}

internal sealed class DeleteQBBuilder<TDoc, TDelete> : SqlDeleteQBBuilder<TDoc, TDelete> where TDoc : class
{
	public override IDataLayerInfo DataLayer => PgSqlDataLayer.Default;

	public DeleteQBBuilder() { }
	public DeleteQBBuilder(DeleteQBBuilder<TDoc, TDelete> other) : base(other) { }
	public DeleteQBBuilder(IQBBuilder other) : base(other) { }
}

internal sealed class SoftDelQBBuilder<TDoc, TDelete> : SqlSoftDelQBBuilder<TDoc, TDelete> where TDoc : class
{
	public override IDataLayerInfo DataLayer => PgSqlDataLayer.Default;

	public SoftDelQBBuilder() { }
	public SoftDelQBBuilder(SoftDelQBBuilder<TDoc, TDelete> other) : base(other) { }
	public SoftDelQBBuilder(IQBBuilder other) : base(other) { }
}

internal sealed class RestoreQBBuilder<TDoc, TRestore> : SqlRestoreQBBuilder<TDoc, TRestore> where TDoc : class
{
	public override IDataLayerInfo DataLayer => PgSqlDataLayer.Default;

	public RestoreQBBuilder() { }
	public RestoreQBBuilder(RestoreQBBuilder<TDoc, TRestore> other) : base(other) { }
	public RestoreQBBuilder(IQBBuilder other) : base(other) { }
}