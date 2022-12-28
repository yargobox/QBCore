using Microsoft.EntityFrameworkCore;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class RestoreQueryBuilder<TDocument, TRestore> : QueryBuilder<TDocument, TRestore>, IRestoreQueryBuilder<TDocument, TRestore>
	where TDocument : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Restore;

	public RestoreQueryBuilder(QBRestoreBuilder<TDocument, TRestore> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task RestoreAsync(object id, TRestore? document = default(TRestore?), DataSourceRestoreOptions? options = null, CancellationToken cancellationToken = default)
	{
		if (id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document == null && typeof(TRestore) != typeof(EmptyDto))
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw new NotSupportedException($"EF restore query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfCoreDbContextLogger;

		var deId = (EfCoreDEInfo?)Builder.DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id data entry.");
		if (deId.Setter == null)
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id data entry setter.");
		var deDeleted = (EfCoreDEInfo?)Builder.DocumentInfo.DateDeletedField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have a date deletion field.");
		if (deDeleted.Flags.HasFlag(DataEntryFlags.ReadOnly))
			throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' has a readonly date deletion field!");

		if (Builder.Conditions.Count != 1)
		{
			throw new NotSupportedException($"EF update query builder must have one single equality condition for the id data entry.");
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

		if (!deDeleted.IsNullable && dateDel is null)
		{
			dateDel = Convert.ChangeType(DateTimeOffset.UtcNow, deDeleted.UnderlyingType);
		}

		if (options != null)
		{
			if (options.QueryStringCallback != null)
			{
				if (logger == null)
				{
					throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfCoreDbContextLogger)} interface for logging query strings.");
				}

				logger.QueryStringCallback += options.QueryStringCallback;
			}
			else if (options.QueryStringCallbackAsync != null)
			{
				throw new NotSupportedException($"{nameof(DataSourceRestoreOptions)}.{nameof(DataSourceRestoreOptions.QueryStringCallbackAsync)} is not supported.");
			}
		}

		try
		{
			int? restoredCount;
			if (dateDel is null)
			{
				restoredCount = await dbContext.Database.SqlQuery<int?>($"WITH restored AS (UPDATE \"{top.DBSideName}\" SET \"{deDeleted.DBSideName}\" = NULL WHERE \"{deId.DBSideName}\" = {id} RETURNING *) SELECT count(*) FROM restored;")
					.SingleOrDefaultAsync();
			}
			else
			{
				restoredCount = await dbContext.Database.SqlQuery<int?>($"WITH restored AS (UPDATE \"{top.DBSideName}\" SET \"{deDeleted.DBSideName}\" = {dateDel} WHERE \"{deId.DBSideName}\" = {id} RETURNING *) SELECT count(*) FROM restored;")
					.SingleOrDefaultAsync();
			}

			if ((restoredCount ?? 0) <= 0)
			{
				throw new KeyNotFoundException($"The restore operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.");
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