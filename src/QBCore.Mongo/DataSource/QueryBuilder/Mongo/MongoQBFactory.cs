using System.Reflection;
using QBCore.Configuration;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

public class MongoQBFactory : IQueryBuilderFactory
{
	public Type DataSourceConcrete { get; }
	public Type DatabaseContextInterface => typeof(IMongoDbContext);
	public QueryBuilderTypes SupportedQueryBuilders { get; }

	private QBBuilderMethodRefs? _builderMethods;

	private object? _insert;
	private object? _select;
	private object? _update;
	private object? _delete;
	private object? _restore;

	public MongoQBFactory(Type dataSourceConcrete, DataSourceOptions options, QBBuilderMethodRefs? builderMethods, bool lazyBuild)
	{
		DataSourceConcrete = dataSourceConcrete;
		_builderMethods = builderMethods;

		if (options.HasFlag(DataSourceOptions.CanInsert)) SupportedQueryBuilders |= QueryBuilderTypes.Insert;
		if (options.HasFlag(DataSourceOptions.CanSelect)) SupportedQueryBuilders |= QueryBuilderTypes.Select;
		if (options.HasFlag(DataSourceOptions.CanUpdate)) SupportedQueryBuilders |= QueryBuilderTypes.Update;
		if (options.HasFlag(DataSourceOptions.CanDelete | DataSourceOptions.SoftDelete)) SupportedQueryBuilders |= QueryBuilderTypes.SoftDel;
		else if (options.HasFlag(DataSourceOptions.CanDelete)) SupportedQueryBuilders |= QueryBuilderTypes.Delete;
		if (options.HasFlag(DataSourceOptions.CanRestore)) SupportedQueryBuilders |= QueryBuilderTypes.Restore;

		if (!lazyBuild)
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_insert = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Insert) ? false :
				GetType()
					.GetMethod(nameof(MakeInsertFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TCreate)
					.Invoke(this, null);

			_select = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Select) ? false :
				GetType()
					.GetMethod(nameof(MakeSelectFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TSelect)
					.Invoke(this, null);
			
			_update = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Update) ? false :
				GetType()
					.GetMethod(nameof(MakeUpdateFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TUpdate)
					.Invoke(this, null);

			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Delete))
			{
				_delete =
					GetType()
						.GetMethod(nameof(MakeDeleteFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
						.MakeGenericMethod(types.TDocument, types.TDelete)
						.Invoke(this, null);
			}
			else if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.SoftDel))
			{
				_delete =
					GetType()
						.GetMethod(nameof(MakeSoftDelFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
						.MakeGenericMethod(types.TDocument, types.TDelete)
						.Invoke(this, null);
			}
			else
			{
				_delete = false;
			}

			_restore = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Restore) ? false :
				GetType()
					.GetMethod(nameof(MakeRestoreFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TRestore)
					.Invoke(this, null);

			// it's no longer needed
			_builderMethods = null;
		}
	}

	public IInsertQueryBuilder<TDocument, TCreate> CreateQBInsert<TDocument, TCreate>(IDataContext dataContext)
	{
		if (_insert == null)
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_insert = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Insert) ? false :
				GetType()
					.GetMethod(nameof(MakeInsertFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TCreate)
					.Invoke(this, null);
		}

		if (_insert is Func<IDataContext, IInsertQueryBuilder<TDocument, TCreate>> creator)
		{
			return creator(dataContext);
		}
		throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IInsertQueryBuilder<TDocument, TCreate>).ToPretty()}.");
	}

	public ISelectQueryBuilder<TDocument, TSelect> CreateQBSelect<TDocument, TSelect>(IDataContext dataContext)
	{
		if (_select == null)
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_select = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Select) ? false :
				GetType()
					.GetMethod(nameof(MakeSelectFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TSelect)
					.Invoke(this, null);
		}

		if (_select is Func<IDataContext, ISelectQueryBuilder<TDocument, TSelect>> creator)
		{
			return creator(dataContext);
		}
		throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(ISelectQueryBuilder<TDocument, TSelect>).ToPretty()}.");
	}

	public IUpdateQueryBuilder<TDocument, TUpdate> CreateQBUpdate<TDocument, TUpdate>(IDataContext dataContext)
	{
		if (_update == null)
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_update = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Update) ? false :
				GetType()
					.GetMethod(nameof(MakeUpdateFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TUpdate)
					.Invoke(this, null);
		}

		if (_update is Func<IDataContext, IUpdateQueryBuilder<TDocument, TUpdate>> creator)
		{
			return creator(dataContext);
		}
		throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IUpdateQueryBuilder<TDocument, TUpdate>).ToPretty()}.");
	}

	public IDeleteQueryBuilder<TDocument, TDelete> CreateQBDelete<TDocument, TDelete>(IDataContext dataContext)
	{
		if (_delete == null && !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.SoftDel))
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_delete = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Delete) ? false :
				GetType()
					.GetMethod(nameof(MakeDeleteFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TDelete)
					.Invoke(this, null);
		}

		if (_delete is Func<IDataContext, IDeleteQueryBuilder<TDocument, TDelete>> creator)
		{
			return creator(dataContext);
		}
		throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IDeleteQueryBuilder<TDocument, TDelete>).ToPretty()}.");
	}

	public IDeleteQueryBuilder<TDocument, TDelete> CreateQBSoftDel<TDocument, TDelete>(IDataContext dataContext)
	{
		if (_delete == null && !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Delete))
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_delete = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.SoftDel) ? false :
				GetType()
					.GetMethod(nameof(MakeSoftDelFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TDelete)
					.Invoke(this, null);
		}

		if (_delete is Func<IDataContext, IDeleteQueryBuilder<TDocument, TDelete>> creator)
		{
			return creator(dataContext);
		}
		throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IQBSoftDelBuilder<TDocument, TDelete>).ToPretty()}.");
	}

	public IRestoreQueryBuilder<TDocument, TRestore> CreateQBRestore<TDocument, TRestore>(IDataContext dataContext)
	{
		if (_restore == null)
		{
			var types = DataSourceConcrete.GetDataSourceTypes();

			_restore = !SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Restore) ? false :
				GetType()
					.GetMethod(nameof(MakeRestoreFactoryMethod), BindingFlags.Instance | BindingFlags.NonPublic)!
					.MakeGenericMethod(types.TDocument, types.TRestore)
					.Invoke(this, null);
		}

		if (_restore is Func<IDataContext, IRestoreQueryBuilder<TDocument, TRestore>> creator)
		{
			return creator(dataContext);
		}
		throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IRestoreQueryBuilder<TDocument, TRestore>).ToPretty()}.");
	}

	private Func<IDataContext, IInsertQueryBuilder<TDocument, TCreate>> MakeInsertFactoryMethod<TDocument, TCreate>()
	{
		var setupAction =
		(
			_builderMethods?.InsertBuilder != null
				? _builderMethods.InsertBuilder as Action<IQBInsertBuilder<TDocument, TCreate>>
				: FactoryHelper.FindBuilder<IQBInsertBuilder<TDocument, TCreate>>(typeof(TCreate), null)
					?? FactoryHelper.FindBuilder<IQBInsertBuilder<TDocument, TCreate>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBInsertBuilder<TDocument, TCreate>).ToPretty()}.");

		var setup = new QBBuilder<TDocument, TCreate>();
		setupAction(setup);

		return (IDataContext dataContext) => new InsertQueryBuilder<TDocument, TCreate>(new QBBuilder<TDocument, TCreate>(setup), dataContext);
	}
	private Func<IDataContext, ISelectQueryBuilder<TDocument, TSelect>> MakeSelectFactoryMethod<TDocument, TSelect>()
	{
		var setupAction =
		(
			_builderMethods?.SelectBuilder != null
				? _builderMethods.SelectBuilder as Action<IQBMongoSelectBuilder<TDocument, TSelect>>
				: FactoryHelper.FindBuilder<IQBMongoSelectBuilder<TDocument, TSelect>>(typeof(TSelect), null)
					?? FactoryHelper.FindBuilder<IQBMongoSelectBuilder<TDocument, TSelect>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoSelectBuilder<TDocument, TSelect>).ToPretty()}.");

		var setup = new QBBuilder<TDocument, TSelect>();
		setupAction(setup);
		setup.NormalizeSelect();

		return (IDataContext dataContext) => new SelectQueryBuilder<TDocument, TSelect>(new QBBuilder<TDocument, TSelect>(setup), dataContext);
	}
	private Func<IDataContext, IUpdateQueryBuilder<TDocument, TUpdate>> MakeUpdateFactoryMethod<TDocument, TUpdate>()
	{
		var setupAction =
		(
			_builderMethods?.UpdateBuilder != null
				? _builderMethods.UpdateBuilder as Action<IQBUpdateBuilder<TDocument, TUpdate>>
				: FactoryHelper.FindBuilder<IQBUpdateBuilder<TDocument, TUpdate>>(typeof(TUpdate), null)
					?? FactoryHelper.FindBuilder<IQBUpdateBuilder<TDocument, TUpdate>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBUpdateBuilder<TDocument, TUpdate>).ToPretty()}.");

		var setup = new QBBuilder<TDocument, TUpdate>();
		setupAction(setup);

		return (IDataContext dataContext) => new UpdateQueryBuilder<TDocument, TUpdate>(new QBBuilder<TDocument, TUpdate>(setup), dataContext);
	}
	private Func<IDataContext, IDeleteQueryBuilder<TDocument, TDelete>> MakeDeleteFactoryMethod<TDocument, TDelete>()
	{
		var setupAction =
		(
			_builderMethods?.DeleteBuilder != null
				? _builderMethods.DeleteBuilder as Action<IQBDeleteBuilder<TDocument, TDelete>>
				: FactoryHelper.FindBuilder<IQBDeleteBuilder<TDocument, TDelete>>(typeof(TDelete), null)
					?? FactoryHelper.FindBuilder<IQBDeleteBuilder<TDocument, TDelete>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBDeleteBuilder<TDocument, TDelete>).ToPretty()}.");

		var setup = new QBBuilder<TDocument, TDelete>();
		setupAction(setup);

		return (IDataContext dataContext) => new DeleteQueryBuilder<TDocument, TDelete>(new QBBuilder<TDocument, TDelete>(setup), dataContext);
	}
	private Func<IDataContext, IDeleteQueryBuilder<TDocument, TDelete>> MakeSoftDelFactoryMethod<TDocument, TDelete>()
	{
		var setupAction =
		(
			_builderMethods?.SoftDelBuilder != null
				? _builderMethods.SoftDelBuilder as Action<IQBSoftDelBuilder<TDocument, TDelete>>
				: FactoryHelper.FindBuilder<IQBSoftDelBuilder<TDocument, TDelete>>(typeof(TDelete), null)
					?? FactoryHelper.FindBuilder<IQBSoftDelBuilder<TDocument, TDelete>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBSoftDelBuilder<TDocument, TDelete>).ToPretty()}.");

		var setup = new QBBuilder<TDocument, TDelete>();
		setupAction(setup);

		return (IDataContext dataContext) => new SoftDelQueryBuilder<TDocument, TDelete>(new QBBuilder<TDocument, TDelete>(setup), dataContext);
	}
	private Func<IDataContext, IRestoreQueryBuilder<TDocument, TRestore>> MakeRestoreFactoryMethod<TDocument, TRestore>()
	{
		var setupAction =
		(
			_builderMethods?.RestoreBuilder != null
				? _builderMethods.RestoreBuilder as Action<IQBRestoreBuilder<TDocument, TRestore>>
				: FactoryHelper.FindBuilder<IQBRestoreBuilder<TDocument, TRestore>>(typeof(TRestore), null)
					?? FactoryHelper.FindBuilder<IQBRestoreBuilder<TDocument, TRestore>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"Datasource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBRestoreBuilder<TDocument, TRestore>).ToPretty()}.");

		var setup = new QBBuilder<TDocument, TRestore>();
		setupAction(setup);

		return (IDataContext dataContext) => new RestoreQueryBuilder<TDocument, TRestore>(new QBBuilder<TDocument, TRestore>(setup), dataContext);
	}
}