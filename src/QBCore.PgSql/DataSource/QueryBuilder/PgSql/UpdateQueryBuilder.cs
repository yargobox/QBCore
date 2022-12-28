using Microsoft.EntityFrameworkCore;

using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class UpdateQueryBuilder<TDocument, TUpdate> : QueryBuilder<TDocument, TUpdate>, IUpdateQueryBuilder<TDocument, TUpdate> where TDocument : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Update;

	public UpdateQueryBuilder(UpdateQBBuilder<TDocument, TUpdate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<TDocument?> UpdateAsync(object id, TUpdate document, IReadOnlySet<string>? validFieldNames = null, DataSourceUpdateOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (id == null)
		{
			throw new ArgumentNullException(nameof(id), "Identifier value not specified.");
		}
		if (document == null)
		{
			throw new ArgumentNullException(nameof(document), "Document not specified.");
		}
		if (options?.FetchResultDocument == true)
		{
			throw new NotSupportedException($"PostgreSQL update query builder does not support fetching the result document.");
		}

		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Update)
		{
			throw new NotSupportedException($"PostgreSQL update query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		var dbContext = _dataContext.AsDbContext();

		var deId = (SqlDEInfo?)Builder.DocumentInfo.IdField
			?? throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id data entry.");
		if (deId.Setter == null) throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an id data entry setter.");
		var deUpdated = (SqlDEInfo?)Builder.DocumentInfo.DateUpdatedField;
		var deModified = (SqlDEInfo?)Builder.DocumentInfo.DateModifiedField;

	/* 		if (Builder.Conditions.Count != 1)
			{
				throw new NotSupportedException($"PostgreSQL update query builder must have one single equality condition for the id data entry.");
			}
			var cond = Builder.Conditions.Single();
			if (cond.Alias != top.Alias || cond.Operation != FO.Equal || !cond.IsOnParam || cond.Field.Name != deId.Name)
			{
				throw new NotSupportedException($"PostgreSQL update query builder does not support custom conditions.");
			} */

		var update = Activator.CreateInstance<TDocument>();
		deId.Setter(update, id);
		dbContext.Attach<TDocument>(update);

		try
		{
			object? value;
			bool isSetValue, isUpdatedSet = false, isModifiedSet = false, hasFieldToUpdate = false;
			var dataEntries = Builder.DocumentInfo.DataEntries.Values.Cast<SqlDEInfo>();
			SqlDEInfo? deProjInfo;

			foreach (var deDocInfo in dataEntries.Where(x => x.Property != null && x.Name != deId.Name && (validFieldNames == null || validFieldNames.Contains(x.Name))))
			{
				deProjInfo = (SqlDEInfo?)(Builder.ProjectionInfo?.DataEntries ?? Builder.DocumentInfo.DataEntries).GetValueOrDefault(deDocInfo.Name);
				if (deProjInfo == null)
				{
					continue;
				}

				isSetValue = true;
				value = deProjInfo.Getter(document);

				if (deDocInfo == deUpdated)
				{
					isUpdatedSet = isSetValue = value is not null && value != deDocInfo.UnderlyingType.GetDefaultValue();
				}
				else if (deDocInfo == deModified)
				{
					isModifiedSet = isSetValue = value is not null && value != deDocInfo.UnderlyingType.GetDefaultValue();
				}

				if (isSetValue)
				{
					if (deDocInfo.Setter == null) throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an {deDocInfo.Name} field setter.");

					deDocInfo.Setter(update, value);
					hasFieldToUpdate = true;
				}
			}

			if (!hasFieldToUpdate)
			{
				return default(TDocument?);
			}

			if (deUpdated != null && !isUpdatedSet)
			{
				if (deUpdated.Setter == null) throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an {deUpdated.Name} field setter.");

				deUpdated.Setter(update, Convert.ChangeType(DateTime.UtcNow, deUpdated.UnderlyingType));
			}
			if (deModified != null && !isModifiedSet)
			{
				if (deModified.Setter == null) throw new InvalidOperationException($"Document '{Builder.DocumentInfo.DocumentType.ToPretty()}' does not have an {deModified.Name} field setter.");

				deModified.Setter(update, Convert.ChangeType(DateTime.UtcNow, deModified.UnderlyingType));
			}

			AttachQueryStringCallback(options, dbContext);

			try
			{
				await dbContext.SaveChangesAsync(cancellationToken);

				return default(TDocument?);
			}
			catch (DbUpdateException ex)
			{
				throw new KeyNotFoundException($"The update operation failed: there is no such record as '{id.ToString()}' in '{Builder.DocumentInfo.DocumentType.ToPretty()}'.", ex);
			}
			finally
			{
				DetachQueryStringCallback(options, dbContext);
			}
		}
		finally
		{
			dbContext.Entry<TDocument>(update).State = EntityState.Detached;
		}
	}

	private static void AttachQueryStringCallback(DataSourceOperationOptions? options, DbContext dbContext)
	{
		if (options?.QueryStringCallback != null)
		{
			var logger = (dbContext as IEfDbContextLogger)
				?? throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfDbContextLogger)} interface for logging query strings.");

			logger.QueryStringCallback += options.QueryStringCallback;
		}
		else if (options?.QueryStringCallbackAsync != null)
		{
			throw new NotSupportedException($"{nameof(DataSourceOperationOptions)}.{nameof(DataSourceOperationOptions.QueryStringCallbackAsync)} is not supported.");
		}
	}

	private static void DetachQueryStringCallback(DataSourceOperationOptions? options, DbContext dbContext)
	{
		if (options?.QueryStringCallback != null)
		{
			var logger = (dbContext as IEfDbContextLogger)
				?? throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfDbContextLogger)} interface for logging query strings.");

			logger.QueryStringCallback -= options.QueryStringCallback;
		}
	}
}