using System.Text;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal static class ExtensionsForPgSql
{
	public static StringBuilder AppendContainer(this StringBuilder sb, QBContainer container)
	{
		var dbo = ExtensionsForSql.ParseDbObjectName(container.DBSideName);

		if (dbo.Schema.Length > 0)
		{
			sb.Append(dbo.Schema).Append('.');
		}
		sb.Append('"').Append(dbo.Object).Append('"');

		return sb;
	}

	public static StringBuilder AppendContainer(this StringBuilder sb, (string Schema, string Object) dbo)
	{
		if (dbo.Schema.Length > 0)
		{
			sb.Append(dbo.Schema).Append('.');
		}
		sb.Append('"').Append(dbo.Object).Append('"');

		return sb;
	}
}