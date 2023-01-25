using System.Text;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal static class ExtensionsForPgSql
{
	public static StringBuilder AppendQuotedContainer(this StringBuilder sb, QBContainer container)
	{
		var dbo = ExtensionsForSql.ParseDbObjectName(container.DBSideName);

		if (!string.IsNullOrEmpty(dbo.Schema))
		{
			sb.Append(dbo.Schema).Append('.');
		}
		sb.Append('"').Append(dbo.Object).Append('"');

		return sb;
	}

	public static StringBuilder AppendQuotedContainer(this StringBuilder sb, (string Schema, string Object) dbo)
	{
		if (!string.IsNullOrEmpty(dbo.Schema))
		{
			sb.Append(dbo.Schema).Append('.');
		}
		sb.Append('"').Append(dbo.Object).Append('"');

		return sb;
	}

	public static StringBuilder AppendQuotedDataEntry(this StringBuilder sb, string? alias, SqlDEInfo de)
	{
		if (!string.IsNullOrEmpty(alias))
		{
			sb.Append(alias).Append('.');
		}

		sb.Append('"').Append(de.DBSideName).Append('"');

		return sb;
	}

	public static StringBuilder AppendQuotedDataEntry(this StringBuilder sb, string? alias, DEPath fieldPath)
	{
		if (!string.IsNullOrEmpty(alias))
		{
			sb.Append(alias).Append('.');
		}

		sb.Append('"').Append(fieldPath.GetDBSideName()).Append('"');

		return sb;
	}
}