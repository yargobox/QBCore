using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.DataSource;

public abstract class DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> : IDataSourceListener
{
	public abstract OKeyName KeyName { get; }

	public abstract ValueTask OnAttachAsync(IDataSource dataSource);
	public abstract ValueTask OnDetachAsync(IDataSource dataSource);

	protected virtual bool OnGoingInsert(TCreate document, DataSourceInsertOptions? options, CancellationToken cancellationToken)
	{
		return true;
	}
	protected virtual bool OnAboutInsert(TCreate document, DataSourceInsertOptions? options, CancellationToken cancellationToken)
	{
		return true;
	}
	protected virtual bool OnDoInsert(TCreate document, DataSourceInsertOptions? options, CancellationToken cancellationToken)
	{
		return true;
	}
	protected virtual Task<TCreate> OnAfterInsert(Task<TCreate> result, CancellationToken cancellationToken)
	{
		return result;
	}
	protected virtual Task<TCreate> OnDidInsert(Task<TCreate> result, CancellationToken cancellationToken)
	{
		return result;
	}
}