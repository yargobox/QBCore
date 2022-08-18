using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace QBCore.ObjectFactory;

internal class FactoryObjectRegistry<TKey, TInterface> : IFactoryObjectRegistry<TKey, TInterface>, IFactoryObjectDictionary<TKey, TInterface> where TKey : notnull
{
	protected ConcurrentDictionary<TKey, TInterface> _registry = new ConcurrentDictionary<TKey, TInterface>();

	public TInterface this[TKey key] => _registry[key];
	public IEnumerable<TKey> Keys => _registry.Keys;
	public IEnumerable<TInterface> Values => _registry.Values;
	public int Count => _registry.Count;
	public bool ContainsKey(TKey key) => _registry.ContainsKey(key);
	public IEnumerator<KeyValuePair<TKey, TInterface>> GetEnumerator() => _registry.GetEnumerator();
	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TInterface value) => _registry.TryGetValue(key, out value);
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_registry).GetEnumerator();

	public void RegisterObject(TKey key, TInterface value)
	{
		if (!_registry.TryAdd(key, value))
		{
			throw new InvalidOperationException($"{key.ToString()} is already registered.");
		}
	}

	public bool TryRegisterObject(TKey key, TInterface value)
	{
		return _registry.TryAdd(key, value);
	}

	public TInterface TryGetOrRegisterObject(TKey key, Func<TKey, TInterface> factoryMethod)
	{
		return _registry.GetOrAdd(key, factoryMethod);
	}
}