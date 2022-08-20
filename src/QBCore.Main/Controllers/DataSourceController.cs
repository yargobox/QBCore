using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QBCore.Controllers.Models;
using QBCore.DataSource;
using QBCore.DataSource.Options;

namespace QBCore.Controllers;

[Produces("application/json")]
public class DataSourceController<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore, TDataSource> : ControllerBase
	where TDataSource : IDataSource<TKey, TDocument, TCreate, TSelect, TUpdate, TDelete, TRestore>
{
	protected ILogger<TDataSource> _logger;
	protected TDataSource _service;

	public DataSourceController(TDataSource service, ILogger<TDataSource> logger)
	{
		_service = service;
		_logger = logger;
	}

	[HttpGet, ActionName("index")]
	//[ProducesResponseType(typeof(DataSourceResponse<TSelect>), StatusCodes.Status200OK)]
	public async Task<ActionResult<DataSourceResponse<TSelect>>> IndexAsync(
		[FromQuery, MaxLength(7)] string? mode,
		[FromQuery, MaxLength(8192)] string? filter,
		[FromQuery, MaxLength(2048)] string? sort,
		[FromQuery, Range(1, int.MaxValue)] int? size,
		[FromQuery, Range(1, int.MaxValue)] int? num)
	{
		long skip = 0;
		if (size != null)
		{
			if (num == null)
			{
				throw new ArgumentNullException(nameof(num));
			}
			skip = (long)size.Value * (num.Value - 1);
		}
		else if (num != null)
		{
			throw new ArgumentNullException(nameof(size));
		}

		var dataSourceResponse = new DataSourceResponse<TSelect>
		{
			PageSize = size ?? -1,
			PageNumber = num ?? -1
		};
		var options = new DataSourceSelectOptions
		{
			ObtainLastPageMarker = true,
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		dataSourceResponse.Data =
			await ( await _service.SelectAsync(SoftDel.Actual, null, null, null, skip, num ?? -1, options) )
				.ToListAsync((bool x) => dataSourceResponse.IsLastPage = x ? 1 : 0);

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
		var options = new DataSourceSelectOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		return await _service.SelectAsync(id, null, options);
	}

	[HttpPost, ActionName("create")/* , ValidateAntiForgeryToken */]
	public async Task<ActionResult<TKey>> CreateAsync([FromBody] TCreate model, [FromQuery, Range(0, 1)] int? depends)
	{
		var options = new DataSourceInsertOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		var id = await _service.InsertAsync(model, null, options);
		return CreatedAtAction("create", new { id = id });

/*		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(CreatedAtAction(nameof(CreateAsync), new { id = default(TKey) }));*/
	}

	[HttpPut("{id}"), ActionName("update")]
	public async Task<ActionResult> UpdateAsync(TKey id, [FromBody] TUpdate model, [FromQuery, Range(0, 1)] int? depends)
	{
		var options = new DataSourceUpdateOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		await _service.UpdateAsync(id, model, null, null, options);
		return Ok();
	}

	[HttpDelete("{id}"), ActionName("delete")]
	public async Task<ActionResult> DeleteAsync(TKey id, [FromBody] TDelete? model)
	{
		var options = new DataSourceDeleteOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		await _service.DeleteAsync(id, model, null, options);
		return Ok();
	}

	[HttpPatch("{id}"), ActionName("restore")]
	public async Task<ActionResult> RestoreAsync(TKey id, [FromBody] TRestore? model)
	{
		var options = new DataSourceRestoreOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		await _service.RestoreAsync(id, model, null, options);
		return Ok();
	}
}