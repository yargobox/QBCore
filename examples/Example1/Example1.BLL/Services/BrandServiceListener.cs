using System.Threading;
using Example1.DAL.Entities.Brands;
using QBCore.DataSource;

namespace Example1.BLL.Services;

internal sealed class BrandServiceListener : DataSourceListener<int?, Brand, BrandCreateDto, BrandSelectDto, BrandUpdateDto, EmptyDto>
{
	BrandService _dataSource = null!;

	public override async ValueTask OnAttachAsync(IDataSource dataSource)
	{
		_dataSource = (BrandService) dataSource;
		await ValueTask.CompletedTask;
	}
	public override async ValueTask OnDetachAsync(IDataSource dataSource)
	{
		_dataSource = null!;
		await ValueTask.CompletedTask;
	}

	protected override bool OnAboutInsert(BrandCreateDto document, DataSourceInsertOptions? options, CancellationToken cancellationToken)
	{
		return base.OnAboutInsert(document, options, cancellationToken);
	}
}