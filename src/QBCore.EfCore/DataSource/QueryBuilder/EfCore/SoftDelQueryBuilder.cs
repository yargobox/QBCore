using Microsoft.EntityFrameworkCore;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class SoftDelQueryBuilder<TDocument, TDelete> : QueryBuilder<TDocument, TDelete>, IDeleteQueryBuilder<TDocument, TDelete>
	where TDocument : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.SoftDel;

	public SoftDelQueryBuilder(QBSoftDelBuilder<TDocument, TDelete> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task DeleteAsync(object id, TDelete? document = default(TDelete?), DataSourceDeleteOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id is null) throw EX.QueryBuilder.Make.IdentifierValueNotSpecified(nameof(id));
		if (document is null && typeof(TDelete) != typeof(EmptyDto)) throw EX.QueryBuilder.Make.DocumentNotSpecified(nameof(document));

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfCoreDbContextLogger;

		var deId = (EfCoreDEInfo?)Builder.DocumentInfo.IdField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveIdDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());
		if (deId.Setter == null)
			throw EX.QueryBuilder.Make.DataEntryDoesNotHaveSetter(Builder.DocumentInfo.DocumentType.ToPretty(), deId.Name);
		var deDeleted = (EfCoreDEInfo?)Builder.DocumentInfo.DateDeletedField
			?? throw EX.QueryBuilder.Make.DocumentDoesNotHaveDeletedDataEntry(Builder.DocumentInfo.DocumentType.ToPretty());
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
					throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfCoreDbContextLogger)} interface for logging query strings.");
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
					throw EX.QueryBuilder.Make.OperationFailedNoSuchRecord(QueryBuilderType.ToString(), id.ToString(), Builder.DocumentInfo.DocumentType.ToPretty());
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