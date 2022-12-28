using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using QBCore.Extensions.Linq.Expressions;

namespace Develop.DAL.PostgreSQL;

internal static class PostgreSQLExtensions
{
	public static PropertyBuilder<DateTime> HasDefaultDateTimeNowConstraint(this PropertyBuilder<DateTime> propertyBuilder)
		=> propertyBuilder.HasDefaultValueSql("NOW()");
	
	public static IndexBuilder<TEntity> HasFilterNotNull<TEntity>(this IndexBuilder<TEntity> indexBuilder, Expression<Func<TEntity, object?>> memberSelector) where TEntity : class
		=> indexBuilder.HasFilter($"\"{memberSelector.GetMemberName()}\" IS NOT NULL");

	public static MigrationBuilder CreateView(this MigrationBuilder migrationBuilder, string name, string schema, string body)
	{
		migrationBuilder.Sql(string.Concat($"CREATE OR REPLACE VIEW {schema}.\"{name}\" AS", Environment.NewLine, body, ";"));
		return migrationBuilder;
	}

	public static MigrationBuilder DropView(this MigrationBuilder migrationBuilder, string name, string schema)
	{
		migrationBuilder.Sql($"DROP VIEW {schema}.\"{name}\";");
		return migrationBuilder;
	}
}