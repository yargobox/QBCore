namespace QBCore.Configuration;

public interface IReadOnlyDictionaryDbContext<TKey, TDocument>
{
	IReadOnlyDictionary<TKey, TDocument> Table { get; }
}