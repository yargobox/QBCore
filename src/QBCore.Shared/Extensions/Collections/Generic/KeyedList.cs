using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace QBCore.Extensions.Collections.Generic;

/// <summary>
/// Represents a strongly typed list of objects that can be accessed by index and whose keys are embedded in the values.
/// Keys must be unique. When the number of elements becomes greater than or equal to the index creation threshold value, it builds a lookup dictionary based on these keys.
/// </summary>
/// <typeparam name="K">The type of keys in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</typeparam>
/// <typeparam name="T">The type of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</typeparam>
[DebuggerTypeProxy(typeof(DebugViewOfKeyedList<,>))]
[DebuggerDisplay("Count = {Count}")]
public class KeyedList<K, T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList where K : notnull
{
	/// <summary>
	/// A delegate to extract a key from the specified element.
	/// </summary>
	public Func<T, K> GetKeyFromItem => _getKeyFromItem;

	/// <summary>
	/// Gets the key-index of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> elements
	/// </summary>
	public IReadOnlyDictionary<K, T> Index => (IReadOnlyDictionary<K, T>?)_index ?? new ReadOnlyDictionaryAdapter(this);

	/// <summary>
	/// Gets a read only collection containing the keys
	/// </summary>
	public ReadOnlyKeyList Keys => new ReadOnlyKeyList(this);

	/// <summary>
	/// Gets the <see cref="System.Collections.Generic.IEqualityComparer{K}" /> that is used to determine equality of keys for the index.
	/// </summary>
	/// <returns>The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> generic interface implementation that is used to determine equality of keys for the current <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> and to provide hash values for the keys.</returns>
	public IEqualityComparer<K> Comparer => _comparer;

	/// <summary>
	/// The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.
	/// </summary>
	public int IndexCreationThreshold => _indexCreationThreshold;

	/// <summary>
	/// Gets the number of elements contained in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <returns>The number of elements contained in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	public int Count => _list.Count;
	
	/// <summary>
	/// Gets or sets the total number of elements the internal data structure can hold without resizing.
	/// </summary>
	/// <returns>The number of elements that the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> can contain before resizing is required.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">Capacity is set to a value that is less than Count.</exception>
	/// <exception cref="OutOfMemoryException">There is not enough memory available</exception>
	public int Capacity
	{
		get => _list.Capacity;
		set
		{
			if (_index != null)
			{
				if (_list.Capacity > value)
				{
					_index.TrimExcess(value);
				}
				else
				{
					_index.EnsureCapacity(value);
				}
			}
			_list.Capacity = value;
		}
	}

	bool IList.IsReadOnly => false;
	bool ICollection<T>.IsReadOnly => false;
	bool IList.IsFixedSize => false;
	bool ICollection.IsSynchronized => false;
	object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;

	/// <summary>
	/// The default number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.
	/// </summary>
	public const int DefaultIndexCreationThreshold = 8;

	protected readonly List<T> _list;
	protected readonly Func<T, K> _getKeyFromItem;
	protected readonly IEqualityComparer<K> _comparer;
	protected readonly int _indexCreationThreshold;
	protected Dictionary<K, T>? _index;



	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> class that is empty and has the default initial capacity.
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null</exception>
	public KeyedList(Func<T, K> getKeyFromItem)
	{
		if (getKeyFromItem == null) throw new ArgumentNullException(nameof(getKeyFromItem));

		_list = new List<T>();
		_getKeyFromItem = getKeyFromItem;
		_comparer = EqualityComparer<K>.Default;
		_indexCreationThreshold = DefaultIndexCreationThreshold;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> class that contains elements copied from
	/// the specified collection and has sufficient capacity to accommodate the number of elements copied.
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null -or- collection is null.</exception>
	public KeyedList(Func<T, K> getKeyFromItem, IEnumerable<T> collection)
	{
		if (getKeyFromItem == null) throw new ArgumentNullException(nameof(getKeyFromItem));
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		_list = new List<T>(collection);
		_getKeyFromItem = getKeyFromItem;
		_comparer = EqualityComparer<K>.Default;
		_indexCreationThreshold = DefaultIndexCreationThreshold;

		BuildIndexIfNeeded();
	}

	/// <summary>
	/// Initializes a new instance of the class that is empty, has the default initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public KeyedList(Func<T, K> getKeyFromItem, IEqualityComparer<K>? comparer, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (getKeyFromItem == null) throw new ArgumentNullException(nameof(getKeyFromItem));
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<T>();
		_getKeyFromItem = getKeyFromItem;
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> class that contains elements copied from
	/// the specified collection and has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null -or- collection is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public KeyedList(Func<T, K> getKeyFromItem, IEnumerable<T> collection, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (getKeyFromItem == null) throw new ArgumentNullException(nameof(getKeyFromItem));
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<T>(collection);
		_getKeyFromItem = getKeyFromItem;
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;

		BuildIndexIfNeeded();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="capacity">The number of elements that the new list can initially store.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">capacity is less than 0 -or- indexCreationThreshold is less than -1.</exception>
	public KeyedList(Func<T, K> getKeyFromItem, int capacity, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (getKeyFromItem == null) throw new ArgumentNullException(nameof(getKeyFromItem));
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<T>(capacity);
		_getKeyFromItem = getKeyFromItem;
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> class that ATTACHES the specified list and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="listToAttach">The list to attach (NOT COPY).</param>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">getKeyFromItem is null -or- listToAttach is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public KeyedList(List<T> listToAttach, Func<T, K> getKeyFromItem, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (getKeyFromItem == null) throw new ArgumentNullException(nameof(getKeyFromItem));
		if (listToAttach == null) throw new ArgumentNullException(nameof(listToAttach));
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = listToAttach;
		_getKeyFromItem = getKeyFromItem;
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;

		BuildIndexIfNeeded();
	}



	/// <summary>
	/// Gets or sets the element at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <returns>The element at the specified index.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</exception>
	/// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public T this[int index]
	{
		get => _list[index];
		set
		{
			if (index < 0 || index >= _list.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			var newKey = _getKeyFromItem(value) ?? throw new ArgumentNullException(nameof(value) + ".{Key}");
			var oldKey = _getKeyFromItem(_list[index]);

			if (!_comparer.Equals(newKey, oldKey))
			{
				if (_index == null)
				{
					var newIndex = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), newKey));
					if (newIndex >= 0)
					{
						throw new ArgumentException(nameof(value));
					}
				}
				else
				{
					_index.Add(newKey, value);
					_index.Remove(oldKey);
				}
			}

			_list[index] = value;
		}
	}
	object? IList.this[int index]
	{
		get => _list[index];
		set
		{
			if (value == null)
			{
				if (default(T) != null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_list[index] = default(T)!;
			}
			else if (value is T tvalue)
			{
				_list[index] = tvalue;
			}
			else
			{
				throw new ArgumentException(nameof(value));
			}
		}
	}



	/// <summary>
	/// Adds an object to the end of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="item">The object to be added to the end of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <exception cref="System.ArgumentNullException">An obtained key from the item is null.</exception>
	/// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public void Add(T item)
	{
		var key = _getKeyFromItem(item) ?? throw new ArgumentNullException(nameof(item) + ".{Key}");

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(_getKeyFromItem(x), key)))
			{
				throw new ArgumentException(nameof(item));
			}
		}
		else
		{
			_index.Add(key, item);
		}

		_list.Add(item);
	}
	
	int IList.Add(object? value)
	{
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

		return _list.Count - 1;
	}

	/// <summary>
	/// Attempts to add the specified key and value to the index.
	/// </summary>
	/// <param name="value">The value of the element to add. It can be null.</param>
	/// <returns>true if the key/value pair was added to the index successfully; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null</exception>
	public bool TryAdd(T value)
	{
		var key = _getKeyFromItem(value) ?? throw new ArgumentNullException(nameof(value) + ".{Key}");

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(_getKeyFromItem(x), key)))
			{
				return false;
			}
		}
		else
		{
			if (!_index.TryAdd(key, value))
			{
				return false;
			}
		}

		_list.Add(value);
		return true;
	}

	/// <summary>
	/// Adds the elements of the specified collection to the end of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="collection">the collection whose elements should be added to the end of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	public void AddRange(IEnumerable<T> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		foreach (var x in collection) Add(x);
	}



	/// <summary>
	/// Inserts an element into the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index at which item should be inserted.</param>
	/// <param name="item">The object to insert. The value can be null for reference types.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</exception>
	public void Insert(int index, T item)
	{
		if (index < 0 || index > _list.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		var key = _getKeyFromItem(item) ?? throw new ArgumentNullException(nameof(item) + ".{Key}");

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(_getKeyFromItem(x), key)))
			{
				throw new ArgumentException(nameof(item));
			}
		}
		else
		{
			_index.Add(key, item);
		}

		_list.Insert(index, item);
	}

	void IList.Insert(int index, object? value)
	{
		if (value == null)
		{
			if (default(T) != null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Insert(index, default(T)!);
		}
		else if (value is T tvalue)
		{
			Insert(index, tvalue);
		}
		else
		{
			throw new ArgumentException(nameof(value));
		}
	}

	/// <summary>
	/// Inserts the elements of a collection into the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
	/// <param name="collection">The collection whose elements should be inserted into the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</exception>
	public void InsertRange(int index, IEnumerable<T> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		foreach (var x in collection) Insert(index++, x);
	}



	/// <summary>
	/// Removes the first occurrence of a specific object from the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="item">The object to remove from the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	public bool Remove(T item)
	{
		var key = _getKeyFromItem(item) ?? throw new ArgumentNullException(nameof(item) + ".{Key}");

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

	void IList.Remove(object? value)
	{
		if (value == null && default(T) == null)
		{
			Remove(default(T)!);
		}
		else if (value is T tvalue)
		{
			Remove(tvalue);
		}
	}

	/// <summary>
	/// Removes the value with the specified key from the index, and copies the element to the value parameter.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <param name="value">The removed element.</param>
	/// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool Remove(K key, [MaybeNullWhen(false)] out T value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));

		if (_index != null && !_index.Remove(key))
		{
			value = default(T);
			return false;
		}

		var index = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), key));
		if (index < 0)
		{
			value = default(T);
			return false;
		}

		value = _list[index];
		_list.RemoveAt(index);
		return true;
	}

	/// <summary>
	/// Removes all the elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
	/// <returns>The number of elements removed from the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int RemoveAll(Predicate<T> match)
	{
		if (_index != null)
		{
			if (match == null) throw new ArgumentNullException(nameof(match));

			var callRemoveAll = false;
			foreach (var x in _list)
			{
				if (match(x))
				{
					callRemoveAll = true;
					_index.Remove(_getKeyFromItem(x));
				}
			}

			if (!callRemoveAll) return 0;
		}

		return _list.RemoveAll(match);
	}

	/// <summary>
	/// Removes a range of elements from the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
	/// <param name="count">The number of elements to remove.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public void RemoveRange(int index, int count)
	{
		if (_index != null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if ((count += index) > _list.Count) throw new ArgumentException(nameof(count));

			for (int i = index; i < count; i++)
			{
				_index.Remove(_getKeyFromItem(_list[i]));
			}
		}

		_list.RemoveRange(index, count);
	}

	/// <summary>
	/// Removes the element at the specified index of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</exception>
	public void RemoveAt(int index)
	{
		_index?.Remove(_getKeyFromItem(_list[index]));
		_list.RemoveAt(index);
	}

	/// <summary>
	/// Removes all elements from the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	public void Clear()
	{
		_index?.Clear();
		_list.Clear();
	}



	/// <summary>
	/// Returns an enumerator that iterates through the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Enumerator for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)_list).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_list).GetEnumerator();
	}



	/// <summary>
	/// Determines whether an element is in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <returns>true if item is found in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />; otherwise, false.</returns>
	public bool Contains(T item)
	{
		var key = _getKeyFromItem(item) ?? throw new ArgumentNullException(nameof(item) + ".{Key}");

		if (_index == null)
		{
			var index = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), key));
			if (index < 0)
			{
				return false;
			}

			return EqualityComparer<T>.Default.Equals(_list[index], item);
		}
		else
		{
			T? value;
			if (!_index.TryGetValue(key, out value))
			{
				return false;
			}

			return EqualityComparer<T>.Default.Equals(value, item);
		}
	}

	bool IList.Contains(object? value)
	{
		if (value == null && default(T) == null)
		{
			return Contains(default(T)!);
		}
		else if (value is T tvalue)
		{
			return Contains(tvalue);
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Determines whether the index contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the index.</param>
	/// <returns>true if the index contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool ContainsKey(K key)
	{
		if (_index == null)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			return _list.Exists(x => _comparer.Equals(_getKeyFromItem(x), key));
		}

		return _index.ContainsKey(key);
	}



	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the first occurrence of item within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />, if found; otherwise, -1.</returns>
	public int IndexOf(T item)
	{
		return _list.IndexOf(item);
	}

	int IList.IndexOf(object? value)
	{
		if (value == null && default(T) == null)
		{
			return IndexOf(default(T)!);
		}
		else if (value is T tvalue)
		{
			return IndexOf(tvalue);
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
	/// <returns>true if the index contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool TryGetValue(K key, [MaybeNullWhen(false)] out T value)
	{
		if (_index == null)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			var index = _list.FindIndex(x => _comparer.Equals(_getKeyFromItem(x), key));
			if (index < 0)
			{
				value = default(T);
				return false;
			}

			value = _list[index];
			return true;
		}

		return _index.TryGetValue(key, out value);
	}
	


	/// <summary>
	/// Copies the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> to a compatible one-dimensional array, starting at the specified index of the target array.
	/// </summary>
	/// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The Array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <exception cref="System.ArgumentNullException">array is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
	/// <exception cref="System.ArgumentException">The number of elements in the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> is greater than the available space from arrayIndex to the end of the destination array.</exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		_list.CopyTo(array, arrayIndex);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_list).CopyTo(array, index);
	}

	/// <summary>
	/// Copies the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> to a compatible one-dimensional array, starting at the beginning of the target array.
	/// </summary>
	/// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The Array must have zero-based indexing.</param>
	/// <exception cref="System.ArgumentNullException">array is null.</exception>
	/// <exception cref="System.ArgumentException">The number of elements in the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> is greater than the number of elements that the destination array can contain.</exception>
	public void CopyTo(T[] array) => _list.CopyTo(array);

	/// <summary>
	/// Copies a range of elements from the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> to a compatible one-dimensional array, starting at the specified index of the target array.
	/// </summary>
	/// <param name="index">The zero-based index in the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> at which copying begins.</param>
	/// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The Array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <param name="count">The number of elements to copy.</param>
	/// <exception cref="System.ArgumentNullException">array is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- arrayIndex is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index is equal to or greater than the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count of the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. -or- The number of elements from index to the end of the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> is greater than the available space from arrayIndex to the end of the destination array.</exception>
	public void CopyTo(int index, T[] array, int arrayIndex, int count) => _list.CopyTo(index, array, arrayIndex, count);



	/// <summary>
	/// Copies the elements of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> to a new array.
	/// </summary>
	/// <returns>An array containing copies of the elements of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	public T[] ToArray() => _list.ToArray();

	/// <summary>
	/// Creates a shallow copy of a range of elements in the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="index">The zero-based <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> index at which the range starts.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <returns>A shallow copy of a range of elements in the source <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public List<T> GetRange(int index, int count) => _list.GetRange(index, count);

	/// <summary>
	/// Returns a read-only <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}" /> wrapper for the current collection.
	/// </summary>
	/// <returns>An object that acts as a read-only wrapper around the current <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	public ReadOnlyCollection<T> AsReadOnly() => _list.AsReadOnly();

	/// <summary>
	/// Converts the elements in the current <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> to another type, and returns a list containing the converted elements.
	/// </summary>
	/// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
	/// <param name="converter">A <see cref="System.Converter{T, TOutput}"/> delegate that converts each element from one type to another type.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> of the target type containing the converted elements from the current <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">converter is null.</exception>
	public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => _list.ConvertAll<TOutput>(converter);



	/// <summary>
	/// Ensures that the capacity of this list is at least the specified capacity. If
	/// the current capacity is less than capacity, it is successively increased to twice
	/// the current capacity until it is at least the specified capacity.
	/// </summary>
	/// <param name="capacity">The minimum capacity to ensure.</param>
	/// <returns>The new capacity of this list.</returns>
	public int EnsureCapacity(int capacity)
	{
		_index?.EnsureCapacity(capacity);
		return _list.EnsureCapacity(capacity);
	}

	/// <summary>
	/// Sets the capacity to the actual number of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />, if that number is less than a threshold value.
	/// </summary>
	public void TrimExcess()
	{
		_index?.TrimExcess();
		_list.TrimExcess();
	}



	/// <summary>
	/// Searches a range of elements in the sorted <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> for an element using the specified comparer and returns the zero-based index of the element.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to search.</param>
	/// <param name="count">The length of the range to search.</param>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" />.</param>
	/// <returns>The zero-based index of item in the sorted <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	public int BinarySearch(int index, int count, T item, IComparer<T>? comparer) => _list.BinarySearch(index, count, item, comparer);

	/// <summary>
	/// Searches the entire sorted <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> for an element using the default comparer and returns the zero-based index of the element.
	/// </summary>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <returns>The zero-based index of item in the sorted <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</returns>
	/// <exception cref="InvalidOperationException">The default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	public int BinarySearch(T item) => _list.BinarySearch(item);

	/// <summary>
	/// Searches the entire sorted <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> for an element using the specified comparer and returns the zero-based index of the element.
	/// </summary>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements. -or- null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" />.</param>
	/// <returns>The zero-based index of item in the sorted <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.Count.</returns>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or <see cref="System.IComparable" /> interface for type T.</exception>
	public int BinarySearch(T item, IComparer<T>? comparer) => _list.BinarySearch(item, comparer);

	/// <summary>
	/// Determines whether the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> contains elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public bool Exists(Predicate<T> match) => _list.Exists(match);
	
	/// <summary>
	/// Determines whether every element in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> matches the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions to check against the elements.</param>
	/// <returns>true if every element in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public bool TrueForAll(Predicate<T> match) => _list.TrueForAll(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public T? Find(Predicate<T> match) => _list.Find(match);

	/// <summary>
	/// Retrieves all the elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public List<T> FindAll(Predicate<T> match) => _list.FindAll(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that starts at the specified index and contains the specified number of elements.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. -or- count is less than 0. -or- startIndex and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int FindIndex(int startIndex, int count, Predicate<T> match) => _list.FindIndex(startIndex, count, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int FindIndex(int startIndex, Predicate<T> match) => _list.FindIndex(startIndex, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int FindIndex(Predicate<T> match) => _list.FindIndex(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public T? FindLast(Predicate<T> match) => _list.FindLast(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that contains the specified number of elements and ends at the specified index.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. -or- count is less than 0. -or- startIndex and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int FindLastIndex(int startIndex, int count, Predicate<T> match) => _list.FindLastIndex(startIndex, count, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that extends from the first element to the specified index.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int FindLastIndex(int startIndex, Predicate<T> match) => _list.FindLastIndex(startIndex, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int FindLastIndex(Predicate<T> match) => _list.FindLastIndex(match);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that extends from index to the last element, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int IndexOf(T item, int index) => _list.IndexOf(item, index);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that starts at the specified index and contains the specified number of elements.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that starts at index and contains count number of elements, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. -or- count is less than 0. -or- index and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int IndexOf(T item, int index, int count) => _list.IndexOf(item, index, count);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the last occurrence of item within the entire the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />, if found; otherwise, -1.</returns>
	public int LastIndexOf(T item) => _list.LastIndexOf(item);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that extends from the first element to the specified index.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that extends from the first element to index, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int LastIndexOf(T item, int index) => _list.LastIndexOf(item, index);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last
	/// occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />
	/// that contains the specified number of elements and ends at the specified index.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <returns>
	/// The zero-based index of the last occurrence of item within the range of elements
	/// in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that contains count number of elements
	/// and ends at index, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. -or- count is less than 0. -or- index and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public int LastIndexOf(T item, int index, int count) => _list.LastIndexOf(item, index, count);
	


	/// <summary>
	/// Reverses the order of the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	public void Reverse() => _list.Reverse();

	/// <summary>
	/// Reverses the order of the elements in the specified range.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to reverse.</param>
	/// <param name="count">The number of elements in the range to reverse.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</exception>
	public void Reverse(int index, int count) => _list.Reverse(index, count);

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> using the specified <see cref="System.Comparison{T}" />.
	/// </summary>
	/// <param name="comparison">The <see cref="System.Comparison{T}" /> to use when comparing elements.</param>
	/// <exception cref="System.ArgumentNullException">comparison is null.</exception>
	/// <exception cref="System.ArgumentException">The implementation of comparison caused an error during the sort. For example, comparison might not return 0 when comparing an item with itself.</exception>
	public void Sort(Comparison<T> comparison) => _list.Sort(comparison);

	/// <summary>
	/// Sorts the elements in a range of elements in <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> using the specified comparer.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to sort.</param>
	/// <param name="count">The length of the range to sort.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" />.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not specify a valid range in the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />. -or- The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	public void Sort(int index, int count, IComparer<T>? comparer) => _list.Sort(index, count, comparer);

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> using the default comparer.
	/// </summary>
	/// <exception cref="InvalidOperationException">The default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	public void Sort() => _list.Sort();

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> using the specified comparer.
	/// </summary>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" />.</param>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	/// <exception cref="System.ArgumentException">The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
	public void Sort(IComparer<T>? comparer) => _list.Sort(comparer);



	/// <summary>
	/// Performs the specified action on each element of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.
	/// </summary>
	/// <param name="action"> The action delegate to perform on each element of the <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</param>
	/// <exception cref="System.ArgumentNullException">action is null.</exception>
	/// <exception cref="InvalidOperationException">An element in the collection has been modified.</exception>
	public void ForEach(Action<T> action) => _list.ForEach(action);



	protected void BuildIndexIfNeeded()
	{
		if (_indexCreationThreshold >= 0 && _index == null && _list.Count >= _indexCreationThreshold)
		{
			_index = new Dictionary<K, T>(_list.Capacity, _comparer);
			foreach (var x in _list)
			{
				_index.Add(_getKeyFromItem(x), x);
			}
		}
	}



	protected sealed class ReadOnlyDictionaryAdapter : IReadOnlyDictionary<K, T>
	{
		public IEnumerable<K> Keys => _keyedList._list.Select(x => _keyedList._getKeyFromItem(x));
		public IEnumerable<T> Values => _keyedList._list;
		public int Count => _keyedList._list.Count;
		
		private readonly KeyedList<K, T> _keyedList;

		public ReadOnlyDictionaryAdapter(KeyedList<K, T> keyedList) => _keyedList = keyedList;

		public T this[K key]
		{
			get
			{
				if (_keyedList._index != null)
				{
					return _keyedList._index[key];
				}
				else
				{
					if (key == null) throw new ArgumentNullException(nameof(key));

					var comparer = _keyedList._comparer;
					var getKeyFromItem = _keyedList._getKeyFromItem;
					var index = _keyedList._list.FindIndex(x => comparer.Equals(getKeyFromItem(x), key));
					if (index < 0)
					{
						throw new KeyNotFoundException();
					}

					return _keyedList._list[index];
				}
			}
		}

		public bool ContainsKey(K key)
		{
			if (_keyedList._index != null)
			{
				return _keyedList._index.ContainsKey(key);
			}
			else
			{
				if (key == null) throw new ArgumentNullException(nameof(key));

				var comparer = _keyedList._comparer;
				var getKeyFromItem = _keyedList._getKeyFromItem;
				var index = _keyedList._list.FindIndex(x => comparer.Equals(getKeyFromItem(x), key));
				return index >= 0;
			}
		}

		public IEnumerator<KeyValuePair<K, T>> GetEnumerator()
		{
			var getKeyFromItem = _keyedList._getKeyFromItem;
			foreach (var x in _keyedList._list)
			{
				yield return new KeyValuePair<K, T>(getKeyFromItem(x), x);
			}
		}

		public bool TryGetValue(K key, [MaybeNullWhen(false)] out T value)
		{
			if (_keyedList._index != null)
			{
				return _keyedList._index.TryGetValue(key, out value);
			}
			else
			{
				if (key == null) throw new ArgumentNullException(nameof(key));

				var comparer = _keyedList._comparer;
				var getKeyFromItem = _keyedList._getKeyFromItem;
				var index = _keyedList._list.FindIndex(x => comparer.Equals(getKeyFromItem(x), key));
				if (index < 0)
				{
					value = default(T);
					return false;
				}

				value = _keyedList._list[index];
				return true;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}



	[DebuggerTypeProxy(typeof(DebugViewOfKeyedListReadOnlyKeyList<,>))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class ReadOnlyKeyList : ICollection<K>, IEnumerable<K>, IEnumerable, IList<K>, IReadOnlyCollection<K>, IReadOnlyList<K>, ICollection, IList
	{
		public int Count => _keyedList.Count;
		public bool IsReadOnly => true;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;
		bool IList.IsFixedSize => false;

		private readonly KeyedList<K, T> _keyedList;
		private object? _syncRoot;

		public ReadOnlyKeyList(KeyedList<K, T> keyedList) => _keyedList = keyedList;

		public K this[int index] => _keyedList._getKeyFromItem(_keyedList[index]);
		K IList<K>.this[int index]
		{
			get => this[index];
			set => throw new NotSupportedException();
		}
		object? IList.this[int index]
		{
			get => this[index];
			set => throw new NotSupportedException();
		}

		void ICollection<K>.Add(K item) => throw new NotSupportedException();
		int IList.Add(object? value) => throw new NotSupportedException();
		void IList<K>.Insert(int index, K item) => throw new NotSupportedException();
		void IList.Insert(int index, object? value) => throw new NotSupportedException();
		void ICollection<K>.Clear() => throw new NotSupportedException();
		void IList.Clear() => throw new NotSupportedException();
		bool ICollection<K>.Remove(K item) => throw new NotSupportedException();
		void IList.Remove(object? value) => throw new NotSupportedException();
		void IList<K>.RemoveAt(int index) => throw new NotSupportedException();
		void IList.RemoveAt(int index) => throw new NotSupportedException();

		public bool Contains(K item) => _keyedList.ContainsKey(item);
		bool IList.Contains(object? value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is not K tkey) throw new ArgumentException(nameof(value));

			return _keyedList.ContainsKey(tkey);
		}

		public int IndexOf(K item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var comparer = _keyedList._comparer;
			var getKeyFromItem = _keyedList._getKeyFromItem;
			return _keyedList._list.FindIndex(x => comparer.Equals(getKeyFromItem(x), item));
		}
		int IList.IndexOf(object? value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is not K tkey) throw new ArgumentException(nameof(value));

			var comparer = _keyedList._comparer;
			var getKeyFromItem = _keyedList._getKeyFromItem;
			return _keyedList._list.FindIndex(x => comparer.Equals(getKeyFromItem(x), tkey));
		}

		public void CopyTo(K[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (array.Length - arrayIndex < _keyedList.Count) throw new ArgumentException(nameof(array));

			foreach (var x in _keyedList._list)
			{
				array[arrayIndex++] = _keyedList._getKeyFromItem(x);
			}
		}
		public void CopyTo(Array array, int index)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (array.Rank != 1) throw new ArgumentException(nameof(array));
			var lowerBound = array.GetLowerBound(0);
			if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
			{
				if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
				if (_keyedList.Count > 0) throw new ArgumentException(nameof(array));
				return;
			}
			if (array.Length - (index - lowerBound) < _keyedList.Count) throw new ArgumentException(nameof(array));

			foreach (var x in _keyedList._list)
			{
				array.SetValue(_keyedList._getKeyFromItem(x), index++);
			}
		}

		public IEnumerator<K> GetEnumerator()
		{
			foreach (var item in _keyedList._list)
			{
				yield return _keyedList._getKeyFromItem(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

internal sealed class DebugViewOfKeyedList<K, T> where K : notnull
{
	readonly KeyedList<K, T> _keyedList;

	public DebugViewOfKeyedList(KeyedList<K, T> keyedList)
	{
		if (keyedList == null) throw new ArgumentNullException(nameof(keyedList));

		_keyedList = keyedList;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Items
	{
		get
		{
			var count = Math.Min(_keyedList.Count, 100);
			var items = new T[count];
			int i = 0;
			foreach (var item in _keyedList)
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

internal sealed class DebugViewOfKeyedListReadOnlyKeyList<K, T> where K : notnull
{
	readonly ICollection<K> _collection;

	public DebugViewOfKeyedListReadOnlyKeyList(ICollection<K> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		_collection = collection;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public K[] Items
	{
		get
		{
			var count = Math.Min(_collection.Count, 100);
			var items = new K[count];
			int i = 0;
			foreach (var item in _collection)
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

public static class ExtensionsForKeyedList
{
	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that contains elements copied from the specified collection.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> that contains elements from the input sequence.</returns>
	public static KeyedList<K, T> ToKeyedList<K, T>(this IEnumerable<T> collection, Func<T, K> getKeyFromItem) where K : notnull
	{
		if (collection is KeyedList<K, T> keyedList)
		{
			return new KeyedList<K, T>(getKeyFromItem, collection, keyedList.Comparer, keyedList.IndexCreationThreshold);
		}
		else
		{
			return new KeyedList<K, T>(getKeyFromItem, collection);
		}
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> from a <see cref="System.Collections.Generic.List{T}" />.
	/// You can attach a list to a keyed list to avoid copying, but you shouldn't use it after that.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="list">The list whose elements are copied to the new keyed list -or- the list to attach to the new keyed list..</param>
	/// <param name="attach">Specifies whether to attach or copy the list.</param>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	public static KeyedList<K, T> ToKeyedList<K, T>(this List<T> list, bool attach, Func<T, K> getKeyFromItem) where K : notnull
	{
		return attach
			? new KeyedList<K, T>(list, getKeyFromItem)
			: new KeyedList<K, T>(getKeyFromItem, list);
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> copy.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="keyedList">The <see cref="QBCore.Extensions.Collections.Generic.KeyedList{K, T}" /> list whose elements are copied to the new one.</param>
	/// <exception cref="System.ArgumentNullException">keyedList is null.</exception>
	public static KeyedList<K, T> ToKeyedList<K, T>(this KeyedList<K, T> keyedList) where K : notnull
	{
		if (keyedList == null) throw new ArgumentNullException(nameof(keyedList));

		return new KeyedList<K, T>(keyedList.GetKeyFromItem, keyedList, keyedList.Comparer, keyedList.IndexCreationThreshold);
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> that contains elements copied from the specified collection.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> that contains elements from the input sequence.</returns>
	public static DictionaryList<K, T> ToDictionaryList<K, T>(this IEnumerable<T> collection, Func<T, K> getKeyFromItem) where K : notnull
	{
		if (collection is KeyedList<K, T> keyedList)
		{
			return new DictionaryList<K, T>(getKeyFromItem, collection, keyedList.Comparer, keyedList.IndexCreationThreshold);
		}
		else
		{
			return new DictionaryList<K, T>(getKeyFromItem, collection);
		}
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> from a <see cref="System.Collections.Generic.List{T}" />.
	/// You can attach a list to a dictionary list to avoid copying, but you shouldn't use it after that.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="list">The list whose elements are copied to the new dictionary list -or- the list to attach to the new dictionary list.</param>
	/// <param name="attach">Specifies whether to attach or copy the list.</param>
	/// <param name="getKeyFromItem">The delegate to extract a key from the specified element.</param>
	public static DictionaryList<K, T> ToDictionaryList<K, T>(this List<T> list, bool attach, Func<T, K> getKeyFromItem) where K : notnull
	{
		return attach
			? new DictionaryList<K, T>(list, getKeyFromItem)
			: new DictionaryList<K, T>(getKeyFromItem, list);
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> copy.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="dictionaryList">The <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> whose elements are copied to the new one.</param>
	/// <exception cref="System.ArgumentNullException">dictionaryList is null.</exception>
	public static DictionaryList<K, T> ToDictionaryList<K, T>(this DictionaryList<K, T> dictionaryList) where K : notnull
	{
		if (dictionaryList == null) throw new ArgumentNullException(nameof(dictionaryList));

		return new DictionaryList<K, T>(dictionaryList.GetKeyFromItem, dictionaryList, dictionaryList.Comparer, dictionaryList.IndexCreationThreshold);
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that contains elements copied from the specified collection.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="collection">The collection whose elements are copied to the new <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that contains elements from the input sequence.</returns>
	public static OrderedDictionary<K, T> ToOrderedDictionary<K, T>(this IEnumerable<KeyValuePair<K, T>> collection) where K : notnull
	{
		if (collection is OrderedDictionary<K, T> orderedDictionary)
		{
			return new OrderedDictionary<K, T>(collection, orderedDictionary.Comparer, orderedDictionary.IndexCreationThreshold);
		}
		else
		{
			return new OrderedDictionary<K, T>(collection);
		}
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> from a <see cref="System.Collections.Generic.List{KeyValuePair{K, T}}" />.
	/// You can attach a list to a dictionary list to avoid copying, but you shouldn't use it after that.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="list">The list whose elements are copied to the new dictionary list -or- the list to attach to the new dictionary list.</param>
	/// <param name="attach">Specifies whether to attach or copy the list.</param>
	public static OrderedDictionary<K, T> ToOrderedDictionary<K, T>(this List<KeyValuePair<K, T>> list, bool attach) where K : notnull
	{
		return new OrderedDictionary<K, T>(list, attach);
	}

	/// <summary>
	/// Creates a <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> copy.
	/// </summary>
	/// <typeparam name="K">The type of keys in <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" />.</typeparam>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="orderedDictionary">The <see cref="QBCore.Extensions.Collections.Generic.DictionaryList{K, T}" /> whose elements are copied to the new one.</param>
	/// <exception cref="System.ArgumentNullException">orderedDictionary is null.</exception>
	public static OrderedDictionary<K, T> ToOrderedDictionary<K, T>(this OrderedDictionary<K, T> orderedDictionary) where K : notnull
	{
		if (orderedDictionary == null) throw new ArgumentNullException(nameof(orderedDictionary));

		return new OrderedDictionary<K, T>(orderedDictionary, orderedDictionary.Comparer, orderedDictionary.IndexCreationThreshold);
	}
}