using Microsoft.EntityFrameworkCore;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.EntityFramework;

internal sealed class DeleteQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
	where TDocument : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Delete;

	public DeleteQueryBuilder(QbDeleteBuilder<TDocument, TDelete> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default)
	{
		if (id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document == null && typeof(TDelete) != typeof(EmptyDto))
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Delete)
		{
			throw new NotSupportedException($"EF delete query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfDbContextLogger;

		var deId = (EfDEInfo?)Builder.DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field.");
		if (deId.Setter == null)
		{
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field setter.");
		}

		if (Builder.Conditions.Count != 1)
		{
			throw new NotSupportedException($"EF delete query builder must have one single equality condition for the id field.");
		}
		var cond = Builder.Conditions.Single();
		if (cond.Alias != top.Alias || cond.Operation != FO.Equal || !cond.IsOnParam || cond.Field.Name != deId.Name)
		{
			throw new NotSupportedException($"EF delete query builder does not support custom conditions.");
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
			else if (options.QueryStringAsyncCallback != null)
			{
				throw new NotSupportedException($"{nameof(DataSourceOperationOptions)}.{nameof(DataSourceOperationOptions.QueryStringAsyncCallback)} is not supported.");
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
}