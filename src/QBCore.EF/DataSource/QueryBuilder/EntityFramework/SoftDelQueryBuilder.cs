using Microsoft.EntityFrameworkCore;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.EntityFramework;

internal sealed class SoftDelQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
	where TDocument : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public SoftDelQueryBuilder(QbSoftDelBuilder<TDocument, TDelete> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
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
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw new NotSupportedException($"EF soft delete query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfDbContextLogger;

		var deId = (EfDEInfo?)Builder.DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field.");
		if (deId.Setter == null)
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id field setter.");
		var deDeleted = (EfDEInfo?)Builder.DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' has a readonly date deletion field!");

		if (Builder.Conditions.Count != 1)
		{
			throw new NotSupportedException($"EF update query builder must have one single equality condition for the id field.");
		}
		var cond = Builder.Conditions.Single();
		if (cond.Alias != top.Alias || cond.Operation != FO.Equal || !cond.IsOnParam || cond.Field.Name != deId.Name)
		{
			throw new NotSupportedException($"EF update query builder does not support custom conditions.");
		}

		object? dateDel = null;

		if (document is not null)
		{
			var getDateDelFromDto = Builder.ProjectionInfo?.DateDeletedField?.Getter ?? Builder.ProjectionInfo?.DataEntries.GetValueOrDefault(deDeleted.Name)?.Getter;
			if (getDateDelFromDto != null)
			{
				dateDel = getDateDelFromDto(document);
			}
		}

		if (dateDel is null || dateDel == deDeleted.UnderlyingType.GetDefaultValue())
		{
			dateDel = Convert.ChangeType(DateTimeOffset.UtcNow, deDeleted.UnderlyingType);
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
			//((EfDocumentInfo?)Builder.DocumentInfo).EntityType.GetSchemaQualifiedTableName()

			var deletedCount = await dbContext.Database.SqlQuery<int?>($"WITH updated AS (UPDATE \"{top.DBSideName}\" SET \"{deDeleted.DBSideName}\" = NOW() WHERE \"{deId.DBSideName}\" = {id} RETURNING *) SELECT count(*) FROM updated;")
				.SingleOrDefaultAsync();
				
				if ((deletedCount ?? 0) <= 0)
				{
					throw new KeyNotFoundException($"The soft delete operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
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