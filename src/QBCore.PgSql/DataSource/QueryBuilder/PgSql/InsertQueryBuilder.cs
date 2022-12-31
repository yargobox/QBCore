using System.Data;
using System.Text;
using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class InsertQueryBuilder<TDoc, TCreate> : QueryBuilder<TDoc, TCreate>, IInsertQueryBuilder<TDoc, TCreate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public InsertQueryBuilder(InsertQBBuilder<TDoc, TCreate> builder, IDataContext dataContext)
		: base(builder, dataContext as IPgSqlDataContext ?? throw new ArgumentException(nameof(dataContext)))
	{
		builder.Normalize();
	}

	public async Task<TDoc> InsertAsync(TDoc document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (document is null) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

		var top = Builder.Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Insert && top?.ContainerOperation != ContainerOperations.Exec)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
		}

		NpgsqlConnection? connection = null;
		NpgsqlTransaction? transaction = null;
		if (options != null)
		{
			if (options.Connection != null)
			{
				connection = (options.Connection as NpgsqlConnection) ?? throw new ArgumentException(nameof(options.Connection));
			}
			if (options.Transaction != null)
			{
				transaction = (options.Transaction as NpgsqlTransaction) ?? throw new ArgumentException(nameof(options.Transaction));
				
				if (transaction.Connection != connection)
				{
					throw  EX.QueryBuilder.Make.SpecifiedTransactionOpenedForDifferentConnection();
				}
			}
		}

		if (top.ContainerOperation == ContainerOperations.Insert)
		{
			var deId = (SqlDEInfo?)Builder.DocumentInfo.IdField;
			var deCreated = (SqlDEInfo?)Builder.DocumentInfo.DateCreatedField;
			var deModified = (SqlDEInfo?)Builder.DocumentInfo.DateModifiedField;

			var dbo = ParseDbObjectName(top.DBSideName);
			var sb = new StringBuilder();

			sb.Append("INSERT INTO ");
			if (dbo.Schema.Length > 0)
			{
				sb.Append(dbo.Schema).Append('.');
			}
			sb.Append('"').Append(dbo.Object).Append('"').Append('(');

			var command = new NpgsqlCommand();
			bool isSetValue, isCreatedSet = false, isModifiedSet = false, next = false;
			object? value;
			QBParameter? param;

			foreach (var de in Builder.DocumentInfo.DataEntries.Values.Cast<SqlDEInfo>())
			{
				isSetValue = true;
				value = de.Getter(document);

				if (de == deCreated)
				{
					isCreatedSet = isSetValue = value is not null && value != de.UnderlyingType.GetDefaultValue();
				}
				else if (de == deModified)
				{
					isModifiedSet = isSetValue = value is not null && value != de.UnderlyingType.GetDefaultValue();
				}

				if (isSetValue)
				{
					if (next) sb.Append(", "); else next = true;

					sb.Append('"').Append(de.DBSideName!).Append('"');

					param = Builder.Parameters.FirstOrDefault(x => x.ParameterName == de.Name);
					command.Parameters.Add(MakeUnnamedParameter(param, value));
				}
			}

			if (deCreated != null && !isCreatedSet)
			{
				if (next) sb.Append(", "); else next = true;

				sb.Append('"').Append(deCreated.DBSideName!).Append('"');
			}
			if (deModified != null && !isModifiedSet)
			{
				if (next) sb.Append(", "); else next = true;

				sb.Append('"').Append(deModified.DBSideName!).Append('"');
			}

			sb.AppendLine(")").Append("VALUES (");
			int i;
			for (i = 1; i <= command.Parameters.Count; i++)
			{
				if (i > 1) sb.Append(", ");

				sb.Append("$").Append(i);
			}

			if (deCreated != null && !isCreatedSet)
			{
				if (i++ > 1) sb.Append(", ");

				sb.Append("NOW()");
			}
			if (deModified != null && !isModifiedSet)
			{
				if (i++ > 1) sb.Append(", ");

				sb.Append("NOW()");
			}

			sb.Append(')');

			if (deId?.Setter != null)
			{
				sb.Append(" RETURNING \"").Append(deId.DBSideName).Append('"');
			}

			var queryString = sb.ToString();

			if (options != null)
			{
				if (options.QueryStringCallbackAsync != null)
				{
					await options.QueryStringCallbackAsync(queryString).ConfigureAwait(false);
				}
				else if (options.QueryStringCallback != null)
				{
					options.QueryStringCallback(queryString);
				}
			}

			try
			{
				command.CommandText = queryString;
				command.CommandType = CommandType.Text;
				connection ??= await DataContext.AsNpgsqlDataSource().OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
				command.Connection = connection;
				command.Transaction = transaction;

				if (deId?.Setter != null)
				{
					value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
					deId.Setter(document, value);
				}
				else
				{
					await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				}
			}
			finally
			{
				await command.DisposeAsync().ConfigureAwait(false);

				if (connection != null && options?.Connection == null)
				{
					await connection.DisposeAsync().ConfigureAwait(false);
				}
			}

			return document;
		}
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{
			throw new NotImplementedException();
		}
	}
}