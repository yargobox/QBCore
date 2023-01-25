using System.Data;
using System.Text;
using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class SoftDelQueryBuilder<TDoc, TDelete> : QueryBuilder<TDoc, TDelete>, IDeleteQueryBuilder<TDoc, TDelete> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public SoftDelQueryBuilder(SoftDelQBBuilder<TDoc, TDelete> builder, IDataContext dataContext)
		: base(builder, dataContext as IPgSqlDataContext ?? throw new ArgumentException(nameof(dataContext)))
	{
		builder.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null && typeof(TDelete) != typeof(EmptyDto)) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

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
			var deDeleted = (SqlDEInfo?)Builder.DocInfo.DateDeletedField
				?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveDeletedDataEntry(Builder.DocInfo.DocumentType.ToPretty());
			var deProjDeleted = Builder.DtoInfo?.DateDeletedField;

			string queryString;
			var sb = new StringBuilder();
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
						else if (Builder.DtoInfo!.DataEntries.TryGetValue(p.ParameterName, out de))
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

			sb.Append("UPDATE ").AppendQuotedContainer(top).Append(" SET").AppendLine();
			sb.Append("\t\"").Append(deDeleted.DBSideName).Append("\" = ");
			
			if (isDeletedSet)
			{
				var param = Builder.Parameters.FirstOrDefault(x => x.ParameterName == deProjDeleted!.Name);
				command.Parameters.Add(MakeUnnamedParameter(param, deleted));

				sb.Append("$1");
			}
			else
			{
				sb.Append("NOW()");
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
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{
			throw new NotImplementedException();
		}
	}
}