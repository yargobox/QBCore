using System.Collections.Concurrent;
using QBCore.Configuration;

namespace Example1.DAL.Configuration;

public sealed class DictionaryDbContext<TKey, TDocument> : IDictionaryDbContext<TKey, TDocument> where TKey : notnull
{
	public IDictionary<TKey, TDocument> Table { get; private set; }

	public DictionaryDbContext()
	{
		Table = new ConcurrentDictionary<TKey, TDocument>();
	}
}