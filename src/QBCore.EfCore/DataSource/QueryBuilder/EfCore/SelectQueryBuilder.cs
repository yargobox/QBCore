using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QBCore.Configuration;
using QBCore.DataSource.Options;

namespace QBCore.DataSource.QueryBuilder.EfCore;

internal sealed partial class SelectQueryBuilder<TDoc, TSelect> : QueryBuilder<TDoc, TSelect>, ISelectQueryBuilder<TDoc, TSelect> where TDoc : class
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public SelectQueryBuilder(SelectQBBuilder<TDoc, TSelect> building, IDataContext dataContext) : base(building, dataContext)
	{
		building.Normalize();
	}

	public IQueryable<TDoc> AsQueryable(DataSourceQueryableOptions? options = null)
	{
		return _dataContext.AsDbContext().Set<TDoc>();
	}

	public async Task<long> CountAsync(DataSourceCountOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (options != null)
		{
			if (options.Skip < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(options.Skip));
			}
			if (options.NativeSelectQuery != null && options.NativeSelectQuery is not string)
			{
				throw new ArgumentException(nameof(options.NativeSelectQuery));
			}
		}

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfCoreDbContextLogger;

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
				throw new NotSupportedException($"{nameof(DataSourceCountOptions)}.{nameof(DataSourceCountOptions.QueryStringCallbackAsync)} is not supported.");
			}
		}

		try
		{
			return await dbContext.Set<TDoc>().LongCountAsync();
		}
		finally
		{
			if (options != null && options.QueryStringCallback != null && logger != null)
			{
				logger.QueryStringCallback -= options.QueryStringCallback;
			}
		}
	}

	public async Task<IDSAsyncCursor<TSelect>> SelectAsync(long skip = 0L, int take = -1, DataSourceSelectOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (skip < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(skip));
		}

		/* var stages = BuildSelectPipelineStages(Builder.Containers, Builder.Connects, Builder.Conditions, Builder.Fields, Builder.SortOrders);
		var query = BuildSelectQuery(stages, Builder.Parameters); */

		//!!! stub code
		var sb = new StringBuilder();

		sb.Append("SELECT").AppendLine().Append("\t");

		var docInfo = (EfCoreDocumentInfo)Builder.DocInfo;
		var selInfo = (EfCoreDocumentInfo?)Builder.DtoInfo;
		var next = false;
		foreach (var de in docInfo.DataEntries.Values.Cast<EfCoreDEInfo>()
			.Where(de => de.Property != null && selInfo?.DataEntries.ContainsKey(de.Name) == true))
		{
			if (next) sb.Append(", "); else next = true;
			sb.Append('"').Append(de.DBSideName).Append('"');
		}
		sb.AppendLine().Append("FROM ")
			.Append(docInfo.EntityType?.GetSchema() ?? throw new InvalidOperationException())
			.Append(".\"")
			.Append(docInfo.EntityType.GetTableName() ?? docInfo.EntityType.GetViewName() ?? docInfo.EntityType.GetFunctionName())
			.Append('"');

		var dbContext = _dataContext.AsDbContext();
		var logger = dbContext as IEfCoreDbContextLogger;

		//!!! SqlQueryRaw is not supported by the PostgreSQL provider
		var query = dbContext.Database.SqlQueryRaw<TSelect>(sb.ToString());

		if (skip > 0L)
		{
			query = query.Skip((int)skip);
		}
		if (take >= 0)
		{
			query = query.Take(take + (options?.ObtainLastPageMarker == true ? 1 : 0));
		}

		if (options != null)
		{
			/* if (options.NativeSelectQueryCallback != null)
			{
				options.NativeSelectQueryCallback(query);
			} */
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
				throw new NotSupportedException($"{nameof(DataSourceCountOptions)}.{nameof(DataSourceCountOptions.QueryStringCallbackAsync)} is not supported.");
			}
		}

		try
		{
			if (options?.ObtainLastPageMarker == true)
			{
				return await Task.FromResult(new DSAsyncCursorWithLastPageMarker<TSelect>(query.AsAsyncEnumerable(), take, cancellationToken));
			}
			else
			{
				return await Task.FromResult(new DSAsyncCursor<TSelect>(query.AsAsyncEnumerable(), cancellationToken));
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