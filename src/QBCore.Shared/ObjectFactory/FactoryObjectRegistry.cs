using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace QBCore.ObjectFactory;

public interface IFactoryObjectDictionary<K, T> : IReadOnlyDictionary<K, T> where K : notnull
{
}

public interface IFactoryObjectRegistry<K, T> : IReadOnlyDictionary<K, T> where K : notnull
{
	void RegisterObject(K key, T value);
	bool TryRegisterObject(K key, T value);
	T GetOrRegisterObject(K key, Func<K, T> factoryMethod);
	void TrimExcess();
}
internal class ConcurrentFactoryObjectRegistry<K, T> : IFactoryObjectRegistry<K, T>, IFactoryObjectDictionary<K, T> where K : notnull
{
	public int Count => _registry.Count;
	public IEnumerable<K> Keys => _registry.Keys;
	public IEnumerable<T> Values => _registry.Values;

	protected readonly ConcurrentDictionary<K, T> _registry;

	public ConcurrentFactoryObjectRegistry() => _registry = new ConcurrentDictionary<K, T>();
	public ConcurrentFactoryObjectRegistry(int concurrencyLevel, int capacity) => _registry = new ConcurrentDictionary<K, T>(concurrencyLevel, capacity);
	public ConcurrentFactoryObjectRegistry(IEqualityComparer<K>? comparer) => _registry = new ConcurrentDictionary<K, T>(comparer);
	public ConcurrentFactoryObjectRegistry(int concurrencyLevel, int capacity, IEqualityComparer<K>? comparer) => _registry = new ConcurrentDictionary<K, T>(concurrencyLevel, capacity, comparer);
	public ConcurrentFactoryObjectRegistry(IDictionary<K, T> dictionary, IEqualityComparer<K>? comparer) => _registry = new ConcurrentDictionary<K, T>(dictionary, comparer);
	public ConcurrentFactoryObjectRegistry(IEnumerable<KeyValuePair<K, T>> collection, IEqualityComparer<K>? comparer) => _registry = new ConcurrentDictionary<K, T>(collection, comparer);
	public ConcurrentFactoryObjectRegistry(int concurrencyLevel, IEnumerable<KeyValuePair<K, T>> collection, IEqualityComparer<K>? comparer) => _registry = new ConcurrentDictionary<K, T>(concurrencyLevel, collection, comparer);

	public T this[K key] => _registry[key];

	public bool ContainsKey(K key) => _registry.ContainsKey(key);

	public IEnumerator<KeyValuePair<K, T>> GetEnumerator() => _registry.GetEnumerator();
	
	public bool TryGetValue(K key, [MaybeNullWhen(false)] out T value) => _registry.TryGetValue(key, out value);
	
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_registry).GetEnumerator();

	public void RegisterObject(K key, T value)
	{
		if (!_registry.TryAdd(key, value))
		{
			throw new InvalidOperationException($"{key.ToString()} is already registered.");
		}
	}

	public bool TryRegisterObject(K key, T value) => _registry.TryAdd(key, value);

	public T GetOrRegisterObject(K key, Func<K, T> factoryMethod) => _registry.GetOrAdd(key, factoryMethod);

	public void TrimExcess() { }

	public FactoryObjectRegistry<K, T> ToFactoryObjectRegistry()
	{
		var result = new FactoryObjectRegistry<K, T>(_registry.Count, _registry.Comparer);
		foreach (var p in _registry)
		{
			result.RegisterObject(p.Key, p.Value);
		}
		return result;
	}
}

internal class FactoryObjectRegistry<K, T> : IFactoryObjectRegistry<K, T>, IFactoryObjectDictionary<K, T> where K : notnull
{
	public int Count => _registry.Count;
	public IEnumerable<K> Keys => _registry.Keys;
	public IEnumerable<T> Values => _registry.Values;

	protected readonly Dictionary<K, T> _registry;

	public FactoryObjectRegistry() => _registry = new Dictionary<K, T>();
	public FactoryObjectRegistry(int capacity) => _registry = new Dictionary<K, T>(capacity);
	public FactoryObjectRegistry(IEqualityComparer<K>? comparer) => _registry = new Dictionary<K, T>(comparer);
	public FactoryObjectRegistry(int capacity, IEqualityComparer<K>? comparer) => _registry = new Dictionary<K, T>(capacity, comparer);
	public FactoryObjectRegistry(IDictionary<K, T> dictionary, IEqualityComparer<K>? comparer) => _registry = new Dictionary<K, T>(dictionary, comparer);
	public FactoryObjectRegistry(IEnumerable<KeyValuePair<K, T>> collection, IEqualityComparer<K>? comparer) => _registry = new Dictionary<K, T>(collection, comparer);

	public T this[K key] => _registry[key];

	public bool ContainsKey(K key) => _registry.ContainsKey(key);

	public IEnumerator<KeyValuePair<K, T>> GetEnumerator() => _registry.GetEnumerator();
	
	public bool TryGetValue(K key, [MaybeNullWhen(false)] out T value) => _registry.TryGetValue(key, out value);
	
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_registry).GetEnumerator();

	public void RegisterObject(K key, T value)
	{
		if (!_registry.TryAdd(key, value))
		{
			throw new InvalidOperationException($"{key.ToString()} is already registered.");
		}
	}

	public bool TryRegisterObject(K key, T value) => _registry.TryAdd(key, value);

	public T GetOrRegisterObject(K key, Func<K, T> factoryMethod)
	{
		T? value;
		if (!_registry.TryGetValue(key, out value))
		{
			if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));

			value = factoryMethod(key);
			_registry.Add(key, value);
		}
		return value;
	}

	public void TrimExcess() => _registry.TrimExcess();
}