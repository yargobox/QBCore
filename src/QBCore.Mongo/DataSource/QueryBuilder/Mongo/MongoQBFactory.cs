using System.Reflection;
using QBCore.Configuration;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal class MongoQBFactory : IQueryBuilderFactory
{
	public Type DataSourceConcrete { get; }
	public IDataLayerInfo DataLayer => MongoDataLayer.Default;
	public QueryBuilderTypes SupportedQueryBuilders { get; }

	private Delegate? _insertBuilderMethod;
	private Delegate? _selectBuilderMethod;
	private Delegate? _updateBuilderMethod;
	private Delegate? _deleteBuilderMethod;
	private Delegate? _softDelBuilderMethod;
	private Delegate? _restoreBuilderMethod;

	private object? _insert;
	private object? _select;
	private object? _update;
	private object? _delete;
	private object? _restore;

	public MongoQBFactory(Type dataSourceConcrete, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		DataSourceConcrete = dataSourceConcrete;

		_insertBuilderMethod = insertBuilderMethod;
		_selectBuilderMethod = selectBuilderMethod;
		_updateBuilderMethod = updateBuilderMethod;
		_deleteBuilderMethod = deleteBuilderMethod;
		_softDelBuilderMethod = softDelBuilderMethod;
		_restoreBuilderMethod = restoreBuilderMethod;

		if (options.HasFlag(DataSourceOptions.CanInsert)) SupportedQueryBuilders |= QueryBuilderTypes.Insert;
		if (options.HasFlag(DataSourceOptions.CanSelect)) SupportedQueryBuilders |= QueryBuilderTypes.Select;
		if (options.HasFlag(DataSourceOptions.CanUpdate)) SupportedQueryBuilders |= QueryBuilderTypes.Update;
		if (options.HasFlag(DataSourceOptions.CanDelete | DataSourceOptions.SoftDelete)) SupportedQueryBuilders |= QueryBuilderTypes.SoftDel;
		else if (options.HasFlag(DataSourceOptions.CanDelete)) SupportedQueryBuilders |= QueryBuilderTypes.Delete;
		if (options.HasFlag(DataSourceOptions.CanRestore)) SupportedQueryBuilders |= QueryBuilderTypes.Restore;

		if (!lazyInitialization)
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

			// they're no longer needed
			_insertBuilderMethod = null;
			_selectBuilderMethod = null;
			_updateBuilderMethod = null;
			_deleteBuilderMethod = null;
			_softDelBuilderMethod = null;
			_restoreBuilderMethod = null;
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
		throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IInsertQueryBuilder<TDocument, TCreate>).ToPretty()}.");
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
		throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(ISelectQueryBuilder<TDocument, TSelect>).ToPretty()}.");
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
		throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IUpdateQueryBuilder<TDocument, TUpdate>).ToPretty()}.");
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
		throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IDeleteQueryBuilder<TDocument, TDelete>).ToPretty()}.");
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
		throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IDeleteQueryBuilder<TDocument, TDelete>).ToPretty()}.");
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
		throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder like {typeof(IRestoreQueryBuilder<TDocument, TRestore>).ToPretty()}.");
	}

	private Func<IDataContext, IInsertQueryBuilder<TDocument, TCreate>> MakeInsertFactoryMethod<TDocument, TCreate>()
	{
		var setupAction =
		(
			_insertBuilderMethod != null
				? _insertBuilderMethod as Action<IQBMongoInsertBuilder<TDocument, TCreate>>
				: FactoryHelper.FindBuilder<IQBMongoInsertBuilder<TDocument, TCreate>>(typeof(TCreate), null)
					?? FactoryHelper.FindBuilder<IQBMongoInsertBuilder<TDocument, TCreate>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoInsertBuilder<TDocument, TCreate>).ToPretty()}.");

		var setup = new QBInsertBuilder<TDocument, TCreate>();
		setupAction(setup);
		setup.Normalize();

		return (IDataContext dataContext) => new InsertQueryBuilder<TDocument, TCreate>(new QBInsertBuilder<TDocument, TCreate>(setup), dataContext);
	}
	private Func<IDataContext, ISelectQueryBuilder<TDocument, TSelect>> MakeSelectFactoryMethod<TDocument, TSelect>()
	{
		var setupAction =
		(
			_selectBuilderMethod != null
				? _selectBuilderMethod as Action<IQBMongoSelectBuilder<TDocument, TSelect>>
				: FactoryHelper.FindBuilder<IQBMongoSelectBuilder<TDocument, TSelect>>(typeof(TSelect), null)
					?? FactoryHelper.FindBuilder<IQBMongoSelectBuilder<TDocument, TSelect>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoSelectBuilder<TDocument, TSelect>).ToPretty()}.");

		var setup = new QBSelectBuilder<TDocument, TSelect>();
		setupAction(setup);
		setup.Normalize();

		return (IDataContext dataContext) => new SelectQueryBuilder<TDocument, TSelect>(new QBSelectBuilder<TDocument, TSelect>(setup), dataContext);
	}
	private Func<IDataContext, IUpdateQueryBuilder<TDocument, TUpdate>> MakeUpdateFactoryMethod<TDocument, TUpdate>()
	{
		var setupAction =
		(
			_updateBuilderMethod != null
				? _updateBuilderMethod as Action<IQBMongoUpdateBuilder<TDocument, TUpdate>>
				: FactoryHelper.FindBuilder<IQBMongoUpdateBuilder<TDocument, TUpdate>>(typeof(TUpdate), null)
					?? FactoryHelper.FindBuilder<IQBMongoUpdateBuilder<TDocument, TUpdate>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoUpdateBuilder<TDocument, TUpdate>).ToPretty()}.");

		var setup = new QBUpdateBuilder<TDocument, TUpdate>();
		setupAction(setup);
		setup.Normalize();

		return (IDataContext dataContext) => new UpdateQueryBuilder<TDocument, TUpdate>(new QBUpdateBuilder<TDocument, TUpdate>(setup), dataContext);
	}
	private Func<IDataContext, IDeleteQueryBuilder<TDocument, TDelete>> MakeDeleteFactoryMethod<TDocument, TDelete>()
	{
		var setupAction =
		(
			_deleteBuilderMethod != null
				? _deleteBuilderMethod as Action<IQBMongoDeleteBuilder<TDocument, TDelete>>
				: FactoryHelper.FindBuilder<IQBMongoDeleteBuilder<TDocument, TDelete>>(typeof(TDelete), null)
					?? FactoryHelper.FindBuilder<IQBMongoDeleteBuilder<TDocument, TDelete>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoDeleteBuilder<TDocument, TDelete>).ToPretty()}.");

		var setup = new QBDeleteBuilder<TDocument, TDelete>();
		setupAction(setup);
		setup.Normalize();

		return (IDataContext dataContext) => new DeleteQueryBuilder<TDocument, TDelete>(new QBDeleteBuilder<TDocument, TDelete>(setup), dataContext);
	}
	private Func<IDataContext, IDeleteQueryBuilder<TDocument, TDelete>> MakeSoftDelFactoryMethod<TDocument, TDelete>()
	{
		var setupAction =
		(
			_softDelBuilderMethod != null
				? _softDelBuilderMethod as Action<IQBMongoSoftDelBuilder<TDocument, TDelete>>
				: FactoryHelper.FindBuilder<IQBMongoSoftDelBuilder<TDocument, TDelete>>(typeof(TDelete), null)
					?? FactoryHelper.FindBuilder<IQBMongoSoftDelBuilder<TDocument, TDelete>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoSoftDelBuilder<TDocument, TDelete>).ToPretty()}.");

		var setup = new QBSoftDelBuilder<TDocument, TDelete>();
		setupAction(setup);
		setup.Normalize();

		return (IDataContext dataContext) => new SoftDelQueryBuilder<TDocument, TDelete>(new QBSoftDelBuilder<TDocument, TDelete>(setup), dataContext);
	}
	private Func<IDataContext, IRestoreQueryBuilder<TDocument, TRestore>> MakeRestoreFactoryMethod<TDocument, TRestore>()
	{
		var setupAction =
		(
			_restoreBuilderMethod != null
				? _restoreBuilderMethod as Action<IQBMongoRestoreBuilder<TDocument, TRestore>>
				: FactoryHelper.FindBuilder<IQBMongoRestoreBuilder<TDocument, TRestore>>(typeof(TRestore), null)
					?? FactoryHelper.FindBuilder<IQBMongoRestoreBuilder<TDocument, TRestore>>(DataSourceConcrete, null)
		)
		?? throw new InvalidOperationException($"DataSource {DataSourceConcrete.ToPretty()} does not have a query builder setup {typeof(IQBMongoRestoreBuilder<TDocument, TRestore>).ToPretty()}.");

		var setup = new QBRestoreBuilder<TDocument, TRestore>();
		setupAction(setup);
		setup.Normalize();

		return (IDataContext dataContext) => new RestoreQueryBuilder<TDocument, TRestore>(new QBRestoreBuilder<TDocument, TRestore>(setup), dataContext);
	}
}