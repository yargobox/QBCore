using System.Reflection;
using QBCore.Configuration;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder.EntityFramework;

internal class EfQBFactory : IQueryBuilderFactory
{
	public IDataLayerInfo DataLayer => EfDataLayer.Default;
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

	private static readonly MethodInfo _getInsertBuilder = typeof(EfQBFactory)
		.GetMethod(nameof(GetInsertBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getInsertBuilder));
	private static readonly MethodInfo _getSelectBuilder = typeof(EfQBFactory)
		.GetMethod(nameof(GetSelectBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getSelectBuilder));
	private static readonly MethodInfo _getUpdateBuilder = typeof(EfQBFactory)
		.GetMethod(nameof(GetUpdateBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getUpdateBuilder));
	private static readonly MethodInfo _getDeleteBuilder = typeof(EfQBFactory)
		.GetMethod(nameof(GetDeleteBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getDeleteBuilder));
	private static readonly MethodInfo _getSoftDelBuilder = typeof(EfQBFactory)
		.GetMethod(nameof(GetSoftDelBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getSoftDelBuilder));
	private static readonly MethodInfo _getRestoreBuilder = typeof(EfQBFactory)
		.GetMethod(nameof(GetRestoreBuilder), 2, BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard, Array.Empty<Type>(), null)
			?? throw new ArgumentNullException(nameof(_getRestoreBuilder));

	public EfQBFactory(DSTypeInfo dsTypeInfo, DataSourceOptions options, Delegate? insertBuilderMethod, Delegate? selectBuilderMethod, Delegate? updateBuilderMethod, Delegate? deleteBuilderMethod, Delegate? softDelBuilderMethod, Delegate? restoreBuilderMethod, bool lazyInitialization)
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
				var setupActionArgType = typeof(IQbEfInsertBuilder<,>).MakeGenericType(_dsTypeInfo.TDocument, _dsTypeInfo.TCreate);
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
				var setupActionArgType = typeof(IQbEfSelectBuilder<,>).MakeGenericType(_dsTypeInfo.TDocument, _dsTypeInfo.TSelect);
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
				var setupActionArgType = typeof(IQbEfUpdateBuilder<,>).MakeGenericType(_dsTypeInfo.TDocument, _dsTypeInfo.TUpdate);
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
					var setupActionArgType = typeof(IQbEfSoftDelBuilder<,>).MakeGenericType(_dsTypeInfo.TDocument, _dsTypeInfo.TDelete);
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
					var setupActionArgType = typeof(IQbEfRestoreBuilder<,>).MakeGenericType(_dsTypeInfo.TDocument, _dsTypeInfo.TRestore);
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
				var setupActionArgType = typeof(IQbEfDeleteBuilder<,>).MakeGenericType(_dsTypeInfo.TDocument, _dsTypeInfo.TDelete);
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

	public IInsertQueryBuilder<TDocument, TCreate> CreateQBInsert<TDocument, TCreate>(IDataContext dataContext) where TDocument : class
	{
		var setup = (QbInsertBuilder<TDocument, TCreate>?)GetInsertBuilder()
			?? throw new NotSupportedException($"DataSource '{_dsTypeInfo.Concrete.ToPretty()}' does not support the insert operation.");

		return new InsertQueryBuilder<TDocument, TCreate>(new QbInsertBuilder<TDocument, TCreate>(setup), dataContext);
	}
	public ISelectQueryBuilder<TDocument, TSelect> CreateQBSelect<TDocument, TSelect>(IDataContext dataContext) where TDocument : class
	{
		var setup = (QbSelectBuilder<TDocument, TSelect>?)GetSelectBuilder()
			?? throw new NotSupportedException($"DataSource '{_dsTypeInfo.Concrete.ToPretty()}' does not support the select operation.");

		return new SelectQueryBuilder<TDocument, TSelect>(new QbSelectBuilder<TDocument, TSelect>(setup), dataContext);
	}
	public IUpdateQueryBuilder<TDocument, TUpdate> CreateQBUpdate<TDocument, TUpdate>(IDataContext dataContext) where TDocument : class
	{
		var setup = (QbUpdateBuilder<TDocument, TUpdate>?)GetUpdateBuilder()
			?? throw new NotSupportedException($"DataSource '{_dsTypeInfo.Concrete.ToPretty()}' does not support the update operation.");

		return new UpdateQueryBuilder<TDocument, TUpdate>(new QbUpdateBuilder<TDocument, TUpdate>(setup), dataContext);
	}
	public IDeleteQueryBuilder<TDocument, TDelete> CreateQBDelete<TDocument, TDelete>(IDataContext dataContext) where TDocument : class
	{
		var setup = (QbDeleteBuilder<TDocument, TDelete>?)GetDeleteBuilder()
			?? throw new NotSupportedException($"DataSource '{_dsTypeInfo.Concrete.ToPretty()}' does not support the delete operation.");

		return new DeleteQueryBuilder<TDocument, TDelete>(new QbDeleteBuilder<TDocument, TDelete>(setup), dataContext);
	}
	public IDeleteQueryBuilder<TDocument, TDelete> CreateQBSoftDel<TDocument, TDelete>(IDataContext dataContext) where TDocument : class
	{
		var setup = (QbSoftDelBuilder<TDocument, TDelete>?)GetSoftDelBuilder()
			?? throw new NotSupportedException($"DataSource '{_dsTypeInfo.Concrete.ToPretty()}' does not support the soft delete operation.");

		return new SoftDelQueryBuilder<TDocument, TDelete>(new QbSoftDelBuilder<TDocument, TDelete>(setup), dataContext);
	}
	public IRestoreQueryBuilder<TDocument, TRestore> CreateQBRestore<TDocument, TRestore>(IDataContext dataContext) where TDocument : class
	{
		var setup = (QbRestoreBuilder<TDocument, TRestore>?)GetRestoreBuilder()
			?? throw new NotSupportedException($"DataSource '{_dsTypeInfo.Concrete.ToPretty()}' does not support the restore operation.");

		return new RestoreQueryBuilder<TDocument, TRestore>(new QbRestoreBuilder<TDocument, TRestore>(setup), dataContext);
	}

	private IQBBuilder? GetInsertBuilder()
		=> (IQBBuilder?) _getInsertBuilder.MakeGenericMethod(_dsTypeInfo.TDocument, _dsTypeInfo.TCreate).Invoke(this, null);
	private IQBBuilder? GetSelectBuilder()
		=> (IQBBuilder?) _getSelectBuilder.MakeGenericMethod(_dsTypeInfo.TDocument, _dsTypeInfo.TSelect).Invoke(this, null);
	private IQBBuilder? GetUpdateBuilder()
		=> (IQBBuilder?) _getUpdateBuilder.MakeGenericMethod(_dsTypeInfo.TDocument, _dsTypeInfo.TUpdate).Invoke(this, null);
	private IQBBuilder? GetDeleteBuilder()
		=> (IQBBuilder?) _getDeleteBuilder.MakeGenericMethod(_dsTypeInfo.TDocument, _dsTypeInfo.TDelete).Invoke(this, null);
	private IQBBuilder? GetSoftDelBuilder()
		=> (IQBBuilder?) _getSoftDelBuilder.MakeGenericMethod(_dsTypeInfo.TDocument, _dsTypeInfo.TDelete).Invoke(this, null);
	private IQBBuilder? GetRestoreBuilder()
		=> (IQBBuilder?) _getRestoreBuilder.MakeGenericMethod(_dsTypeInfo.TDocument, _dsTypeInfo.TRestore).Invoke(this, null);

	private IQBBuilder? GetInsertBuilder<TDocument, TCreate>() where TDocument : class
	{
		if (_insertBuilder != null)
		{
			return _insertBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Insert))
		{
			return null;
		}

		QbInsertBuilder<TDocument, TCreate>? setup = null;
		var setupAction = (Action<IQbEfInsertBuilder<TDocument, TCreate>>?)_insertBuilderMethod;
		if (setupAction != null)
		{
			setup = new QbInsertBuilder<TDocument, TCreate>();
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
				setup = new QbInsertBuilder<TDocument, TCreate>(other);
			}
			else
			{
				setup = new QbInsertBuilder<TDocument, TCreate>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _insertBuilder, setup, null);
		return _insertBuilder;
	}
	private IQBBuilder? GetSelectBuilder<TDocument, TSelect>() where TDocument : class
	{
		if (_selectBuilder != null)
		{
			return _selectBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Select))
		{
			return null;
		}

		QbSelectBuilder<TDocument, TSelect>? setup = null;
		var setupAction = (Action<IQbEfSelectBuilder<TDocument, TSelect>>?)_selectBuilderMethod;
		if (setupAction != null)
		{
			setup = new QbSelectBuilder<TDocument, TSelect>();
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
				setup = new QbSelectBuilder<TDocument, TSelect>(other);
			}
			else
			{
				setup = new QbSelectBuilder<TDocument, TSelect>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _selectBuilder, setup, null);
		return _selectBuilder;
	}
	private IQBBuilder? GetUpdateBuilder<TDocument, TUpdate>() where TDocument : class
	{
		if (_updateBuilder != null)
		{
			return _updateBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Update))
		{
			return null;
		}

		QbUpdateBuilder<TDocument, TUpdate>? setup = null;
		var setupAction = (Action<IQbEfUpdateBuilder<TDocument, TUpdate>>?)_updateBuilderMethod;
		if (setupAction != null)
		{
			setup = new QbUpdateBuilder<TDocument, TUpdate>();
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
				setup = new QbUpdateBuilder<TDocument, TUpdate>(other);
			}
			else
			{
				setup = new QbUpdateBuilder<TDocument, TUpdate>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _updateBuilder, setup, null);
		return _updateBuilder;
	}
	private IQBBuilder? GetDeleteBuilder<TDocument, TDelete>() where TDocument : class
	{
		if (_deleteBuilder != null)
		{
			return _deleteBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Delete))
		{
			return null;
		}

		QbDeleteBuilder<TDocument, TDelete>? setup = null;
		var setupAction = (Action<IQbEfDeleteBuilder<TDocument, TDelete>>?)_deleteBuilderMethod;
		if (setupAction != null)
		{
			setup = new QbDeleteBuilder<TDocument, TDelete>();
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
				setup = new QbDeleteBuilder<TDocument, TDelete>(other);
			}
			else
			{
				setup = new QbDeleteBuilder<TDocument, TDelete>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _deleteBuilder, setup, null);
		return _deleteBuilder;
	}
	private IQBBuilder? GetSoftDelBuilder<TDocument, TDelete>() where TDocument : class
	{
		if (_softDelBuilder != null)
		{
			return _softDelBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.SoftDel))
		{
			return null;
		}

		QbSoftDelBuilder<TDocument, TDelete>? setup = null;
		var setupAction = (Action<IQbEfSoftDelBuilder<TDocument, TDelete>>?)_deleteBuilderMethod;
		if (setupAction != null)
		{
			setup = new QbSoftDelBuilder<TDocument, TDelete>();
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
				setup = new QbSoftDelBuilder<TDocument, TDelete>(other);
			}
			else
			{
				setup = new QbSoftDelBuilder<TDocument, TDelete>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _softDelBuilder, setup, null);
		return _softDelBuilder;
	}
	private IQBBuilder? GetRestoreBuilder<TDocument, TRestore>() where TDocument : class
	{
		if (_restoreBuilder != null)
		{
			return _restoreBuilder;
		}
		if (!SupportedQueryBuilders.HasFlag(QueryBuilderTypes.Restore))
		{
			return null;
		}

		QbRestoreBuilder<TDocument, TRestore>? setup = null;
		var setupAction = (Action<IQbEfRestoreBuilder<TDocument, TRestore>>?)_restoreBuilderMethod;
		if (setupAction != null)
		{
			setup = new QbRestoreBuilder<TDocument, TRestore>();
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
				setup = new QbRestoreBuilder<TDocument, TRestore>(other);
			}
			else
			{
				setup = new QbRestoreBuilder<TDocument, TRestore>();
				setup.AutoBuild();
			}
		}

		setup.Normalize();
		Interlocked.CompareExchange(ref _restoreBuilder, setup, null);
		return _restoreBuilder;
	}
}