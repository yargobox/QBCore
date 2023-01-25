using System.Data;
using System.Text;
using Dapper;
using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class UpdateQueryBuilder<TDoc, TUpdate> : QueryBuilder<TDoc, TUpdate>, IUpdateQueryBuilder<TDoc, TUpdate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public UpdateQueryBuilder(UpdateQBBuilder<TDoc, TUpdate> builder, IDataContext dataContext)
		: base(builder, dataContext as IPgSqlDataContext ?? throw new ArgumentException(nameof(dataContext)))
	{
		builder.Normalize();
	}

	public async Task<TDoc?> UpdateAsync(object id, TUpdate document, IReadOnlySet<string>? validFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

		var top = Builder.Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Update && top?.ContainerOperation != ContainerOperations.Exec)
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

		if (top.ContainerOperation == ContainerOperations.Update)
		{
			if (Builder.Conditions.Count == 0)
			{
				throw EX.QueryBuilder.Make.QueryBuilderMustHaveAtLeastOneCondition(Builder.DataLayer.Name, QueryBuilderType.ToString());
			}

			var deId = (SqlDEInfo?)Builder.DocInfo.IdField
				?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocInfo.DocumentType.ToPretty());
			if (deId.Setter == null)
				throw EX.QueryBuilder.Make.DataEntryDoesNotHaveSetter(Builder.DocInfo.DocumentType.ToPretty(), deId.Name);
			var deUpdated = (SqlDEInfo?)Builder.DocInfo.DateUpdatedField;
			var deModified = (SqlDEInfo?)Builder.DocInfo.DateModifiedField;

			string queryString;
			var sb = new StringBuilder();
			var command = new NpgsqlCommand();

			sb.Append("UPDATE ").AppendQuotedContainer(top).Append(" SET").AppendLine();

			var dataEntries = (Builder.DtoInfo?.DataEntries ?? Builder.DocInfo.DataEntries).Values.Cast<SqlDEInfo>();
			bool isSetValue, isUpdatedSet = false, isModifiedSet = false, next = false;
			object? value;
			SqlDEInfo? deDoc;
			QBParameter? qbParam;

			foreach (var deProj in dataEntries.Where(x => x.Name != deId.Name && (validFieldNames == null || validFieldNames.Contains(x.Name))))
			{
				if (deProj.Document == Builder.DocInfo)
				{
					deDoc = deProj;
				}
				else
				{
					deDoc = (SqlDEInfo?)Builder.DocInfo.DataEntries.GetValueOrDefault(deProj.Name);
					if (deDoc == null)
					{
						continue;
					}
				}

				isSetValue = true;
				value = deProj.Getter(document);

				if (deDoc == deUpdated)
				{
					isUpdatedSet = isSetValue = value is not null && value != deProj.UnderlyingType.GetDefaultValue();
				}
				else if (deDoc == deModified)
				{
					isModifiedSet = isSetValue = value is not null && value != deProj.UnderlyingType.GetDefaultValue();
				}

				if (isSetValue)
				{
					if (next) sb.AppendLine(","); else next = true;

					qbParam = Builder.Parameters.FirstOrDefault(x => x.ParameterName == deProj.Name);
					command.Parameters.Add(MakeUnnamedParameter(qbParam, value));

					sb.Append("\t\"").Append(deDoc.DBSideName).Append("\" = $").Append(command.Parameters.Count);
				}
			}

			if (!next)
			{
				if (options?.FetchResultDocument == true)
				{
					sb.Clear();
					
					sb.Append("SELECT * FROM ").AppendQuotedContainer(top);

					foreach (var p in Builder.Parameters.Where(x => (x.Direction & ParameterDirection.Input) == ParameterDirection.Input && x.ParameterName == "@id"))
					{
						p.Value = id;
						break;
					}

					sb.AppendLine().Append("WHERE ");
					BuildConditionTree(sb, Builder.Conditions, GetQuotedDBSideNameWithoutAlias, Builder.Parameters, command.Parameters);
					sb.AppendLine().Append("LIMIT 1");

					queryString = sb.ToString();

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
						command.Connection ??= connection;
						command.Connection ??= await DataContext.AsNpgsqlDataSource().OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
						command.Transaction ??= transaction;

						await using (var cursor = new DSAsyncCursor<TDoc>(command, connection == null, cancellationToken))
						{
							command = null;
							connection = null;

							if (await cursor.MoveNextAsync(CommandBehavior.SingleRow, cancellationToken))
							{
								return cursor.Current;
							}
						}

						throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
					}
					finally
					{
						if (command != null)
						{
							await command.DisposeAsync().ConfigureAwait(false);
						}

						if (connection != null)
						{
							await connection.DisposeAsync().ConfigureAwait(false);
						}
					}
				}
				else
				{
					return default(TDoc?);
				}
			}

			if (deUpdated != null && !isUpdatedSet)
			{
				sb.AppendLine(",").Append("\t\"").Append(deUpdated.DBSideName).Append("\" = NOW()");
			}
			if (deModified != null && !isModifiedSet)
			{
				sb.AppendLine(",").Append("\t\"").Append(deModified.DBSideName).Append("\" = NOW()");
			}

			foreach (var p in Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input) && x.ParameterName == "@id"))
			{
				p.Value = id;
				break;
			}

			sb.AppendLine().Append("WHERE ");
			BuildConditionTree(sb, Builder.Conditions, GetQuotedDBSideNameWithoutAlias, Builder.Parameters, command.Parameters);

			if (options?.FetchResultDocument == true)
			{
				sb.AppendLine().Append("RETURNING *");
			}

			queryString = sb.ToString();

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

				if (options?.FetchResultDocument == true)
				{
					var dataReader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					var rowParser = dataReader.GetRowParser<TDoc>();
					TDoc? result = null;
					
					while (await dataReader.ReadAsync(cancellationToken).ConfigureAwait(false))
					{
						result ??= rowParser(dataReader);
					}
					while (await dataReader.NextResultAsync().ConfigureAwait(false))
					{ }

					return result ?? throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
				}
				else
				{
					var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
					if (rowsAffected == 0)
					{
						throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
					}

					return default(TDoc?);
				}
			}
			finally
			{
				await command.DisposeAsync().ConfigureAwait(false);
				if (connection != null && options?.Connection == null) await connection.DisposeAsync().ConfigureAwait(false);
			}
		}
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{
			if (options?.FetchResultDocument == true) throw new NotSupportedException($"Procedure-based update query builder does not support fetching the result document.");

			throw new NotImplementedException();
		}
	}
}