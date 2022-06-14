/* using Example1.BLL.Services;
using Example1.DAL.Entities.Brands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using QBCore.Controllers;
using QBCore.DataSource;

namespace Example1.API.Controllers;

[ApiController]
[Route("api/brands")]
[Route("api/v1/brand2")]
[Produces("application/json")]
public class BrandController : DataSourceController<int?, Brand, BrandCreateDto, BrandSelectDto, BrandUpdateDto, EmptyDto, BrandService>
{
	public BrandController(BrandService service, ILogger<BrandService> logger) : base(service, logger)
	{
	}
} */