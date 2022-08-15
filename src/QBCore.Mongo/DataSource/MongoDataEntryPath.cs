using System.Diagnostics;
using System.Linq.Expressions;

namespace QBCore.DataSource;

[DebuggerDisplay("{FullName}")]
internal sealed class MongoDataEntryPath : DataEntryPath
{
	public override IDataLayerInfo DataLayer => MongoDataLayer.Default;

	public string DBSideName => string.Join('.', this.Cast<MongoDataEntry>().Select(x => x.DBSideName));

	public MongoDataEntryPath(LambdaExpression path, bool allowPointToSelf) : base(path, allowPointToSelf) { }
	public MongoDataEntryPath(Type documentType, string path, bool allowPointToSelf) : base(documentType, path, allowPointToSelf) { }
}

internal static class ExtensionsForDataEntryPath
{
	public static string GetDBSideName(this DataEntryPath path)
	{
		if (path is MongoDataEntryPath de)
		{
			return de.DBSideName;
		}
		throw new InvalidOperationException($"Data entry path '{path.ToString(false)}' does not belong to Mongo.");
	}
}