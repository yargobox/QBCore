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
	//[ProducesResponseType(typeof(DataSourceResponse<IAsyncEnumerable<object>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<DataSourceResponse<IAsyncEnumerable<TSelect>>>> IndexAsync(
		[FromQuery, MaxLength(7)] string? _de,
		[FromQuery, MaxLength(8192)] string? _fl,
		[FromQuery, MaxLength(2048)] string? _so,
		[FromQuery, Range(1, int.MaxValue)] int? _ps,
		[FromQuery, Range(1, int.MaxValue)] int? _pn)
	{
		long skip = 0;
		if (_ps != null)
		{
			if (_pn == null)
			{
				throw new ArgumentNullException(nameof(_pn));
			}
			skip = (long)_ps.Value * (_pn.Value - 1);
		}
		else if (_pn != null)
		{
			throw new ArgumentNullException(nameof(_ps));
		}

		var dataSourceResponse = new DataSourceResponse<TSelect>
		{
			PageSize = _ps ?? -1,
			PageNumber = _pn ?? -1
		};
		var options = new DataSourceSelectOptions
		{
			QueryStringCallback = x => Console.WriteLine(x)//!!!
		};

		dataSourceResponse.Data = await (await _service.SelectAsync(SoftDel.Actual, null, null, skip, _pn ?? -1, options))
			.ToListAsync((bool x) => Interlocked.Exchange(ref dataSourceResponse.IsLastPage, x ? 1 : 0));

		return await Task.FromResult(Ok(dataSourceResponse));

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
	public async Task<ActionResult<TSelect>> GetAsync([FromQuery, Required] TKey id)
	{
		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(Ok());
	}

	[HttpPost, ActionName("create")]
	//[ValidateAntiForgeryToken]
	public async Task<ActionResult<TKey>> CreateAsync(
		[FromBody] TCreate model,
		[FromQuery, Range(0, 1)] int? _dp)
	{
		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(CreatedAtAction(nameof(CreateAsync), new { id = default(TKey) }));
	}

	[HttpPut("{id}"), ActionName("update")]
	public async Task<ActionResult> UpdateAsync(
		[FromQuery, Required] TKey id,
		[FromBody] TUpdate model,
		[FromQuery, Range(0, 1)] int? _dp)
	{
		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(Ok());
	}

	[HttpDelete("{id}"), ActionName("delete")]
	public async Task<ActionResult> DeleteAsync(
		[FromQuery, Required] TKey id,
		[FromBody] TDelete? model)
	{
		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(Ok());
	}

	[HttpPatch("{id}"), ActionName("restore")]
	public async Task<ActionResult> RestoreAsync(
		[FromQuery, Required] TKey id,
		[FromBody] TRestore? model)
	{
		Console.WriteLine(ControllerContext.ActionDescriptor.ActionName);
		Console.WriteLine(string.Join(", ", ControllerContext.ActionDescriptor.RouteValues.Select(x => x.Key + " = " + x.Value)));
		Console.WriteLine(string.Join(", ", Request.Query.Select(x => x.Key + " = " + x.Value)));
		return await Task.FromResult(Ok());
	}
}