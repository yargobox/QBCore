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

			var deId = (SqlDEInfo?)Builder.DocumentInfo.IdField
				?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());
			var deDeleted = (SqlDEInfo?)Builder.DocumentInfo.DateDeletedField
				?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveDeletedDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());
			var deProjDeleted = Builder.ProjectionInfo?.DateDeletedField;

			string queryString;
			var sb = new StringBuilder();
			var dbo = ParseDbObjectName(top.DBSideName);
			var command = new NpgsqlCommand();
			bool isDeletedSet = false;
			object? deleted = null;

			if (document is not null)
			{
				DEInfo? de;

				foreach (var p in Builder.Parameters)
				{
					if (p.Direction.HasFlag(ParameterDirection.Input))
					{
						if (p.ParameterName == "@id")
						{
							p.Value = id;
						}
						else if (Builder.ProjectionInfo!.DataEntries.TryGetValue(p.ParameterName, out de))
						{
							p.Value = de.Getter(document);
						}
					}
				}

				if (deProjDeleted?.Getter != null)
				{
					deleted = deProjDeleted.Getter(document);
					isDeletedSet = deleted is not null && deleted != deProjDeleted.UnderlyingType.GetDefaultValue();
				}
			}

			sb.Append("UPDATE ");
			if (dbo.Schema.Length > 0)
			{
				sb.Append(dbo.Schema).Append('.');
			}
			sb.Append('"').Append(dbo.Object).AppendLine("\" SET");
			sb.Append("\t\"").Append(deDeleted.DBSideName).Append("\" = ");
			
			if (isDeletedSet)
			{
				var param = Builder.Parameters.FirstOrDefault(x => x.ParameterName == deProjDeleted!.Name);
				command.Parameters.Add(MakeUnnamedParameter(param, deleted));

				sb.Append("$1");
			}
			else
			{
				sb.Append("NULL");
			}

			sb.AppendLine().Append("WHERE ");
			BuildConditionTree(sb, Builder.Conditions, GetDBSideName, Builder.Parameters, command.Parameters);

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
				command.Transaction = transaction;

				var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				if (rowsAffected == 0)
				{
					throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocumentInfo.DocumentType.ToPretty());
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
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{
			throw new NotImplementedException();
		}
	}
}