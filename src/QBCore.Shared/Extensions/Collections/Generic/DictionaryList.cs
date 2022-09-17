using System.Collections;
using System.Diagnostics;

namespace QBCore.Extensions.Collections.Generic;

/// <summary>
/// Represents an ordered collection of keys and values based on <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />
/// </summary>
/// <typeparam name="K">The type of keys in the dictionary, which cannot be an integer.</typeparam>
/// <typeparam name="T">The type of the values in the dictionary.</typeparam>
[DebuggerTypeProxy(typeof(DebugViewOfGenericDictionary<,>))]
[DebuggerDisplay("Count = {Count}")]
public class DictionaryList<K, T> : KeyedList<K, T>, ICollection<KeyValuePair<K, T>>, IEnumerable<KeyValuePair<K, T>>, IEnumerable, IDictionary<K, T>, IReadOnlyCollection<KeyValuePair<K, T>>, IReadOnlyDictionary<K, T>, ICollection, IDictionary where K : notnull
{
	ICollection<K> IDictionary<K, T>.Keys => new ReadOnlyKeyList(this);
	IEnumerable<K> IReadOnlyDictionary<K, T>.Keys => new ReadOnlyKeyList(this);
	ICollection IDictionary.Keys => new ReadOnlyKeyList(this);

	ICollection<T> IDictionary<K, T>.Values => this;
	IEnumerable<T> IReadOnlyDictionary<K, T>.Values => this;
	ICollection IDictionary.Values => new ValueCollection(this);

	bool ICollection<KeyValuePair<K, T>>.IsReadOnly => false;
	bool IDictionary.IsReadOnly => false;
	bool IDictionary.IsFixedSize => false;


	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />class that is empty and has the default initial capacity.
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null</exception>
	public DictionaryList(Func<T, K> getKeyFromItem)
		: base(getKeyFromItem)
	{
		Debug.Assert(typeof(K) != typeof(int), $"'int' keys is not supoorted. Use KeyedList<int, {typeof(T).Name}> instead.");
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> class that is empty, has the default initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null.</exception>
	public DictionaryList(Func<T, K> getKeyFromItem, IEqualityComparer<K>? comparer, int indexCreationThreshold = DefaultIndexCreationThreshold)
		: base(getKeyFromItem, comparer, indexCreationThreshold)
	{
		Debug.Assert(typeof(K) != typeof(int), $"'int' keys is not supoorted. Use KeyedList<int, {typeof(T).Name}> instead.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> class that contains elements copied from
	/// the specified collection and has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null -or- collection is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public DictionaryList(Func<T, K> getKeyFromItem, IEnumerable<T> collection, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
		: base(getKeyFromItem, collection, comparer, indexCreationThreshold)
	{
		Debug.Assert(typeof(K) != typeof(int), $"'int' keys is not supoorted. Use KeyedList<int, {typeof(T).Name}> instead.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="capacity">The number of elements that the new list can initially store.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">capacity is less than 0 -or- indexCreationThreshold is less than -1.</exception>
	public DictionaryList(Func<T, K> getKeyFromItem, int capacity, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
		: base(getKeyFromItem, capacity, comparer, indexCreationThreshold)
	{
		Debug.Assert(typeof(K) != typeof(int), $"'int' keys is not supoorted. Use KeyedList<int, {typeof(T).Name}> instead.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> class that ATTACHES the specified list and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="listToAttach">The list to attach (NOT COPY).</param>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null -or- collection is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public DictionaryList(List<T> listToAttach, Func<T, K> getKeyFromItem, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
		: base(listToAttach, getKeyFromItem, comparer, indexCreationThreshold)
	{
		Debug.Assert(typeof(K) != typeof(int), $"'int' keys is not supoorted. Use KeyedList<int, {typeof(T).Name}> instead.");
	}

	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get or set.</param>
	/// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	/// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
	/// <exception cref="System.ArgumentException">On a key change, when key and value.{Key} are different, but value.{Key} exists.</exception>
	public T this[K key]
	{
		get
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			if (_index == null)
			{
				var index = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), key));
				if (index < 0)
				{
					throw new KeyNotFoundException();
				}
				return _list[index];
			}
			return _index[key];
		}
		set
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			var itemKey = _getKeyFromItem(value) ?? throw new ArgumentNullException(nameof(value) + ".{Key}");
			var index = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), key));

			if (_comparer.Equals(key, itemKey))
			{
				if (index < 0)
				{
					BuildIndexIfNeeded();

					_index?.Add(itemKey, value);
					_list.Add(value);
				}
				else
				{
					_list[index] = value;
				}
			}
			else
			{
				if (index < 0)
				{
					BuildIndexIfNeeded();

					if (_index == null)
					{
						if (_list.Exists(x => _comparer.Equals(_getKeyFromItem(x), itemKey)))
						{
							throw new ArgumentException(nameof(value) + ".{Key}");
						}
					}
					else
					{
						_index.Add(itemKey, value);
					}

					_list.Add(value);
				}
				else
				{
					if (_index != null)
					{
						_index.Add(itemKey, value);
						_index.Remove(key);
					}

					_list[index] = value;
				}
			}
		}
	}
	object? IDictionary.this[object key]
	{
		get
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			if (key is K tkey)
			{
				return this[tkey];
			}

			throw new ArgumentException(nameof(key));
		}
		set
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			if (key is K tkey)
			{
				if (value == null)
				{
					if (default(T) != null)
					{
						throw new ArgumentNullException(nameof(value));
					}

					this[tkey] = default(T)!;
				}
				else if (value is T tvalue)
				{
					this[tkey] = tvalue;
				}
				else
				{
					throw new ArgumentException(nameof(value));
				}
			}

			throw new ArgumentException(nameof(key));
		}
	}


	void IDictionary<K, T>.Add(K key, T value)
	{
		Debug.Assert(_comparer.Equals(key, _getKeyFromItem(value)));

		Add(value);
	}
	void ICollection<KeyValuePair<K, T>>.Add(KeyValuePair<K, T> item)
	{
		Debug.Assert(_comparer.Equals(item.Key, _getKeyFromItem(item.Value)));

		Add(item.Value);
	}
	void IDictionary.Add(object key, object? value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		if (value == null)
		{
			if (default(T) != null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Add(default(T)!);
		}
		else if (value is T tvalue)
		{
			Add(tvalue);
		}
		else
		{
			throw new ArgumentException(nameof(value));
		}
	}


	/// <summary>
	/// Removes the value with the specified key from the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool Remove(K key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));

		if (_index != null && !_index.Remove(key))
		{
			return false;
		}

		var index = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), key));
		if (index < 0)
		{
			return false;
		}

		_list.RemoveAt(index);
		return true;
	}
	void IDictionary.Remove(object key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		Remove(tkey);
	}
	bool ICollection<KeyValuePair<K, T>>.Remove(KeyValuePair<K, T> item)
	{
		var key = _getKeyFromItem(item.Value) ?? throw new ArgumentNullException(nameof(item) + ".{Key}");

		Debug.Assert(_comparer.Equals(item.Key, key));

		var index = _list.IndexOf(item.Value);
		if (index < 0)
		{
			return false;
		}

		_index?.Remove(item.Key);
		_list.RemoveAt(index);
		return true;
	}


	bool ICollection<KeyValuePair<K, T>>.Contains(KeyValuePair<K, T> item)
	{
		return _index == null
			? _list.Contains(item.Value)
			: _index.Contains(item);
	}
	bool IDictionary.Contains(object key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		return ContainsKey(tkey);
	}


	public new IEnumerator<KeyValuePair<K, T>> GetEnumerator()
	{
		foreach (var x in _list)
		{
			yield return new KeyValuePair<K, T>(_getKeyFromItem(x), x);
		}
	}
	IEnumerator<KeyValuePair<K, T>> IEnumerable<KeyValuePair<K, T>>.GetEnumerator() => GetEnumerator();
	IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(GetEnumerator());
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


	void ICollection<KeyValuePair<K, T>>.CopyTo(KeyValuePair<K, T>[] array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
		if (array.Length - arrayIndex < _list.Count) throw new ArgumentException(nameof(array));

		foreach (var x in _list)
		{
			array[arrayIndex++] = new KeyValuePair<K, T>(_getKeyFromItem(x), x);
		}
	}
 
	/// <summary>
	/// Sets the capacity of this dictionary to hold up a specified number of entries without any further expansion of its backing storage.
	/// </summary>
	/// <param name="capacity">The new capacity.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">capacity is less than <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.</exception>
	public void TrimExcess(int capacity)
	{
		if (capacity < _list.Count) throw new ArgumentOutOfRangeException(nameof(capacity));

		_index?.TrimExcess(capacity);
		_list.Capacity = capacity;
	}


	sealed class DictionaryEnumerator : IDictionaryEnumerator, IDisposable
	{
		readonly IEnumerator<KeyValuePair<K, T>> _enumerator;

		public DictionaryEnumerator(IEnumerator<KeyValuePair<K, T>> enumerator) => _enumerator = enumerator;
		public DictionaryEntry Entry => new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);
		public object Key => _enumerator.Current.Key;
		public object? Value => _enumerator.Current.Value;
		public object Current => _enumerator.Current;

		public bool MoveNext() => _enumerator.MoveNext();
		public void Reset() => _enumerator.Reset();

		public void Dispose()
		{
			_enumerator.Dispose();
			GC.SuppressFinalize(this);
		}
	}

	sealed class ValueCollection : ICollection
	{
		public int Count => _dictionaryList.Count;
		public bool IsSynchronized => false;
		public object SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;

		private readonly DictionaryList<K, T> _dictionaryList;
		private object? _syncRoot;

		public ValueCollection(DictionaryList<K, T> dictionaryList) => _dictionaryList = dictionaryList;

		public void CopyTo(Array array, int index)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (array.Rank != 1) throw new ArgumentException(nameof(array));
			var lowerBound = array.GetLowerBound(0);
			if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
			{
				if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
				if (_dictionaryList._list.Count > 0) throw new ArgumentException(nameof(array));
				return;
			}
			if (array.Length - (index - lowerBound) < _dictionaryList._list.Count) throw new ArgumentException(nameof(array));

			foreach (var x in _dictionaryList._list)
			{
				array.SetValue(x, index++);
			}
		}

		public IEnumerator GetEnumerator() => _dictionaryList._list.GetEnumerator();
	}
}

internal sealed class DebugViewOfGenericDictionary<K, T> where K : notnull
{
	readonly IDictionary<K, T> _dictionary;

	public DebugViewOfGenericDictionary(IDictionary<K, T> dictionary)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		_dictionary = dictionary;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public KeyValuePair<K, T>[] Items
	{
		get
		{
			var count = Math.Min(_dictionary.Count, 100);
			var items = new KeyValuePair<K, T>[count];
			int i = 0;
			foreach (var item in ((IDictionary<K, T>)_dictionary))
			{
				items[i++] = item;
				
				if (i >= count)
				{
					break;
				}
			}
			return items;
		}
	}
}