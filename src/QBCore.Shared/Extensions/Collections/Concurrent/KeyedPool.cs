using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using QBCore.Extensions.Collections.Generic;

namespace QBCore.Extensions.Collections.Concurrent;

[DebuggerTypeProxy(typeof(DebugViewOfGenericDictionary<,>))]
[DebuggerDisplay("Count = {Count}")]
public class KeyedPool<K, T> : ICollection<KeyValuePair<K, T>>, IEnumerable<KeyValuePair<K, T>>, IEnumerable, IDictionary<K, T>, IReadOnlyCollection<KeyValuePair<K, T>>, IReadOnlyDictionary<K, T>, ICollection, IDictionary where K : notnull
{
	/// <summary>
	/// Gets the number of key/value pairs contained in the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
	/// </summary>
	/// <returns>The number of key/value pairs contained in the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.</returns>
	public int Count => _pool.Count;

	/// <summary>
	///  Gets the <see cref="System.Collections.Generic.IEqualityComparer{K}" /> that is used to determine equality of keys for the dictionary.
	/// </summary>
	/// <returns>The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> generic interface implementation that is used to determine equality of keys for the current <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> and to provide hash values for the keys.</returns>
	public IEqualityComparer<K> Comparer => _pool.Comparer;

	bool ICollection<KeyValuePair<K, T>>.IsReadOnly => false;
	bool IDictionary.IsReadOnly => false;
	bool ICollection.IsSynchronized => true;
	bool IDictionary.IsFixedSize => false;
	object ICollection.SyncRoot => throw new NotSupportedException();

	/// <summary>
	/// Gets a collection containing the keys in the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
	/// </summary>
	public Dictionary<K, T>.KeyCollection Keys => _pool.Keys;
	ICollection<K> IDictionary<K, T>.Keys => _pool.Keys;
	IEnumerable<K> IReadOnlyDictionary<K, T>.Keys => _pool.Keys;
	ICollection IDictionary.Keys => _pool.Keys;

	/// <summary>
	/// Gets a collection containing the values in the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
	/// </summary>
	public Dictionary<K, T>.ValueCollection Values => _pool.Values;
	ICollection<T> IDictionary<K, T>.Values => _pool.Values;
	IEnumerable<T> IReadOnlyDictionary<K, T>.Values => _pool.Values;
	ICollection IDictionary.Values => _pool.Values;

	protected Dictionary<K, T> _pool;



	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />class that is empty and has the default initial capacity.
	/// </summary>
	public KeyedPool()
	{
		_pool = new Dictionary<K, T>(0);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> class that is empty, has the default initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	public KeyedPool(IEqualityComparer<K>? comparer)
	{
		_pool = new Dictionary<K, T>(comparer);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> class that contains elements copied from
	/// the specified collection and has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	public KeyedPool(IEnumerable<KeyValuePair<K, T>> collection, IEqualityComparer<K>? comparer = null)
	{
		_pool = new Dictionary<K, T>(collection, comparer);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> class that contains elements copied from
	/// the specified dictionary and has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="dictionary">The dictionary whose elements are copied to the new dictionary.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <exception cref="System.ArgumentNullException">dictionary is null.</exception>
	public KeyedPool(IDictionary<K, T> dictionary, IEqualityComparer<K>? comparer = null)
	{
		_pool = new Dictionary<K, T>(dictionary, comparer);
	}



	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get or set.</param>
	/// <returns> The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
	public T this[K key]
	{
		get
		{
			return _pool[key];
		}
		set
		{
			Add(key, value);
		}
	}

	object? IDictionary.this[object key]
	{
		get
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			if (key is K tkey)
			{
				return _pool[tkey];
			}

			throw new ArgumentException(nameof(key));
		}
		set
		{
			if (key is null) throw new ArgumentNullException(nameof(key));

			if (key is K tkey)
			{
				if (value is null)
				{
					if (default(T) is not null)
					{
						throw new ArgumentNullException(nameof(value));
					}

					Add(tkey, default(T)!);
				}
				else if (value is T tvalue)
				{
					Add(tkey, tvalue);
				}
				else
				{
					throw new ArgumentException(nameof(value));
				}
			}

			throw new ArgumentException(nameof(key));
		}
	}



	/// <summary>
	/// Adds the specified key and value to the dictionary.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	/// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.</exception>
	public void Add(K key, T value)
	{
		if (key is null) throw new ArgumentNullException(nameof(key));

		Dictionary<K, T>? prev, next = null;

		do
		{
			prev = _pool;

			if (prev.ContainsKey(key))
			{
				return;
			}

			if (next is null)
			{
				next = new Dictionary<K, T>(prev, prev.Comparer);
			}
			else
			{
				next.Clear();
				next.EnsureCapacity(prev.Count + 1);
				foreach (var p in prev) next.Add(p.Key, p.Value);
			}

			next.Add(key, value);
		}
		while (Interlocked.CompareExchange(ref _pool, next, prev) != prev);
	}

	void ICollection<KeyValuePair<K, T>>.Add(KeyValuePair<K, T> item)
	{
		Add(item.Key, item.Value);
	}

	void IDictionary.Add(object key, object? value)
	{
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		if (value is null)
		{
			if (default(T) is not null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Add(tkey, default(T)!);
		}
		else if (value is T tvalue)
		{
			Add(tkey, tvalue);
		}
		else
		{
			throw new ArgumentException(nameof(value));
		}
	}

	/// <summary>
	/// Attempts to add the specified key and value to the dictionary.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. It can be null.</param>
	/// <returns>true if the key/value pair was added to the dictionary successfully; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null</exception>
	public bool TryAdd(K key, T value)
	{
		if (key is null) throw new ArgumentNullException(nameof(key));

		Dictionary<K, T>? prev, next = null;

		do
		{
			prev = _pool;

			if (prev.ContainsKey(key))
			{
				return false;
			}

			if (next is null)
			{
				next = new Dictionary<K, T>(prev, prev.Comparer);
			}
			else
			{
				next.Clear();
				next.EnsureCapacity(prev.Count + 1);
				foreach (var p in prev) next.Add(p.Key, p.Value);
			}

			next.Add(key, value);
		}
		while (Interlocked.CompareExchange(ref _pool, next, prev) != prev);

		return true;
	}

	/// <summary>
	/// Attempts to add the specified key and value to the dictionary.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="factoryMethod">value factory.</param>
	/// <returns>true if the key/value pair was added to the dictionary successfully; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null</exception>
	public T GetOrAdd(K key, Func<K, T> factoryMethod)
	{
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (factoryMethod is null) throw new ArgumentNullException(nameof(factoryMethod));

		Dictionary<K, T>? prev, next = null;
		T createdValue = default(T)!;
		T? existingValue;
		bool isCreated = false;

		do
		{
			prev = _pool;

			if (prev.TryGetValue(key, out existingValue))
			{
				return existingValue;
			}

			if (next is null)
			{
				next = new Dictionary<K, T>(prev, prev.Comparer);
			}
			else
			{
				next.Clear();
				next.EnsureCapacity(prev.Count + 1);
				foreach (var p in prev) next.Add(p.Key, p.Value);
			}

			if (!isCreated)
			{
				createdValue = factoryMethod(key);
				isCreated = true;
			}

			next.Add(key, createdValue);
		}
		while (Interlocked.CompareExchange(ref _pool, next, prev) != prev);

		return createdValue;
	}

	/// <summary>
	/// Adds the elements of the specified collection to the end of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
	/// </summary>
	/// <param name="collection">the collection whose elements should be added to the end of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	public void AddRange(IEnumerable<KeyValuePair<K, T>> collection)
	{
		if (collection is null) throw new ArgumentNullException(nameof(collection));

		Dictionary<K, T>? prev, next = null;
		bool changed = false;

		do
		{
			prev = _pool;

			if (collection.All(x => prev.ContainsKey(x.Key ?? throw new ArgumentNullException(nameof(collection) + ".Key"))))
			{
				return;
			}

			if (next is null)
			{
				next = new Dictionary<K, T>(prev, prev.Comparer);
			}
			else
			{
				next.Clear();
				foreach (var p in prev) next.Add(p.Key, p.Value);
			}

			foreach (var x in collection)
			{
				changed = next.TryAdd(x.Key, x.Value) || changed;
			}
		}
		while (changed && Interlocked.CompareExchange(ref _pool, next, prev) != prev);
	}


	/// <summary>
    /// This method is not supported. Properties can only be added to <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
	public bool Remove(K key)
	{
		throw new NotSupportedException();
	}

	bool ICollection<KeyValuePair<K, T>>.Remove(KeyValuePair<K, T> item)
	{
		throw new NotSupportedException();
	}

	void IDictionary.Remove(object key)
	{
		throw new NotSupportedException();
	}

	/// <summary>
    /// This method is not supported. Properties can only be added to <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
	public bool Remove(K key, out T value)
	{
		throw new NotSupportedException();
	}


	/// <summary>
    /// This method is not supported. Properties can only be added to <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
	public void Clear()
	{
		throw new NotSupportedException();
	}



	/// <summary>
	/// Determines whether the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool ContainsKey(K key)
	{
		return _pool.ContainsKey(key);
	}

	bool ICollection<KeyValuePair<K, T>>.Contains(KeyValuePair<K, T> item)
	{
		T? value;
		return _pool.TryGetValue(item.Key, out value) && EqualityComparer<T>.Default.Equals(value, item.Value);
	}

	bool IDictionary.Contains(object key)
	{
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		return _pool.ContainsKey(tkey);
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool TryGetValue(K key, [MaybeNullWhen(false)] out T value)
	{
		return _pool.TryGetValue(key, out value);
	}


	/// <summary>
	/// Copies the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> to a compatible one-dimensional array, starting at the specified index of the target array.
	/// </summary>
	/// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />. The Array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <exception cref="System.ArgumentNullException">array is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
	/// <exception cref="System.ArgumentException">The number of elements in the source <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> is greater than the available space from arrayIndex to the end of the destination array.</exception>
	public void CopyTo(KeyValuePair<K, T>[] array, int arrayIndex)
	{
		var pool = _pool;

		if (array is null) throw new ArgumentNullException(nameof(array));
		if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
		if (arrayIndex + array.Length < pool.Count) throw new ArgumentException(nameof(arrayIndex));

		--arrayIndex;
		foreach (var p in pool)
		{
			array[++arrayIndex] = p;
		}
	}

	void ICollection.CopyTo(Array array, int index)
	{
		var pool = _pool;

		if (array is null) throw new ArgumentNullException(nameof(array));
		if (array.Rank != 1) throw new ArgumentException(nameof(array));
		var lowerBound = array.GetLowerBound(0);
		if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
		if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
		{
			if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (pool.Count > 0) throw new ArgumentException(nameof(array));
			return;
		}
		if (array.Length - (index - lowerBound) < pool.Count) throw new ArgumentException(nameof(array));

		--index;
		foreach (var p in pool)
		{
			array.SetValue(p, ++index);
		}
	}



	/// <summary>
	/// Returns an enumerator that iterates through the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.
	/// </summary>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.Enumerator for the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.</returns>
	public IEnumerator<KeyValuePair<K, T>> GetEnumerator() => _pool.GetEnumerator();
	
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_pool).GetEnumerator();

	IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_pool).GetEnumerator();



	/// <summary>
	/// Copies the elements of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" /> to a new array.
	/// </summary>
	/// <returns>An array containing copies of the elements of the <see cref="QBCore.Extensions.Collections.Generic.KeyedPool{K, T}" />.</returns>
	public KeyValuePair<K, T>[] ToArray()
	{
		var pool = _pool;
		var arr = new KeyValuePair<K, T>[pool.Count];

		int i = -1;
		foreach (var p in pool)
		{
			arr[++i] = p;
		}

		return arr;
	}
}