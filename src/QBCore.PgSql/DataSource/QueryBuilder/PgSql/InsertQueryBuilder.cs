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
			var deId = (SqlDEInfo?)Builder.DocInfo.IdField;
			var deCreated = (SqlDEInfo?)Builder.DocInfo.DateCreatedField;
			var deModified = (SqlDEInfo?)Builder.DocInfo.DateModifiedField;

			var sb = new StringBuilder();

			sb.Append("INSERT INTO ").AppendContainer(top);

			var command = new NpgsqlCommand();
			bool isSetValue, isCreatedSet = false, isModifiedSet = false, next = false;
			object? value;
			QBParameter? param;

			foreach (var de in Builder.DocInfo.DataEntries.Values.Cast<SqlDEInfo>())
			{
				if (de.Flags.HasAnyFlag(DataEntryFlags.ReadOnly | DataEntryFlags.NoStorage))
				{
					continue;
				}

				isSetValue = true;
				value = de.Getter(document);

				if (de == deCreated)
				{
					isCreatedSet = isSetValue = value is not null && !value.Equals(de.UnderlyingType.GetDefaultValue());
				}
				else if (de == deModified)
				{
					isModifiedSet = isSetValue = value is not null && !value.Equals(de.UnderlyingType.GetDefaultValue());
				}

				if (isSetValue)
				{
					if (next) sb.Append(", "); else { next = true; sb.Append('('); }

					sb.Append('"').Append(de.DBSideName!).Append('"');

					param = Builder.Parameters.FirstOrDefault(x => x.ParameterName == de.Name);
					command.Parameters.Add(MakeUnnamedParameter(param, value));
				}
			}

			if (deCreated != null && !isCreatedSet && !deCreated.Flags.HasAnyFlag(DataEntryFlags.ReadOnly))
			{
				if (next) sb.Append(", "); else { next = true; sb.Append('('); }

				sb.Append('"').Append(deCreated.DBSideName!).Append('"');
			}
			if (deModified != null && !isModifiedSet && !deModified.Flags.HasAnyFlag(DataEntryFlags.ReadOnly))
			{
				if (next) sb.Append(", "); else { next = true; sb.Append('('); }

				sb.Append('"').Append(deModified.DBSideName!).Append('"');
			}

			if (next)
			{
				sb.AppendLine(")").Append("VALUES (");

				int i;
				for (i = 1; i <= command.Parameters.Count; i++)
				{
					if (i > 1) sb.Append(", ");

					sb.Append("$").Append(i);
				}

				if (deCreated != null && !isCreatedSet && !deCreated.Flags.HasAnyFlag(DataEntryFlags.ReadOnly))
				{
					if (i++ > 1) sb.Append(", ");

					sb.Append("NOW()");
				}
				if (deModified != null && !isModifiedSet && !deModified.Flags.HasAnyFlag(DataEntryFlags.ReadOnly))
				{
					if (i++ > 1) sb.Append(", ");

					sb.Append("NOW()");
				}

				sb.Append(')');
			}

			if (deId?.Flags.HasAnyFlag(DataEntryFlags.ReadOnly | DataEntryFlags.NoStorage) == true)
			{
				if (deId.DependsOn != null)
				{
					sb.AppendLine().Append("RETURNING ");

					next = false;
					foreach (var de in deId.DependsOn.Select(x => Builder.DocInfo.DataEntries[x]).Cast<SqlDEInfo>())
					{
						if (next) sb.Append(", "); else next = true;

						sb.Append('"').Append(de.DBSideName).Append('"');
					}
				}
				else if (deId.Setter != null)
				{
					sb.AppendLine().Append("RETURNING \"").Append(deId.DBSideName).Append('"');
				}
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
				command.Transaction ??= transaction;

				if (deId?.Flags.HasAnyFlag(DataEntryFlags.ReadOnly | DataEntryFlags.NoStorage) == true)
				{
					if (deId.DependsOn != null)
					{
						await using var dataReader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false);
						if (!await dataReader.NextResultAsync(cancellationToken).ConfigureAwait(false))
						{
							throw new InvalidOperationException();
						}

						foreach (var de in deId.DependsOn.Select(x => Builder.DocInfo.DataEntries[x]).Cast<SqlDEInfo>())
						{
							if (de.Setter != null)
							{
								de.Setter(document, dataReader[de.DBSideName!]);
							}
						}
					}
					else if (deId.Setter != null)
					{
						value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
						deId.Setter(document, value);
					}
					else
					{
						await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
					}
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