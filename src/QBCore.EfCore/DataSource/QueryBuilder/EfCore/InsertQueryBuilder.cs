using Microsoft.EntityFrameworkCore;
using QBCore.Configuration;
using QBCore.DataSource.Options;
using QBCore.Extensions.Internals;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed class InsertQueryBuilder<TDoc, TCreate> : QueryBuilder<TDoc, TCreate>, IInsertQueryBuilder<TDoc, TCreate> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Insert;

	public InsertQueryBuilder(InsertQBBuilder<TDoc, TCreate> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public async Task<TDoc> InsertAsync(TDoc document, DataSourceInsertOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		var top = Builder.Containers.First();
		if (top.ContainerOperation != ContainerOperations.Insert)
		{
			throw EX.QueryBuilder.Make.QueryBuilderOperationNotSupported(Builder.DataLayer.Name, QueryBuilderType.ToString(), top?.ContainerOperation.ToString());
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfCoreDbContextLogger;

		var deCreated = (EfCoreDEInfo?)Builder.DocInfo.DateCreatedField;
		var deModified = (EfCoreDEInfo?)Builder.DocInfo.DateModifiedField;

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
			await dbContext.AddAsync<TDoc>(document, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			return document;
		}
		finally
		{
			dbContext.Entry<TDoc>(document).State = EntityState.Detached;

			DetachQueryStringCallback(options, dbContext);
		}
	}

	private static void AttachQueryStringCallback(DataSourceOperationOptions? options, DbContext dbContext)
	{
		if (options?.QueryStringCallback != null)
		{
			var logger = (dbContext as IEfCoreDbContextLogger)
				?? throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfCoreDbContextLogger)} interface for logging query strings.");

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
			var logger = (dbContext as IEfCoreDbContextLogger)
				?? throw new NotSupportedException($"Database context '{dbContext.GetType().Name}' should have supported the {nameof(IEfCoreDbContextLogger)} interface for logging query strings.");

			logger.QueryStringCallback -= options.QueryStringCallback;
		}
	}
}