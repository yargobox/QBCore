using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.PgSql;

internal sealed class InsertQueryBuilder<TDocument, TCreate> : QueryBuilder<TDocument, TCreate>, IInsertQueryBuilder<TDocument, TCreate>
	where TDocument : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public InsertQueryBuilder(InsertQBBuilder<TDocument, TCreate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<TDocument> InsertAsync(TDocument document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Insert)
		{
			throw new NotSupportedException($"PostgreSQL insert query builder does not support an operation like '{top.ContainerOperation.ToString()}'.");
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfDbContextLogger;

		var deCreated = (SqlDEInfo?)Builder.DocumentInfo.DateCreatedField;
		var deModified = (SqlDEInfo?)Builder.DocumentInfo.DateModifiedField;

		if (deCreated?.Flags.HasFlag(DataEntryFlags.ReadOnly) == false && deCreated.Setter != null)
		{
			var dateValue = deCreated.Getter(document!);
			var zero = deCreated.DataEntryType.GetDefaultValue();
			if (dateValue == zero)
			{
				dateValue = Convert.ChangeType(DateTime.UtcNow, deCreated.UnderlyingType);
				deCreated.Setter(document!, dateValue);
			}
		}

		if (deModified?.Flags.HasFlag(DataEntryFlags.ReadOnly) == false && deModified.Setter != null)
		{
			var dateValue = deModified.Getter(document!);
			var zero = deModified.DataEntryType.GetDefaultValue();
			if (dateValue == zero)
			{
				dateValue = Convert.ChangeType(DateTime.UtcNow, deModified.DataEntryType);
				deModified.Setter(document!, dateValue);
			}
		}

		AttachQueryStringCallback(options, dbContext);

		try
		{
			await dbContext.AddAsync<TDocument>(document, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			return document;
		}
		finally
		{
			dbContext.Entry<TDocument>(document).State = EntityState.Detached;

			DetachQueryStringCallback(options, dbContext);
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