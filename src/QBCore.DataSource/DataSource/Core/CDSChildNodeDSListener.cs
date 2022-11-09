using QBCore.DataSource.Options;

namespace QBCore.DataSource.Core;

public sealed class CDSChildNodeDSListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> : DataSourceListener<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore> _dataSource = null!;
	IEnumerable<ICDSCondition> _parentNodeConds;

	public CDSChildNodeDSListener(IEnumerable<ICDSCondition> parentNodeConds)
	{
		if (parentNodeConds == null) throw new ArgumentNullException(nameof(parentNodeConds));

		_parentNodeConds = parentNodeConds;
	}

	public override async ValueTask OnAttachAsync(IDataSource dataSource)
	{
		_dataSource = (IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>)dataSource;
		await ValueTask.CompletedTask;
	}
	public override async ValueTask OnDetachAsync(IDataSource dataSource)
	{
		_dataSource = null!;
		await ValueTask.CompletedTask;
	}

	protected override bool OnAboutInsert(TCreate document, DataSourceInsertOptions? options, CancellationToken cancellationToken)
	{
		return base.OnAboutInsert(document, options, cancellationToken);
	}
}