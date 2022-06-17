using System.Reflection;
using QBCore.DataSource.QueryBuilder;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract partial class DataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TDataSource>
{
	private static readonly DataSourceDesc _dataSourceDesc;
	protected static readonly Func<IServiceProvider, DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>>? _createNativeListener;
	protected static readonly Func<ITestInsertQueryBuilder<TDocument, TCreate>>? _createTestInsert;
	protected static readonly Func<IInsertQueryBuilder<TDocument, TCreate>>? _createInsert;
	protected static readonly Func<ISelectQueryBuilder<TDocument, TSelect>>? _createSelect;
	protected static readonly Func<ITestUpdateQueryBuilder<TDocument, TUpdate>>? _createTestUpdate;
	protected static readonly Func<IUpdateQueryBuilder<TDocument, TUpdate>>? _createUpdate;
	protected static readonly Func<ITestDeleteQueryBuilder<TDocument, TDelete>>? _createTestDelete;
	protected static readonly Func<IDeleteQueryBuilder<TDocument, TDelete>>? _createDelete;
	protected static readonly Func<ITestRestoreQueryBuilder<TDocument, TDelete>>? _createTestRestore;
	protected static readonly Func<IRestoreQueryBuilder<TDocument, TDelete>>? _createRestore;

	static DataSource()
	{
		_dataSourceDesc = (DataSourceDesc) StaticFactory.DataSources[typeof(TDataSource)];

		if (_dataSourceDesc.DataSourceAttribute.Listener != null)
		{
			Type listenerType = _dataSourceDesc.DataSourceAttribute.Listener;
			var ctor = listenerType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Single();
			var parameters = ctor
				.GetParameters()
				.Select(x => (
					x.ParameterType,
					IsNullable: Nullable.GetUnderlyingType(x.ParameterType) != null
				))
				.ToArray();

			_createNativeListener = DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete> (IServiceProvider provider) =>
			{
				var args = parameters
					.Select(x => x.IsNullable ? provider.GetService(x.ParameterType) : provider.GetRequiredInstance(x.ParameterType))
					.ToArray();

				return (DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete>)ctor.Invoke(args);
			};
		}

		//_createTestInsert = StaticFactory.QueryBuilders.GetInsert<TDocument, TCreate>();
		_createInsert = _dataSourceDesc.Options.HasFlag(DataSourceOptions.CanInsert) ? StaticFactory.QueryBuilders.GetInsert<TDocument, TCreate>() : null;
		_createSelect = _dataSourceDesc.Options.HasFlag(DataSourceOptions.CanSelect) ? StaticFactory.QueryBuilders.TryGetSelect<TDocument, TSelect>() : null;
		//_createTestUpdate = StaticFactory.QueryBuilders.TryGetOFactory<ITestUpdateQueryBuilder<TDocument, TUpdate>>();
		_createUpdate = _dataSourceDesc.Options.HasFlag(DataSourceOptions.CanUpdate) ? StaticFactory.QueryBuilders.TryGetUpdate<TDocument, TUpdate>() : null;
		//_createTestDelete = StaticFactory.QueryBuilders.TryGetOFactory<ITestDeleteQueryBuilder<TDocument, TDelete>>();
		_createDelete = _dataSourceDesc.Options.HasFlag(DataSourceOptions.CanDelete) ? StaticFactory.QueryBuilders.TryGetDelete<TDocument, TDelete>() : null;
		//_createTestRestore = StaticFactory.QueryBuilders.TryGetOFactory<ITestRestoreQueryBuilder<TDocument, TDelete>>();
		//_createRestore = StaticFactory.QueryBuilders.TryGetOFactory<IRestoreQueryBuilder<TDocument, TDelete>>();
	}
}