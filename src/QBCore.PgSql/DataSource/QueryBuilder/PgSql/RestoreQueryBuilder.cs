using System.Data;
using System.Text;
using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class RestoreQueryBuilder<TDoc, TRestore> : QueryBuilder<TDoc, TRestore>, IRestoreQueryBuilder<TDoc, TRestore> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public RestoreQueryBuilder(RestoreQBBuilder<TDoc, TRestore> builder, IDataContext dataContext)
		: base(builder, dataContext as IPgSqlDataContext ?? throw new ArgumentException(nameof(dataContext)))
	{
		builder.Normalize();
	}

	public async Task RestoreAsync(object id, TRestore? document = default(TRestore?), DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default)
	{
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null && typeof(TRestore) != typeof(EmptyDto)) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

		Builder.Normalize();
		var top = Builder.Containers[0];

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

		if (top.ContainerOperation == ContainerOperations.Update && (top.ContainerType == ContainerTypes.Table || top.ContainerType == ContainerTypes.View))
		{
			var command = new NpgsqlCommand();
			var sb = new StringBuilder();
			string queryString;

			if (Builder.Conditions.Count == 0)
			{
				throw EX.QueryBuilder.Make.QueryBuilderMustHaveAtLeastOneCondition(Builder.DataLayer.Name, QueryBuilderType.ToString());
			}

			var deId = (SqlDEInfo?)Builder.DocInfo.IdField
				?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocInfo.DocumentType.ToPretty());
			var deDeleted = (SqlDEInfo?)Builder.DocInfo.DateDeletedField
				?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveDeletedDataEntry(Builder.DocInfo.DocumentType.ToPretty());
			var deDtoDeleted = Builder.DtoInfo?.DateDeletedField;

			bool isDeletedSet = false;
			object? deleted = null;
			DEInfo? de;

			foreach (var p in Builder.Parameters.Where(x => x.Direction.HasFlag(ParameterDirection.Input)))
			{
				if (p.ParameterName == deId.Name)
				{
					p.Value = id;
				}
				else if (!p.HasValue && document is not null && Builder.DtoInfo!.DataEntries.TryGetValue(p.ParameterName, out de))
				{
					p.Value = de.Getter(document);
				}
			}

			if (document is not null && deDtoDeleted?.Getter != null)
			{
				deleted = deDtoDeleted.Getter(document);
				isDeletedSet = deleted is not null && deleted != deDtoDeleted.UnderlyingType.GetDefaultValue();
			}

			sb.Append("UPDATE ").AppendQuotedContainer(top).Append(" SET").AppendLine();
			sb.Append("\t\"").Append(deDeleted.DBSideName).Append("\" = ");

			if (isDeletedSet)
			{
				var param = Builder.Parameters.FirstOrDefault(x => x.ParameterName == deDtoDeleted!.Name);
				command.Parameters.Add(MakeUnnamedParameter(param, deleted));

				sb.Append("$1");
			}
			else
			{
				sb.Append("NULL");
			}

			sb.AppendLine().Append("WHERE ");
			BuildConditionTree(sb, Builder.Conditions, GetQuotedDBSideNameWithoutAlias, Builder.Parameters, command.Parameters);

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

				var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				if (rowsAffected == 0)
				{
					throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocInfo.DocumentType.ToPretty());
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
		}
		else if (top.ContainerOperation == ContainerOperations.Exec && (top.ContainerType == ContainerTypes.Function || top.ContainerType == ContainerTypes.Procedure))
		{
			
		}
		else
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top.ContainerOperation.ToString());
		}
	}
}