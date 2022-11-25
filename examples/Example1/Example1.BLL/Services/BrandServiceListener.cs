using Example1.DAL.Entities;
using Example1.DAL.Entities.Brands;
using QBCore.DataSource;
using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace Example1.BLL.Services;

internal sealed class BrandServiceListener : DataSourceListener<int?, Brand, BrandCreateDto, BrandSelectDto, BrandUpdateDto, SoftDelDto, SoftDelDto>
{
	BrandService _dataSource = null!;

	public override OKeyName KeyName => throw new NotImplementedException();

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