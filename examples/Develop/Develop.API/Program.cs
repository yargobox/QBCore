using Develop.API.Configuration;
using Develop.API.Middlewares;
using Develop.DAL;
using Develop.DAL.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using QBCore.Configuration;
using QBCore.Controllers;
using QBCore.Extensions;
using QBCore.Extensions.Runtime;
using QBCore.ObjectFactory;

var appBuilder = WebApplication.CreateBuilder(args);

var builder = appBuilder.Services
	.AddControllers(options =>
	{
		options.SuppressAsyncSuffixInActionNames = true;
		options.Conventions.Add(new DataSourceControllerRouteConvention
		{
			RoutePrefix = "api/"
		});
		
	})
	.AddApplicationPart(typeof(Develop.Entities.DVP.Project).Assembly)
	.AddQBCore(options => { })
	.ConfigureApplicationPartManager(partManager =>
	{
		partManager.FeatureProviders.Add(new DataSourceControllerFeatureProvider());
	});

builder.Services
	.AddAutoMapper(config =>
	{
		config.AddProfile(new DataSourceMappings((source, dest) => true));
	})
	.Configure<SqlDbSettings>(appBuilder.Configuration.GetRequiredSection(nameof(SqlDbSettings)))
	.AddSingleton<OptionsListener<SqlDbSettings>>()
	.AddTransient<ITransient<IEfDataContextProvider>, EfDataContextProvider>()
	.AddScoped<IEfDataContextProvider, EfDataContextProvider>()
	.AddScoped<DbDevelopContext>(sp => sp.GetRequiredService<IEfDataContextProvider>().GetDataContext().AsDbContext<DbDevelopContext>())
	.AddTransient<IStartupFilter, DatabaseMigrateStartupFilter<DbDevelopContext>>()
	.AddRouting(options =>
	{
		options.LowercaseUrls = true;
		options.AppendTrailingSlash = true;
	})
	.AddEndpointsApiExplorer()
	.AddSwaggerGen();

var app = appBuilder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapDataSourceControllers();

app.Run();