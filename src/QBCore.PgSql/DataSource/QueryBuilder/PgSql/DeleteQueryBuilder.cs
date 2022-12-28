using System.Data.Common;
using Npgsql;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class DeleteQueryBuilder<TDoc, TDelete> : QueryBuilder<TDoc, TDelete>, IDeleteQueryBuilder<TDoc, TDelete> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public DeleteQueryBuilder(DeleteQBBuilder<TDoc, TDelete> building, IDataContext dataContext)
		: base(building, dataContext as IPgSqlDataContext ?? throw new ArgumentException(nameof(dataContext)))
	{
		building.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default)
	{
		if (id is null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document is null && typeof(TDelete) != typeof(EmptyDto))
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}

		var top = Builder.Containers.FirstOrDefault();
		if (top?.ContainerOperation != ContainerOperations.Delete && top?.ContainerOperation != ContainerOperations.Exec)
		{
			throw new NotSupportedException($"{Builder.DataLayer.Name} delete query builder does not support an operation like '{top?.ContainerOperation.ToString()}'.");
		}

		if (top.ContainerOperation == ContainerOperations.Delete)
		{
			NpgsqlCommand? command = null;
			
			var connection = (NpgsqlConnection?)options?.Connection;
			var transaction = (NpgsqlTransaction?)options?.Transaction;
			if (transaction?.Connection != connection)
			{
				throw new InvalidOperationException($"The specified transaction is opened for the different connection.");
			}

			try
			{
				command = new NpgsqlCommand();

				if (Builder.Conditions.Count == 0)
				{
					command.CommandText = "DELETE FROM " + top.DBSideName + " WHERE "
				}
				
				connection ??= await DataContext.AsNpgsqlDataSource().OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
				command.Connection = connection;
				command.Transaction = transaction;

				var rowsAffected = command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				if (command != null) await command.DisposeAsync().ConfigureAwait(false);
				if (connection != null && options?.Connection == null) await connection.DisposeAsync().ConfigureAwait(false);
			}

			var deId = (SqlDEInfo?)Builder.DocumentInfo.IdField;
			if (deId is null)
			{
				throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id data entry.");
			}
			if (deId.Setter == null)
			{
				throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id data entry setter.");
			}

			if (Builder.Conditions.Count != 1)
			{
				throw new NotSupportedException($"{Builder.DataLayer.Name} delete query builder must have at least one equality condition for the id data entry.");
			}
			var cond = Builder.Conditions.First();
			if (cond.Alias != top.Alias || cond.Operation != FO.Equal || !cond.IsOnParam || cond.Field.Name != deId.Name)
			{
				throw new NotSupportedException($"{Builder.DataLayer.Name} delete query builder does not support custom conditions.");
			}

			if (document is not null)
			{
				dbContext.Attach(document);
				deId.Setter(document, id);
				dbContext.Remove(document);
			}

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
					throw new NotSupportedException($"{nameof(DataSourceOperationOptions)}.{nameof(DataSourceOperationOptions.QueryStringCallbackAsync)} is not supported.");
				}
			}

			try
			{
				if (document != null)
				{
					await dbContext.SaveChangesAsync(cancellationToken);
				}
				else
				{
					var deletedCount = await dbContext.Database.SqlQuery<int?>($"WITH deleted AS (DELETE FROM \"{top.DBSideName}\" WHERE \"{deId.DBSideName}\" = {id} RETURNING *) SELECT count(*) FROM deleted;")
						.SingleOrDefaultAsync();

					if ((deletedCount ?? 0) <= 0)
					{
						throw new KeyNotFoundException($"The delete operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
					}
				}
			}
			finally
			{
				if (options != null && options.QueryStringCallback != null && logger != null)
				{
					logger.QueryStringCallback -= options.QueryStringCallback;
				}
			}
		}
		else/*  if (top.ContainerOperation == ContainerOperations.Exec) */
		{

		}
	}
}