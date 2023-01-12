using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QBCore.Controllers.Helpers;
using QBCore.Controllers.Models;
using QBCore.DataSource;
using QBCore.DataSource.Options;
using QBCore.ObjectFactory;

namespace QBCore.Controllers;

[Produces("application/json")]
public class DataSourceController<TKey, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource> : ControllerBase
	where TDataSource : IDataSource<TKey, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	protected readonly IServiceProvider _serviceProvider;
	protected readonly IDSRequestContext _requestContext;
	protected readonly ILogger<TDataSource> _logger;
	protected TDataSource _ds => (TDataSource)_requestContext.Request.DS;
	protected IComplexDataSource? _cds => _requestContext.Request.CDS;

	public DataSourceController(IServiceProvider serviceProvider, IDSRequestContext requestContext, ILogger<TDataSource> logger)
	{
		_serviceProvider = serviceProvider;
		_requestContext = requestContext;
		_logger = logger;
	}

	[HttpGet, ActionName("index")]
	//[ProducesResponseType(typeof(DataSourceResponse<TSelect>), StatusCodes.Status200OK)]
	public async Task<ActionResult<DataSourceResponse<TSelect>>> IndexAsync(
		[FromQuery, MaxLength(7)] string? mode,
		[FromQuery, MaxLength(8192)] string? filter,
		[FromQuery, MaxLength(2048)] string? sort,
		[FromQuery, Range(1, int.MaxValue)] int? psize,
		[FromQuery, Range(1, int.MaxValue)] int? pnum)
	{
		_requestContext.Request = new DSWebRequest(_serviceProvider, ControllerContext);

		long skip = 0;
		if (psize != null)
		{
			if (pnum == null)
			{
				throw new ArgumentNullException(nameof(pnum));
			}
			skip = (long)psize.Value * (pnum.Value - 1);
		}
		else if (pnum != null)
		{
			throw new ArgumentNullException(nameof(psize));
		}

		var softDelMode = SoftDelHelper.Parse(mode);
		var filterConditions = FOHelper.SerializeFromString<TSelect>(filter, _ds.DSInfo.QBFactory.DataLayer);
		var sortOrders = SOHelper.SerializeFromString<TSelect>(sort, _ds.DSInfo.QBFactory.DataLayer);

		var dataSourceResponse = new DataSourceResponse<TSelect>
		{
			PageSize = psize ?? -1,
			PageNumber = pnum ?? -1
		};
		var options = new DataSourceSelectOptions
		{
			//ObtainLastPageMark = true,
			ObtainTotalCount = true,
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		dataSourceResponse.Data =
			await ( await _ds.SelectAsync(softDelMode, filterConditions, sortOrders, null, skip, psize ?? -1, options) )
				//.GetLastPageMark(isLastPage => dataSourceResponse.IsLastPage = isLastPage ? 1 : 0)
				.GetTotalCount(totalCount => dataSourceResponse.TotalCount = totalCount)
				.ToListAsync();

		return Ok(dataSourceResponse);

		/* 		var p = Request.Query["brand_id"].First();
				var area = ControllerContext.ActionDescriptor.RouteValues["area"];
				var actionName = ControllerContext.ActionDescriptor.ActionName;
				var controllerName = ControllerContext.ActionDescriptor.ControllerName; */
/* 		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(Ok(Enumerable.Empty<TSelect>())); */
	}

	[HttpGet("{id}"), ActionName("get")]
	public async Task<ActionResult<TSelect?>> GetAsync(TKey id)
	{
		_requestContext.Request = new DSWebRequest(_serviceProvider, ControllerContext);

		var options = new DataSourceSelectOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		return await _ds.SelectAsync(id, null, options);
	}

	[HttpPost, ActionName("create")/* , ValidateAntiForgeryToken */]
	public async Task<ActionResult<TKey>> CreateAsync([FromBody] TCreate model, [FromQuery, Range(0, 1)] int? depends)
	{
		_requestContext.Request = new DSWebRequest(_serviceProvider, ControllerContext);

		var options = new DataSourceInsertOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		var id = await _ds.InsertAsync(model, null, options);
		return CreatedAtAction("create", new { id = id });

/*		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(CreatedAtAction(nameof(CreateAsync), new { id = default(TKey) }));*/
	}

	[HttpPut("{id}"), ActionName("update")]
	public async Task<ActionResult> UpdateAsync(TKey id, [FromBody] TUpdate model, [FromQuery, Range(0, 1)] int? depends)
	{
		_requestContext.Request = new DSWebRequest(_serviceProvider, ControllerContext);

		var options = new DataSourceUpdateOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		await _ds.UpdateAsync(id, model, null, null, options);
		return Ok();
	}

	[HttpDelete("{id}"), ActionName("delete")]
	public async Task<ActionResult> DeleteAsync(TKey id, [FromBody] TDelete? model)
	{
		_requestContext.Request = new DSWebRequest(_serviceProvider, ControllerContext);

		var options = new DataSourceDeleteOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		await _ds.DeleteAsync(id, model, null, options);
		return Ok();
	}

	[HttpPatch("{id}"), ActionName("restore")]
	public async Task<ActionResult> RestoreAsync(TKey id, [FromBody] TRestore? model)
	{
		_requestContext.Request = new DSWebRequest(_serviceProvider, ControllerContext);

		var options = new DataSourceRestoreOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		await _ds.RestoreAsync(id, model, null, options);
		return Ok();
	}
}