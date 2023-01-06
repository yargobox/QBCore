namespace QBCore.DataSource.QueryBuilder.Mongo;

internal static class ExtensionsForMongo
{
	public static string GetDBSideName(this DEPath path)
	{
		return string.Join('.', path.Cast<MongoDEInfo>().Select(x => x.DBSideName));
	}
}