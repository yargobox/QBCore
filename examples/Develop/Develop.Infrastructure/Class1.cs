using QBCore.DataSource;

namespace Develop.Infrastructure;
public class Class1
{
	public Class1()
	{
		var name = PgSqlDataLayer.Default.Name;
	}
}
