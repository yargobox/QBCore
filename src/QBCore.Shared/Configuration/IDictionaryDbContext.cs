namespace QBCore.Configuration;

public interface IDictionaryDbContext<TKey, TDocument>
{
	IDictionary<TKey, TDocument> Table { get; }
}