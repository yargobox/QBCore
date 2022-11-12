using Example1.API.Middlewares;
using Example1.DAL.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using QBCore.Configuration;
using QBCore.Controllers;
using QBCore.Extensions;
using QBCore.Extensions.Runtime;

EagerLoading.Initialize();

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
//BsonSerializer.RegisterSerializer(new EnumSerializer<UserRoles>(BsonType.String));
BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.String));

ConventionRegistry.Register("camelCase", new ConventionPack { new CamelCaseElementNameConvention() }, _ => true);

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
	.AddApplicationPart(typeof(Example1.DAL.Entities.Brands.Brand).Assembly)
	.AddApplicationPart(typeof(Example1.BLL.Services.BrandService).Assembly)
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
	.Configure<MongoDbSettings>(appBuilder.Configuration.GetSection(nameof(MongoDbSettings)))
	.AddSingleton<IDataContextProvider, DataContextProvider>()
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