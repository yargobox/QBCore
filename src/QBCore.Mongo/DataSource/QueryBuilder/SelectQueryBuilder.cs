using System.Runtime.CompilerServices;
using MongoDB.Driver;
using QBCore.ObjectFactory;

namespace QBCore.DataSource.QueryBuilder;

internal sealed class SelectQueryBuilder<TDocument, TSelect> : QueryBuilder<TDocument, TSelect>, ISelectQueryBuilder<TDocument, TSelect>
{
	public override QueryBuilderTypes QueryBuilderType => QueryBuilderTypes.Select;

	public override Origin Source => new Origin(typeof(SelectQueryBuilder<TDocument, TSelect>));

	public SelectQueryBuilder(QBBuilder<TDocument, TSelect> building)
		: base(building)
	{
	}

	public async Task<long> CountAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		DataSourceSelectOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		if (options?.NativeOptions != null && options.NativeOptions is not CountOptions)
		{
			throw new ArgumentException(nameof(options));
		}

		var countOptions = (CountOptions?)options?.NativeOptions;

		IMongoCollection<TDocument> _collection = null!;//!!!

		return await _collection.CountDocumentsAsync(Builders<TDocument>.Filter.Empty, countOptions, cancellationToken);
	}

	public async IAsyncEnumerable<TSelect> SelectAsync(
		IReadOnlyCollection<QBCondition>? conditions = null,
		IReadOnlyCollection<QBParameter>? parameters = null,
		IReadOnlyCollection<QBSortOrder>? sortOrders = null,
		long? skip = null,
		int? take = null,
		DataSourceSelectOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		if (options?.NativeOptions != null && options.NativeOptions is not FindOptions<TDocument, TSelect>)
		{
			throw new ArgumentException(nameof(options));
		}

		var findOptions = (FindOptions<TDocument, TSelect>?) options?.NativeOptions ?? new FindOptions<TDocument, TSelect>();
		findOptions.Skip = (int?)skip;
		findOptions.Limit = take;

		IMongoCollection<TDocument> _collection = null!;//!!!

		using (var cursor = await _collection.FindAsync(Builders<TDocument>.Filter.Empty, findOptions, cancellationToken))
		{
			while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
			{
				foreach (var doc in cursor.Current)
				{
					yield return doc;
				}
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
	}
}