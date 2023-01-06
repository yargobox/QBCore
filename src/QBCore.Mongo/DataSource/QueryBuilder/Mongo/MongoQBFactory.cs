using System.Reflection;
using QBCore.Configuration;
using QBCore.Extensions.Internals;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.Mongo;

internal class MongoQBFactory : IQueryBuilderFactory
{
	public IDataLayerInfo DataLayer => MongoDataLayer.Default;
	public DSTypeInfo DSTypeInfo => _dsTypeInfo;
	public QueryBuilderTypes SupportedQueryBuilders => _supportedQueryBuilders;

	public IQBBuilder? DefaultInsertBuilder => GetInsertBuilder();
	public IQBBuilder? DefaultSelectBuilder => GetSelectBuilder();
	public IQBBuilder? DefaultUpdateBuilder => GetUpdateBuilder();
	public IQBBuilder? DefaultDeleteBuilder => GetDeleteBuilder();
	public IQBBuilder? DefaultSoftDelBuilder => GetSoftDelBuilder();
	public IQBBuilder? DefaultRestoreBuilder => GetRestoreBuilder();

	private readonly DSTypeInfo _dsTypeInfo;
	private readonly QueryBuilderTypes _supportedQueryBuilders;

	private readonly Delegate? _insertBuilderMethod;
	private readonly Delegate? _selectBuilderMethod;
	private readonly Delegate? _updateBuilderMethod;
	private readonly Delegate? _deleteBuilderMethod;
	private readonly Delegate? _restoreBuilderMethod;

	private IQBBuilder? _insertBuilder;
	private IQBBuilder? _selectBuilder;
	private IQBBuilder? _updateBuilder;
	private IQBBuilder? _deleteBuilder;
	private IQBBuilder? _softDelBuilder;
	private IQBBuilder? _restoreBuilder;

	private static readonly MethodInfo _getInsertBuilder = typeof(MongoQBFactory)
		.GetMethod(nameof(GetInsertBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getInsertBuilder));
	private static readonly MethodInfo _getSelectBuilder = typeof(MongoQBFactory)
		.GetMethod(nameof(GetSelectBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getSelectBuilder));
	private static readonly MethodInfo _getUpdateBuilder = typeof(MongoQBFactory)
		.GetMethod(nameof(GetUpdateBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getUpdateBuilder));
	private static readonly MethodInfo _getDeleteBuilder = typeof(MongoQBFactory)
		.GetMethod(nameof(GetDeleteBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getDeleteBuilder));
	private static readonly MethodInfo _getSoftDelBuilder = typeof(MongoQBFactory)
		.GetMethod(nameof(GetSoftDelBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getSoftDelBuilder));
	private static readonly MethodInfo _getRestoreBuilder = typeof(MongoQBFactory)
		.GetMethod(nameof(GetRestoreBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getRestoreBuilder));

	public MongoQBFactory(DSTypeInfo dsTypeInfo, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
	{
		_dsTypeInfo = dsTypeInfo;

		if (options.HasFlag(DataSourceOptions.CanInsert))
		{
			_supportedQueryBuilders |= QueryBuilderTypes.Insert;

			if (insertBuilderMethod != null)
			{
				_insertBuilderMethod = insertBuilderMethod;
			}
			else
			{
				var setupActionArgType = typeof(IMongoInsertQBBuilder<,>).MakeGenericType(_dsTypeInfo.TDoc, _dsTypeInfo.TCreate);
				_insertBuilderMethod = FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.Concrete, null)
									?? FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.TCreate, null);
			}
		}

		if (options.HasFlag(DataSourceOptions.CanSelect))
		{
			_supportedQueryBuilders |= QueryBuilderTypes.Select;

			if (selectBuilderMethod != null)
			{
				_selectBuilderMethod = selectBuilderMethod;
			}
			else
			{
				var setupActionArgType = typeof(IMongoSelectQBBuilder<,>).MakeGenericType(_dsTypeInfo.TDoc, _dsTypeInfo.TSelect);
				_selectBuilderMethod = FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.Concrete, null)
									?? FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.TSelect, null);
			}
		}

		if (options.HasFlag(DataSourceOptions.CanUpdate))
		{
			_supportedQueryBuilders |= QueryBuilderTypes.Update;

			if (updateBuilderMethod != null)
			{
				_updateBuilderMethod = updateBuilderMethod;
			}
			else
			{
				var setupActionArgType = typeof(IMongoUpdateQBBuilder<,>).MakeGenericType(_dsTypeInfo.TDoc, _dsTypeInfo.TUpdate);
				_updateBuilderMethod = FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.Concrete, null)
									?? FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.TUpdate, null);
			}
		}

		if (options.HasFlag(DataSourceOptions.SoftDelete))
		{
			if (options.HasFlag(DataSourceOptions.CanDelete))
			{
				_supportedQueryBuilders |= QueryBuilderTypes.SoftDel;

				if (softDelBuilderMethod != null)
				{
					_deleteBuilderMethod = softDelBuilderMethod;
				}
				else
				{
					var setupActionArgType = typeof(IMongoSoftDelQBBuilder<,>).MakeGenericType(_dsTypeInfo.TDoc, _dsTypeInfo.TDelete);
					_deleteBuilderMethod = FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.Concrete, null)
										?? FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.TDelete, null);
				}
			}

			if (options.HasFlag(DataSourceOptions.CanRestore))
			{
				_supportedQueryBuilders |= QueryBuilderTypes.Restore;

				if (restoreBuilderMethod != null)
				{
					_restoreBuilderMethod = restoreBuilderMethod;
				}
				else
				{
					var setupActionArgType = typeof(IMongoRestoreQBBuilder<,>).MakeGenericType(_dsTypeInfo.TDoc, _dsTypeInfo.TRestore);
					_restoreBuilderMethod = FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.Concrete, null)
										?? FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.TRestore, null);
				}
			}
		}
		else if (options.HasFlag(DataSourceOptions.CanDelete))
		{
			_supportedQueryBuilders |= QueryBuilderTypes.Delete;

			if (deleteBuilderMethod != null)
			{
				_deleteBuilderMethod = deleteBuilderMethod;
			}
			else
			{
				var setupActionArgType = typeof(IMongoDeleteQBBuilder<,>).MakeGenericType(_dsTypeInfo.TDoc, _dsTypeInfo.TDelete);
				_deleteBuilderMethod = FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.Concrete, null)
									?? FactoryHelper.FindBuilder(setupActionArgType, _dsTypeInfo.TDelete, null);
			}
		}

		if (!lazyInitialization)
		{
			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Insert)) GetInsertBuilder();
			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Select)) GetSelectBuilder();
			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Update)) GetUpdateBuilder();
			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Delete)) GetDeleteBuilder();
			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.SoftDel)) GetSoftDelBuilder();
			if (SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Restore)) GetRestoreBuilder();
		}
	}

	public IInsertQueryBuilder<TDoc, TCreate> CreateQBInsert<TDoc, TCreate>(IDataContext dataContext) where TDoc : class
	{
		var setup = (InsertQBBuilder<TDoc, TCreate>?)GetInsertBuilder()
			?? throw EX.DataSource.Make.DataSourceDoesNotSupportOperation(_dsTypeInfo.Concrete.ToPretty(), QueryBuilderTypes.Insert.ToString());

		return new InsertQueryBuilder<TDoc, TCreate>(new InsertQBBuilder<TDoc, TCreate>(setup), dataContext);
	}
	public ISelectQueryBuilder<TDoc, TSelect> CreateQBSelect<TDoc, TSelect>(IDataContext dataContext) where TDoc : class
	{
		var setup = (SelectQBBuilder<TDoc, TSelect>?)GetSelectBuilder()
			?? throw EX.DataSource.Make.DataSourceDoesNotSupportOperation(_dsTypeInfo.Concrete.ToPretty(), QueryBuilderTypes.Select.ToString());

		return new SelectQueryBuilder<TDoc, TSelect>(new SelectQBBuilder<TDoc, TSelect>(setup), dataContext);
	}
	public IUpdateQueryBuilder<TDoc, TUpdate> CreateQBUpdate<TDoc, TUpdate>(IDataContext dataContext) where TDoc : class
	{
		var setup = (UpdateQBBuilder<TDoc, TUpdate>?)GetUpdateBuilder()
			?? throw EX.DataSource.Make.DataSourceDoesNotSupportOperation(_dsTypeInfo.Concrete.ToPretty(), QueryBuilderTypes.Update.ToString());

		return new UpdateQueryBuilder<TDoc, TUpdate>(new UpdateQBBuilder<TDoc, TUpdate>(setup), dataContext);
	}
	public IDeleteQueryBuilder<TDoc, TDelete> CreateQBDelete<TDoc, TDelete>(IDataContext dataContext) where TDoc : class
	{
		var setup = (DeleteQBBuilder<TDoc, TDelete>?)GetDeleteBuilder()
			?? throw EX.DataSource.Make.DataSourceDoesNotSupportOperation(_dsTypeInfo.Concrete.ToPretty(), QueryBuilderTypes.Delete.ToString());

		return new DeleteQueryBuilder<TDoc, TDelete>(new DeleteQBBuilder<TDoc, TDelete>(setup), dataContext);
	}
	public IDeleteQueryBuilder<TDoc, TDelete> CreateQBSoftDel<TDoc, TDelete>(IDataContext dataContext) where TDoc : class
	{
		var setup = (SoftDelQBBuilder<TDoc, TDelete>?)GetSoftDelBuilder()
			?? throw EX.DataSource.Make.DataSourceDoesNotSupportOperation(_dsTypeInfo.Concrete.ToPretty(), QueryBuilderTypes.SoftDel.ToString());

		return new SoftDelQueryBuilder<TDoc, TDelete>(new SoftDelQBBuilder<TDoc, TDelete>(setup), dataContext);
	}
	public IRestoreQueryBuilder<TDoc, TRestore> CreateQBRestore<TDoc, TRestore>(IDataContext dataContext) where TDoc : class
	{
		var setup = (RestoreQBBuilder<TDoc, TRestore>?)GetRestoreBuilder()
			?? throw EX.DataSource.Make.DataSourceDoesNotSupportOperation(_dsTypeInfo.Concrete.ToPretty(), QueryBuilderTypes.Restore.ToString());

		return new RestoreQueryBuilder<TDoc, TRestore>(new RestoreQBBuilder<TDoc, TRestore>(setup), dataContext);
	}

	private IQBBuilder? GetInsertBuilder()
		=> (IQBBuilder?) _getInsertBuilder.MakeGenericMethod(_dsTypeInfo.TDoc, _dsTypeInfo.TCreate).Invoke(this, null);
	private IQBBuilder? GetSelectBuilder()
		=> (IQBBuilder?) _getSelectBuilder.MakeGenericMethod(_dsTypeInfo.TDoc, _dsTypeInfo.TSelect).Invoke(this, null);
	private IQBBuilder? GetUpdateBuilder()
		=> (IQBBuilder?) _getUpdateBuilder.MakeGenericMethod(_dsTypeInfo.TDoc, _dsTypeInfo.TUpdate).Invoke(this, null);
	private IQBBuilder? GetDeleteBuilder()
		=> (IQBBuilder?) _getDeleteBuilder.MakeGenericMethod(_dsTypeInfo.TDoc, _dsTypeInfo.TDelete).Invoke(this, null);
	private IQBBuilder? GetSoftDelBuilder()
		=> (IQBBuilder?) _getSoftDelBuilder.MakeGenericMethod(_dsTypeInfo.TDoc, _dsTypeInfo.TDelete).Invoke(this, null);
	private IQBBuilder? GetRestoreBuilder()
		=> (IQBBuilder?) _getRestoreBuilder.MakeGenericMethod(_dsTypeInfo.TDoc, _dsTypeInfo.TRestore).Invoke(this, null);

	private IQBBuilder? GetInsertBuilder<TDoc, TCreate>()
	{
		if (_insertBuilder != null)
		{
			return _insertBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Insert))
		{
			return null;
		}

		InsertQBBuilder<TDoc, TCreate>? setup = null;
		var setupAction = (Action<IMongoInsertQBBuilder<TDoc, TCreate>>?)_insertBuilderMethod;
		if (setupAction != null)
		{
			setup = new InsertQBBuilder<TDoc, TCreate>();
			setupAction(setup);
		}
		else
		{
			var other =
				(_updateBuilderMethod != null ? GetUpdateBuilder() : null) ??
				(_selectBuilderMethod != null ? GetSelectBuilder() : null) ??
				(_deleteBuilderMethod != null ? GetSoftDelBuilder() ?? GetDeleteBuilder() : null) ??
				(_restoreBuilderMethod != null ? GetRestoreBuilder() : null);

			if (other != null)
			{
				setup = new InsertQBBuilder<TDoc, TCreate>(other);
			}
			else
			{
				setup = new InsertQBBuilder<TDoc, TCreate>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _insertBuilder, setup, null);
		return _insertBuilder;
	}
	private IQBBuilder? GetSelectBuilder<TDoc, TSelect>()
	{
		if (_selectBuilder != null)
		{
			return _selectBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Select))
		{
			return null;
		}

		SelectQBBuilder<TDoc, TSelect>? setup = null;
		var setupAction = (Action<IMongoSelectQBBuilder<TDoc, TSelect>>?)_selectBuilderMethod;
		if (setupAction != null)
		{
			setup = new SelectQBBuilder<TDoc, TSelect>();
			setupAction(setup);
		}
		else
		{
			var other =
				(_updateBuilderMethod != null ? GetUpdateBuilder() : null) ??
				(_insertBuilderMethod != null ? GetInsertBuilder() : null) ??
				(_deleteBuilderMethod != null ? GetSoftDelBuilder() ?? GetDeleteBuilder() : null) ??
				(_restoreBuilderMethod != null ? GetRestoreBuilder() : null);

			if (other != null)
			{
				setup = new SelectQBBuilder<TDoc, TSelect>(other);
			}
			else
			{
				setup = new SelectQBBuilder<TDoc, TSelect>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _selectBuilder, setup, null);
		return _selectBuilder;
	}
	private IQBBuilder? GetUpdateBuilder<TDoc, TUpdate>()
	{
		if (_updateBuilder != null)
		{
			return _updateBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Update))
		{
			return null;
		}

		UpdateQBBuilder<TDoc, TUpdate>? setup = null;
		var setupAction = (Action<IMongoUpdateQBBuilder<TDoc, TUpdate>>?)_updateBuilderMethod;
		if (setupAction != null)
		{
			setup = new UpdateQBBuilder<TDoc, TUpdate>();
			setupAction(setup);
		}
		else
		{
			var other =
				(_insertBuilderMethod != null ? GetInsertBuilder() : null) ??
				(_selectBuilderMethod != null ? GetSelectBuilder() : null) ??
				(_deleteBuilderMethod != null ? GetSoftDelBuilder() ?? GetDeleteBuilder() : null) ??
				(_restoreBuilderMethod != null ? GetRestoreBuilder() : null);

			if (other != null)
			{
				setup = new UpdateQBBuilder<TDoc, TUpdate>(other);
			}
			else
			{
				setup = new UpdateQBBuilder<TDoc, TUpdate>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _updateBuilder, setup, null);
		return _updateBuilder;
	}
	private IQBBuilder? GetDeleteBuilder<TDoc, TDelete>()
	{
		if (_deleteBuilder != null)
		{
			return _deleteBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Delete))
		{
			return null;
		}

		DeleteQBBuilder<TDoc, TDelete>? setup = null;
		var setupAction = (Action<IMongoDeleteQBBuilder<TDoc, TDelete>>?)_deleteBuilderMethod;
		if (setupAction != null)
		{
			setup = new DeleteQBBuilder<TDoc, TDelete>();
			setupAction(setup);
		}
		else
		{
			var other =
				(_updateBuilderMethod != null ? GetUpdateBuilder() : null) ??
				(_insertBuilderMethod != null ? GetInsertBuilder() : null) ??
				(_selectBuilderMethod != null ? GetSelectBuilder() : null);

			if (other != null)
			{
				setup = new DeleteQBBuilder<TDoc, TDelete>(other);
			}
			else
			{
				setup = new DeleteQBBuilder<TDoc, TDelete>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _deleteBuilder, setup, null);
		return _deleteBuilder;
	}
	private IQBBuilder? GetSoftDelBuilder<TDoc, TDelete>()
	{
		if (_softDelBuilder != null)
		{
			return _softDelBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.SoftDel))
		{
			return null;
		}

		SoftDelQBBuilder<TDoc, TDelete>? setup = null;
		var setupAction = (Action<IMongoSoftDelQBBuilder<TDoc, TDelete>>?)_deleteBuilderMethod;
		if (setupAction != null)
		{
			setup = new SoftDelQBBuilder<TDoc, TDelete>();
			setupAction(setup);
		}
		else
		{
			var other =
				(_restoreBuilderMethod != null ? GetRestoreBuilder() : null) ??
				(_updateBuilderMethod != null ? GetUpdateBuilder() : null) ??
				(_insertBuilderMethod != null ? GetInsertBuilder() : null) ??
				(_selectBuilderMethod != null ? GetSelectBuilder() : null);

			if (other != null)
			{
				setup = new SoftDelQBBuilder<TDoc, TDelete>(other);
			}
			else
			{
				setup = new SoftDelQBBuilder<TDoc, TDelete>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _softDelBuilder, setup, null);
		return _softDelBuilder;
	}
	private IQBBuilder? GetRestoreBuilder<TDoc, TRestore>()
	{
		if (_restoreBuilder != null)
		{
			return _restoreBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Restore))
		{
			return null;
		}

		RestoreQBBuilder<TDoc, TRestore>? setup = null;
		var setupAction = (Action<IMongoRestoreQBBuilder<TDoc, TRestore>>?)_restoreBuilderMethod;
		if (setupAction != null)
		{
			setup = new RestoreQBBuilder<TDoc, TRestore>();
			setupAction(setup);
		}
		else
		{
			var other =
				(_deleteBuilderMethod != null ? GetSoftDelBuilder() : null) ??
				(_updateBuilderMethod != null ? GetUpdateBuilder() : null) ??
				(_insertBuilderMethod != null ? GetInsertBuilder() : null) ??
				(_selectBuilderMethod != null ? GetSelectBuilder() : null);

			if (other != null)
			{
				setup = new RestoreQBBuilder<TDoc, TRestore>(other);
			}
			else
			{
				setup = new RestoreQBBuilder<TDoc, TRestore>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _restoreBuilder, setup, null);
		return _restoreBuilder;
	}
}