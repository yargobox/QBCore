using Microsoft.EntityFrameworkCore;

namespace Develop.API.Configuration;

public class DatabaseMigrateStartupFilter<T> : IStartupFilter where T : DbContext
{
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
	{
		return app =>
		{
			using (var scope = app.ApplicationServices.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<T>().Database;
				var commandTimeout = db.GetCommandTimeout();
				db.SetCommandTimeout(600);
				
				db.EnsureDeleted();
				db.EnsureCreated();
				//db.Migrate();

				db.SetCommandTimeout(commandTimeout);
			}

			next(app);
		};
	}
}