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
	.AddApplicationPart(typeof(Develop.DAL.Entities.DVP.Project).Assembly)
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
	.AddSingleton<OptionsMonitor<SqlDbSettings>>()
	.AddTransient<ITransient<IDataContextProvider>, DataContextProvider>()
	.AddScoped<IDataContextProvider, DataContextProvider>()
	.AddScoped<DbDevelopContext>(sp => sp
		.GetRequiredService<IDataContextProvider>()
		.GetDataContext<DbDevelopContext>()
		.AsContext<DbDevelopContext>()
	)
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
app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
	endpoints.MapDataSourceControllers();
});

app.Run();