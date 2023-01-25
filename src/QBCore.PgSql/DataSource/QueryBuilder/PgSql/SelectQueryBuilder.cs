using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper;
using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.DataSource.QueryBuilder;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed partial class SelectQueryBuilder<TDoc, TSelect> : QueryBuilder<TDoc, TSelect>, ISelectQueryBuilder<TDoc, TSelect> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public SelectQueryBuilder(SelectQBBuilder<TDoc, TSelect> builder, IDataContext dataContext)
		: base(builder, dataContext as IPgSqlDataContext ?? throw new ArgumentException(nameof(dataContext)))
	{
		builder.Normalize();
	}

	public IQueryable<TDoc> AsQueryable(DataSourceQueryableOptions? options = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> CountAsync(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();

/* 		if (options != null)
		{
			if (options.Skip < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(options.Skip));
			}
			if (options.NativeSelectQuery != null && options.NativeSelectQuery is not string)
			{
				throw new ArgumentException(nameof(options.NativeSelectQuery));
			}
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfDbContextLogger;

		if (options != null)
		{
			if (options.QueryStringCallback != null)
			{
				if (logger == null)
				{
					throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfDbContextLogger)} interface for logging query strings.");
				}

				logger.QueryStringCallback += options.QueryStringCallback;
			}
			else if (options.QueryStringCallbackAsync != null)
			{
				throw new NotSupportedException($"{nameof(DataSourceCountOptions)}.{nameof(DataSourceCountOptions.QueryStringCallbackAsync)} is not supported.");
			}
		}

		try
		{
			return await dbContext.Set<TDoc>().LongCountAsync();
		}
		finally
		{
			if (options != null && options.QueryStringCallback != null && logger != null)
			{
				logger.QueryStringCallback -= options.QueryStringCallback;
			}
		} */
	}

	public long Count(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public async Task<IDSAsyncCursor<TSelect>> SelectAsync(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

		var top = Builder.Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Select && top?.ContainerOperation != ContainerOperations.Exec)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
		}

		NpgsqlConnection? connection = null;
		NpgsqlTransaction? transaction = null;
		if (options != null)
		{
			if (options.ObtainLastPageMark && options.ObtainTotalCount)
			{
				throw new ArgumentException(nameof(options.ObtainLastPageMark));
			}

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

		if (top.ContainerOperation == ContainerOperations.Select)
		{
			string queryString;
			var sb = new StringBuilder();
			var command = new NpgsqlCommand();

			BuildSelectQuery(sb, skip, take >= 0 && options?.ObtainLastPageMark == true ? take + 1 : take, options?.ObtainTotalCount == true, command.Parameters, options?.IncludeOptionalFields);

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

				return options?.ObtainLastPageMark == true
					? new DSAsyncCursorWithLastPageMark<TSelect>(command, connection == null, take, cancellationToken)
					: options?.ObtainTotalCount == true
						? new DSAsyncCursorWithTotalCount<TSelect>(command, connection == null, skip, cancellationToken)
						: new DSAsyncCursor<TSelect>(command, connection == null, cancellationToken);
			}
			catch
			{
				if (connection == null)
				{
					connection = command.Connection;
				}
				else
				{
					connection = null;
				}

				await command.DisposeAsync().ConfigureAwait(false);
				
				if (connection != null)
				{
					await connection.DisposeAsync().ConfigureAwait(false);
				}

				throw;
			}
		}
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{
			throw new NotImplementedException();
		}
	}

	public IDSAsyncCursor<TSelect> Select(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

		var top = Builder.Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Select && top?.ContainerOperation != ContainerOperations.Exec)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
		}

		NpgsqlConnection? connection = null;
		NpgsqlTransaction? transaction = null;
		if (options != null)
		{
			if (options.ObtainLastPageMark && options.ObtainTotalCount)
			{
				throw new ArgumentException(nameof(options.ObtainLastPageMark));
			}

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

		if (top.ContainerOperation == ContainerOperations.Select)
		{
			string queryString;
			var sb = new StringBuilder();
			var command = new NpgsqlCommand();

			BuildSelectQuery(sb, skip, take >= 0 && options?.ObtainLastPageMark == true ? take + 1 : take, options?.ObtainTotalCount == true, command.Parameters, options?.IncludeOptionalFields);

			queryString = sb.ToString();

			if (options != null)
			{
				if (options.QueryStringCallback != null)
				{
					options.QueryStringCallback(queryString);
				}
				else if (options.QueryStringCallbackAsync != null)
				{
					throw new NotSupportedException($"Incompatible options of select query builder '{typeof(TSelect).ToPretty()}': '{nameof(DataSourceCountOptions.QueryStringCallbackAsync)}' is not supported in sync method.");
				}
			}

			try
			{
				command.CommandText = queryString;
				command.CommandType = CommandType.Text;
				command.Connection ??= connection;
				command.Connection ??= DataContext.AsNpgsqlDataSource().OpenConnection();
				command.Transaction ??= transaction;

				return options?.ObtainLastPageMark == true
					? new DSAsyncCursorWithLastPageMark<TSelect>(command, connection == null, take, cancellationToken)
					: options?.ObtainTotalCount == true
						? new DSAsyncCursorWithTotalCount<TSelect>(command, connection == null, skip, cancellationToken)
						: new DSAsyncCursor<TSelect>(command, connection == null, cancellationToken);
			}
			catch
			{
				if (connection == null)
				{
					connection = command.Connection;
				}
				else
				{
					connection = null;
				}

				command.Dispose();
				connection?.Dispose();

				throw;
			}
		}
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{
			throw new NotImplementedException();
		}
	}

	private void BuildSelectQuery(StringBuilder sb, long skip, int take, bool obtainTotalCount, NpgsqlParameterCollection commandParams, DEPath[]? includeOptionalFields)
	{
		var top = Builder.Containers.First();
		var topDbo = ExtensionsForSql.ParseDbObjectName(top.DBSideName);
		var topAlias = Builder.Containers.Count > 1 || topDbo.Object != top.Alias ? top.Alias : null;
		bool next;
		var dataEntries = Builder.DtoInfo?.DataEntries ?? Builder.DocInfo.DataEntries;
		QBField? field;

		Func<string?, DEPath, string> getDBSideName = topAlias != null
			? GetQuotedDBSideName
			: (alias, fieldPath) => GetQuotedDBSideName(alias == top.Alias ? null : alias, fieldPath);

		if (obtainTotalCount)
		{
			sb
				.Append("WITH ____cte AS").AppendLine()
				.Append('(').AppendLine();
		}

		foreach (var container in Builder.Containers)
		{
			if (container.ContainerOperation == ContainerOperations.Select)
			{
				sb.Append("SELECT").AppendLine().Append('\t');

				next = false;
				foreach (var de in dataEntries.Values.Cast<SqlDEInfo>())
				{
					if (de.Flags.HasAnyFlag(DataEntryFlags.NotMapped)) continue;

					if (next) sb.AppendLine(",").Append('\t'); else next = true;

					field = Builder.Fields.FirstOrDefault(x => x.Field.Path == de.Name);
					if (field == null)
					{
						sb.AppendQuotedDataEntry(topAlias, de);
						if (de.Name != de.DBSideName)
						{
							sb.Append(" AS \"").Append(de.Name).Append('"');
						}
					}
					else if (!field.IsExcluded)
					{
						if (!field.IsOptional || includeOptionalFields?.Contains(field.Field) == true)
						{
							if (field.RefField != null)
							{
								sb.AppendQuotedDataEntry(field.RefAlias, field.RefField);
								if (field.RefField.GetDBSideName() != de.Name)
								{
									sb.Append(" AS \"").Append(de.Name).Append('"');
								}
							}
							else
							{
								sb.AppendQuotedDataEntry(topAlias, de);
								if (de.Name != de.DBSideName)
								{
									sb.Append(" AS \"").Append(de.Name).Append('"');
								}
							}
						}
						else
						{
							sb.Append("NULL AS \"").Append(de.Name).Append('"');
						}
					}
					else
					{
						sb.Append("NULL AS \"").Append(de.Name).Append('"');
					}
				}

				sb.AppendLine().Append("FROM ").AppendQuotedContainer(topDbo);
				if (topAlias != null)
				{
					sb.Append(" AS ").Append(container.Alias);
				}
			}
			else if (container.ContainerOperation == ContainerOperations.Join || container.ContainerOperation == ContainerOperations.LeftJoin)
			{
				sb
					.AppendLine()
					.Append(container.ContainerOperation == ContainerOperations.Join ? "JOIN " : "LEFT JOIN ")
						.AppendQuotedContainer(container).Append(" AS ").Append(container.Alias).Append(" ON ");

				BuildConditionTree(sb, Builder.Connects, getDBSideName, Builder.Parameters, commandParams);
			}
			else if (container.ContainerOperation == ContainerOperations.CrossJoin)
			{
				sb
					.AppendLine()
					.Append("CROSS JOIN ").AppendQuotedContainer(container).Append(" AS ").Append(container.Alias);
			}
			else
			{
				throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), container.ContainerOperation.ToString());
			}
		}

		if (Builder.Conditions.Count > 0)
		{
			sb.AppendLine().Append("WHERE ");

			BuildConditionTree(sb, Builder.Conditions, getDBSideName, Builder.Parameters, commandParams);
		}

		next = false;
		foreach (var sort in Builder.SortOrders)
		{
			if (next)
			{
				sb.Append(", ");
			}
			else
			{
				next = true;
				sb.AppendLine().Append("ORDER BY ");
			}

			sb.AppendQuotedDataEntry(string.IsNullOrEmpty(sort.Alias) || sort.Alias == top.Alias ? topAlias : sort.Alias, sort.Field);

			if (sort.SortOrder == SO.Descending)
			{
				sb.Append(" DESC");
			}
			else if (sort.SortOrder != SO.Ascending)
			{
				throw new NotSupportedException($"{Builder.DataLayer.Name} does not support the sort order operation '{sort.SortOrder.ToString()}'.");
			}
		}

		if (obtainTotalCount)
		{
			sb
				.AppendLine()
				.Append(')').AppendLine()
				.Append("SELECT * FROM").AppendLine()
				.Append('(').AppendLine()
				.Append("\tTABLE ____cte");
		}

		if (take >= 0) sb.AppendLine().Append(obtainTotalCount ? "\t" : "").Append("LIMIT ").Append(take);
		if (skip > 0) sb.AppendLine().Append(obtainTotalCount ? "\t" : "").Append("OFFSET ").Append(skip);

		if (obtainTotalCount)
		{

			sb
				.AppendLine()
				.Append(") AS ____result").AppendLine()
				.Append("RIGHT JOIN (SELECT count(*) FROM ____cte) c(").Append(DSAsyncCursorWithTotalCount<TSelect>.TotalCountFieldName).Append(") ON true");
		}
	}
}