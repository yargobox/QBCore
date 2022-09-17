using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace QBCore.Extensions.Collections.Generic;

[DebuggerTypeProxy(typeof(DebugViewOfGenericDictionary<,>))]
[DebuggerDisplay("Count = {Count}")]
public class OrderedDictionary<K, T> : ICollection<KeyValuePair<K, T>>, IEnumerable<KeyValuePair<K, T>>, IEnumerable, IDictionary<K, T>, IReadOnlyCollection<KeyValuePair<K, T>>, IReadOnlyDictionary<K, T>, ICollection, IDictionary where K : notnull
{
	/// <summary>
	/// Gets the number of key/value pairs contained in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <returns>The number of key/value pairs contained in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	public int Count => _list.Count;

	/// <summary>
	///  Gets the <see cref="System.Collections.Generic.IEqualityComparer{K}" /> that is used to determine equality of keys for the dictionary.
	/// </summary>
	/// <returns>The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> generic interface implementation that is used to determine equality of keys for the current <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> and to provide hash values for the keys.</returns>
	public IEqualityComparer<K> Comparer => _comparer;

	/// <summary>
	/// The number of elements the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.
	/// </summary>
	public int IndexCreationThreshold => _indexCreationThreshold;

	/// <summary>
	/// Answers the question whether the index (a lookup dictionary) has been created.
	/// </summary>
	/// <returns>true if the index has been created; otherwise, false.</returns>
	public bool IsIndexCreated => _index != null;

	bool ICollection<KeyValuePair<K, T>>.IsReadOnly => false;
	bool IDictionary.IsReadOnly => false;
	bool ICollection.IsSynchronized => false;
	bool IDictionary.IsFixedSize => false;
	object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;

	/// <summary>
	/// Gets a collection containing the keys in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.ReadOnlyKeyList containing the keys in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	public ReadOnlyKeyList Keys => new ReadOnlyKeyList(this);
	ICollection<K> IDictionary<K, T>.Keys => new ReadOnlyKeyList(this);
	IEnumerable<K> IReadOnlyDictionary<K, T>.Keys => new ReadOnlyKeyList(this);
	ICollection IDictionary.Keys => new ReadOnlyKeyList(this);

	/// <summary>
	/// Gets a collection containing the values in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.ReadOnlyValueList containing the values in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	public ReadOnlyValueList Values => new ReadOnlyValueList(this);
	ICollection<T> IDictionary<K, T>.Values => new ReadOnlyValueList(this);
	IEnumerable<T> IReadOnlyDictionary<K, T>.Values => new ReadOnlyValueList(this);
	ICollection IDictionary.Values => new ReadOnlyValueList(this);

	/// <summary>
	/// The default number of elements the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> can hold without creating a lookup dictionary.
	/// </summary>
	public const int DefaultIndexCreationThreshold = 8;

	protected readonly List<KeyValuePair<K, T>> _list;
	protected readonly IEqualityComparer<K> _comparer;
	protected readonly int _indexCreationThreshold;
	protected Dictionary<K, T>? _index;



	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />class that is empty and has the default initial capacity.
	/// </summary>
	public OrderedDictionary()
	{
		_list = new List<KeyValuePair<K, T>>();
		_comparer = EqualityComparer<K>.Default;
		_indexCreationThreshold = DefaultIndexCreationThreshold;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> class that is empty, has the default initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public OrderedDictionary(IEqualityComparer<K>? comparer, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<KeyValuePair<K, T>>();
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> class that contains elements copied from
	/// the specified collection and has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public OrderedDictionary(IEnumerable<KeyValuePair<K, T>> collection, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<KeyValuePair<K, T>>(collection);
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;

		BuildIndexIfNeeded();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> class that contains elements copied from
	/// the specified dictionary and has sufficient capacity to accommodate the number of elements copied, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="dictionary">The dictionary whose elements are copied to the new dictionary.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">dictionary is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public OrderedDictionary(IDictionary<K, T> dictionary, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<KeyValuePair<K, T>>(dictionary.Count);
		_list.AddRange(dictionary);
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;

		BuildIndexIfNeeded();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="capacity">The number of elements that the new dictionary can initially store.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">capacity is less than 0 -or- indexCreationThreshold is less than -1.</exception>
	public OrderedDictionary(int capacity, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = new List<KeyValuePair<K, T>>(capacity);
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> class that can ATTACH the specified list and uses the specified <see cref="System.Collections.Generic.IEqualityComparer{K}" />
	/// </summary>
	/// <param name="listToAttach">The list to attach or copy.</param>
	/// <param name="attach">Specifies whether to attach or copy elements from the list.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{K}" /> implementation to use when comparing keys, or null to use the default <see cref="System.Collections.Generic.EqualityComparer{K}" /> for the type of the key</param>
	/// <param name="indexCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary
	/// (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
	/// <exception cref="System.ArgumentNullException">listToAttach is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">indexCreationThreshold is less than -1.</exception>
	public OrderedDictionary(List<KeyValuePair<K, T>> listToAttach, bool attach, IEqualityComparer<K>? comparer = null, int indexCreationThreshold = DefaultIndexCreationThreshold)
	{
		if (listToAttach == null) throw new ArgumentNullException(nameof(listToAttach));
		if (indexCreationThreshold < -1) throw new ArgumentOutOfRangeException(nameof(indexCreationThreshold));

		_list = attach ? listToAttach : new List<KeyValuePair<K, T>>(listToAttach);
		_comparer = comparer ?? EqualityComparer<K>.Default;
		_indexCreationThreshold = indexCreationThreshold;

		BuildIndexIfNeeded();
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
			if (_index == null)
			{
				if (key == null) throw new ArgumentNullException(nameof(key));

				var index = _list.FindIndex(x => _comparer.Equals(x.Key, key));
				if (index < 0)
				{
					throw new KeyNotFoundException();
				}

				return _list[index].Value;
			}

			return _index[key];
		}
		set
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			var index = _list.FindIndex(x => _comparer.Equals(x.Key, key));
			if (index < 0)
			{
				BuildIndexIfNeeded();

				if (_index != null)
				{
					_index.Add(key, value);
				}

				_list.Add(new KeyValuePair<K, T>(key, value));
			}
			else
			{
				if (_index != null)
				{
					_index[key] = value;
				}

				_list[index] = new KeyValuePair<K, T>(key, value);
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

	/// <summary>
	/// Gets the element value at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>The element value at the specified index.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</exception>
	public T At(int index)
	{
		return _list[index].Value;
	}

	/// <summary>
	/// Sets the element value at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to set.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</exception>
	public void At(int index, T value)
	{
		var key = _list[index].Key;
		_list[index] = new KeyValuePair<K, T>(key, value);
	}



	/// <summary>
	/// Adds the specified key and value to the dictionary.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	/// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public void Add(K key, T value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(x.Key, key)))
			{
				throw new ArgumentException(nameof(key));
			}
		}
		else
		{
			_index.Add(key, value);
		}

		_list.Add(new KeyValuePair<K, T>(key, value));
	}

	void ICollection<KeyValuePair<K, T>>.Add(KeyValuePair<K, T> item)
	{
		if (item.Key == null) throw new ArgumentNullException(nameof(item) + ".Key");

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(x.Key, item.Key)))
			{
				throw new ArgumentException(nameof(item) + ".Key");
			}
		}
		else
		{
			_index.Add(item.Key, item.Value);
		}

		_list.Add(item);
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
	/// Attempts to add the specified key and value to the index.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. It can be null.</param>
	/// <returns>true if the key/value pair was added to the index successfully; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null</exception>
	public bool TryAdd(K key, T value)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(x.Key, key)))
			{
				return false;
			}
		}
		else
		{
			_index.Add(key, value);
		}

		_list.Add(new KeyValuePair<K, T>(key, value));
		return true;
	}

	/// <summary>
	/// Adds the elements of the specified collection to the end of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="collection">the collection whose elements should be added to the end of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	public void AddRange(IEnumerable<KeyValuePair<K, T>> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		foreach (var x in collection) Add(x.Key, x.Value);
	}



	/// <summary>
	/// Inserts an element into the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index at which item should be inserted.</param>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="item">The object to insert. The value can be null for reference types.</param>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</exception>
	/// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public void Insert(int index, K key, T item)
	{
		if (index < 0 || index > _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
		if (key == null) throw new ArgumentNullException(nameof(key));

		BuildIndexIfNeeded();

		if (_index == null)
		{
			if (_list.Exists(x => _comparer.Equals(x.Key, key)))
			{
				throw new ArgumentException(nameof(key));
			}
		}
		else
		{
			_index.Add(key, item);
		}

		_list.Insert(index, new KeyValuePair<K, T>(key, item));
	}

	/// <summary>
	/// Inserts the elements of a collection into the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
	/// <param name="collection">The collection whose elements should be inserted into the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
	/// <exception cref="System.ArgumentNullException">collection is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</exception>
	public void InsertRange(int index, IEnumerable<KeyValuePair<K, T>> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		foreach (var x in collection) Insert(index++, x.Key, x.Value);
	}



	/// <summary>
	/// Removes the value with the specified key from the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool Remove(K key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));

		if (_index != null && !_index.Remove(key))
		{
			return false;
		}

		var index = _list.FindIndex(x => _comparer.Equals(x.Key, key));
		if (index < 0)
		{
			return false;
		}

		_list.RemoveAt(index);
		return true;
	}

	bool ICollection<KeyValuePair<K, T>>.Remove(KeyValuePair<K, T> item)
	{
		if (item.Key == null) throw new ArgumentNullException(nameof(item) + ".Key");

		if (_index == null)
		{
			var index = _list.FindIndex(x => _comparer.Equals(x.Key, item.Key) && EqualityComparer<T>.Default.Equals(x.Value, item.Value));
			if (index >= 0)
			{
				_list.RemoveAt(index);
				return true;
			}
		}
		else
		{
			T? value;
			if (_index.TryGetValue(item.Key, out value) && EqualityComparer<T>.Default.Equals(value, item.Value))
			{
				_index.Remove(item.Key);

				var index = _list.FindIndex(x => _comparer.Equals(x.Key, item.Key));
				if (index >= 0)
				{
					_list.RemoveAt(index);
					return true;
				}
			}
		}

		return false;
	}

	void IDictionary.Remove(object key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		Remove(tkey);
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

		var index = _list.FindIndex(x => _comparer.Equals(x.Key, key));
		if (index < 0)
		{
			value = default(T);
			return false;
		}

		value = _list[index].Value;
		_list.RemoveAt(index);
		return true;
	}

	/// <summary>
	/// Removes all the elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
	/// <returns>The number of elements removed from the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int RemoveAll(Predicate<T> match)
	{
		if (_index != null)
		{
			if (match == null) throw new ArgumentNullException(nameof(match));

			var callRemoveAll = false;
			foreach (var x in _list)
			{
				if (match(x.Value))
				{
					callRemoveAll = true;
					_index.Remove(x.Key);
				}
			}

			if (!callRemoveAll) return 0;
		}

		return _list.RemoveAll(x => match(x.Value));
	}

	/// <summary>
	/// Removes all the elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the elements to remove.</param>
	/// <returns>The number of elements removed from the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int RemoveAll(Predicate<KeyValuePair<K, T>> match)
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
					_index.Remove(x.Key);
				}
			}

			if (!callRemoveAll) return 0;
		}

		return _list.RemoveAll(match);
	}

	/// <summary>
	/// Removes a range of elements from the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
	/// <param name="count">The number of elements to remove.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public void RemoveRange(int index, int count)
	{
		if (_index != null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

			count += index;
			if (count > _list.Count) throw new ArgumentException(nameof(count));

			for (int i = index; i < count; i++)
			{
				_index.Remove(_list[i].Key);
			}
		}

		_list.RemoveRange(index, count);
	}

	/// <summary>
	/// Removes the element at the specified index of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</exception>
	public void RemoveAt(int index)
	{
		_index?.Remove(_list[index].Key);
		_list.RemoveAt(index);
	}

	/// <summary>
	/// Removes all elements from the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	public void Clear()
	{
		_index?.Clear();
		_list.Clear();
	}



	/// <summary>
	/// Determines whether the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool ContainsKey(K key)
	{
		if (_index == null)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			return _list.Exists(x => _comparer.Equals(x.Key, key));
		}

		return _index.ContainsKey(key);
	}

	bool ICollection<KeyValuePair<K, T>>.Contains(KeyValuePair<K, T> item)
	{
		if (_index == null)
		{
			if (item.Key == null) throw new ArgumentNullException(nameof(item) + ".Key");

			return _list.Exists(x => _comparer.Equals(x.Key, item.Key) && EqualityComparer<T>.Default.Equals(x.Value, item.Value));
		}

		T? value;
		return _index.TryGetValue(item.Key, out value) && EqualityComparer<T>.Default.Equals(value, item.Value);
	}

	bool IDictionary.Contains(object key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (key is not K tkey) throw new ArgumentException(nameof(key));

		return ContainsKey(tkey);
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">key is null.</exception>
	public bool TryGetValue(K key, [MaybeNullWhen(false)] out T value)
	{
		if (_index == null)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			var index = _list.FindIndex(x => _comparer.Equals(x.Key, key));
			if (index < 0)
			{
				value = default(T);
				return false;
			}

			value = _list[index].Value;
			return true;
		}

		return _index.TryGetValue(key, out value);
	}

	/// <summary>
	/// Determines whether an element is in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <returns>true if item is found in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />; otherwise, false.</returns>
	public bool Contains(T item)
	{
		var valueComparer = EqualityComparer<T>.Default;
		return _list.Exists(x => valueComparer.Equals(x.Value, item));
	}



	/// <summary>
	/// Copies the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> to a compatible one-dimensional array, starting at the specified index of the target array.
	/// </summary>
	/// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The Array must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <exception cref="System.ArgumentNullException">array is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
	/// <exception cref="System.ArgumentException">The number of elements in the source <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> is greater than the available space from arrayIndex to the end of the destination array.</exception>
	public void CopyTo(KeyValuePair<K, T>[] array, int arrayIndex)
	{
		_list.CopyTo(array, arrayIndex);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (array.Rank != 1) throw new ArgumentException(nameof(array));
		var lowerBound = array.GetLowerBound(0);
		if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
		if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
		{
			if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (_list.Count > 0) throw new ArgumentException(nameof(array));
			return;
		}
		if (array.Length - (index - lowerBound) < _list.Count) throw new ArgumentException(nameof(array));

		foreach (var x in _list)
		{
			array.SetValue(x, index++);
		}
	}



	/// <summary>
	/// Returns an enumerator that iterates through the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Enumerator for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	public IEnumerator<KeyValuePair<K, T>> GetEnumerator() => _list.GetEnumerator();
	
	IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
	
	IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(_list.GetEnumerator());



	/// <summary>
	/// Copies the elements of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> to a new array.
	/// </summary>
	/// <returns>An array containing copies of the elements of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	public KeyValuePair<K, T>[] ToArray() => _list.ToArray();

	/// <summary>
	/// Creates a shallow copy of a range of elements in the source <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="index">The zero-based <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> index at which the range starts.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <returns>A shallow copy of a range of elements in the source <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public List<KeyValuePair<K, T>> GetRange(int index, int count) => _list.GetRange(index, count);

	/// <summary>
	/// Returns a read-only <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}" /> wrapper for the current collection.
	/// </summary>
	/// <returns>An object that acts as a read-only wrapper around the current <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	public ReadOnlyCollection<KeyValuePair<K, T>> AsReadOnly() => _list.AsReadOnly();

	/// <summary>
	/// Converts the elements in the current <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> to another type, and returns a list containing the converted elements.
	/// </summary>
	/// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
	/// <param name="converter">A <see cref="System.Converter{T, TOutput}"/> delegate that converts each element from one type to another type.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> of the target type containing the converted elements from the current <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">converter is null.</exception>
	public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => _list.ConvertAll<TOutput>(x => converter(x.Value));

	/// <summary>
	/// Converts the elements in the current <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> to another type, and returns a list containing the converted elements.
	/// </summary>
	/// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
	/// <param name="converter">A <see cref="System.Converter{KeyValuePair{K, T}, TOutput}"/> delegate that converts each element from one type to another type.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> of the target type containing the converted elements from the current <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">converter is null.</exception>
	public List<TOutput> ConvertAll<TOutput>(Converter<KeyValuePair<K, T>, TOutput> converter) => _list.ConvertAll<TOutput>(converter);



	/// <summary>
	/// Ensures that the capacity of this dictionary is at least the specified capacity. If
	/// the current capacity is less than capacity, it is successively increased to twice
	/// the current capacity until it is at least the specified capacity.
	/// </summary>
	/// <param name="capacity">The minimum capacity to ensure.</param>
	/// <returns>The new capacity of this dictionary.</returns>
	public int EnsureCapacity(int capacity)
	{
		_index?.EnsureCapacity(capacity);
		return _list.EnsureCapacity(capacity);
	}

	/// <summary>
	/// Sets the capacity to the actual number of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />, if that number is less than a threshold value.
	/// </summary>
	public void TrimExcess()
	{
		_index?.TrimExcess();
		_list.TrimExcess();
	}



	/// <summary>
	/// Searches a range of elements in the sorted <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> for an element using the specified comparer and returns the zero-based index of the element.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to search.</param>
	/// <param name="count">The length of the range to search.</param>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" />.</param>
	/// <returns>The zero-based index of item in the sorted <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> generic interface or the <see cref="System.IComparable" /> interface for type <see cref="System.Collections.Generic.KeyValuePair{K, T}" />.</exception>
	public int BinarySearch(int index, int count, KeyValuePair<K, T> item, IComparer<KeyValuePair<K, T>>? comparer) => _list.BinarySearch(index, count, item, comparer);

	/// <summary>
	/// Searches the entire sorted <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> for an element using the default comparer and returns the zero-based index of the element.
	/// </summary>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <returns>The zero-based index of item in the sorted <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</returns>
	/// <exception cref="InvalidOperationException">The default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> generic interface or the <see cref="System.IComparable" /> interface for type <see cref="System.Collections.Generic.KeyValuePair{K, T}" />.</exception>
	public int BinarySearch(KeyValuePair<K, T> item) => _list.BinarySearch(item);

	/// <summary>
	/// Searches the entire sorted <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> for an element using the specified comparer and returns the zero-based index of the element.
	/// </summary>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> implementation to use when comparing elements. -or- null to use the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" />.</param>
	/// <returns>The zero-based index of item in the sorted <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.Count.</returns>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> generic interface or <see cref="System.IComparable" /> interface for type <see cref="System.Collections.Generic.KeyValuePair{K, T}" />.</exception>
	public int BinarySearch(KeyValuePair<K, T> item, IComparer<KeyValuePair<K, T>>? comparer) => _list.BinarySearch(item, comparer);

	/// <summary>
	/// Determines whether the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public bool Exists(Predicate<T> match) => _list.Exists(x => match(x.Value));

	/// <summary>
	/// Determines whether the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the elements to search for.</param>
	/// <returns>true if the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public bool Exists(Predicate<KeyValuePair<K, T>> match) => _list.Exists(match);
	
	/// <summary>
	/// Determines whether every element in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> matches the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions to check against the elements.</param>
	/// <returns>true if every element in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public bool TrueForAll(Predicate<T> match) => _list.TrueForAll(x => match(x.Value));

	/// <summary>
	/// Determines whether every element in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> matches the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions to check against the elements.</param>
	/// <returns>true if every element in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public bool TrueForAll(Predicate<KeyValuePair<K, T>> match) => _list.TrueForAll(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public T? Find(Predicate<T> match)
	{
		if (match == null) throw new ArgumentNullException(nameof(match));

		var index = _list.FindIndex(x => match(x.Value));
		return index < 0 ? default(T?) : _list[index].Value;
	}

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public KeyValuePair<K, T>? Find(Predicate<KeyValuePair<K, T>> match) => _list.Find(match);

	/// <summary>
	/// Retrieves all the elements that match the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the elements to search for.</param>
	/// <returns>A <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public List<KeyValuePair<K, T>> FindAll(Predicate<KeyValuePair<K, T>> match) => _list.FindAll(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that starts at the specified index and contains the specified number of elements.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- startIndex and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindIndex(int startIndex, int count, Predicate<T> match) => _list.FindIndex(startIndex, count, x => match(x.Value));

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that starts at the specified index and contains the specified number of elements.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- startIndex and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindIndex(int startIndex, int count, Predicate<KeyValuePair<K, T>> match) => _list.FindIndex(startIndex, count, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindIndex(int startIndex, Predicate<T> match) => _list.FindIndex(startIndex, x => match(x.Value));

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindIndex(int startIndex, Predicate<KeyValuePair<K, T>> match) => _list.FindIndex(startIndex, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int FindIndex(Predicate<T> match) => _list.FindIndex(x => match(x.Value));

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int FindIndex(Predicate<KeyValuePair<K, T>> match) => _list.FindIndex(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public T? FindLast(Predicate<T> match)
	{
		if (match == null) throw new ArgumentNullException(nameof(match));

		var index = _list.FindLastIndex(x => match(x.Value));
		return index < 0 ? default(T?) : _list[index].Value;
	}

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public KeyValuePair<K, T>? FindLast(Predicate<KeyValuePair<K, T>> match) => _list.FindLast(match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that contains the specified number of elements and ends at the specified index.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- startIndex and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindLastIndex(int startIndex, int count, Predicate<T> match) => _list.FindLastIndex(startIndex, count, x => match(x.Value));

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that contains the specified number of elements and ends at the specified index.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- startIndex and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindLastIndex(int startIndex, int count, Predicate<KeyValuePair<K, T>> match) => _list.FindLastIndex(startIndex, count, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the first element to the specified index.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindLastIndex(int startIndex, Predicate<T> match) => _list.FindLastIndex(startIndex, x => match(x.Value));

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the first element to the specified index.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	/// <exception cref="System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int FindLastIndex(int startIndex, Predicate<KeyValuePair<K, T>> match) => _list.FindLastIndex(startIndex, match);

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int FindLastIndex(Predicate<T> match) => _list.FindLastIndex(x => match(x.Value));

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="match">The <see cref="System.Predicate{KeyValuePair{K, T}}" /> delegate that defines the conditions of the element to search for.</param>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentNullException">match is null.</exception>
	public int FindLastIndex(Predicate<KeyValuePair<K, T>> match) => _list.FindLastIndex(match);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from index to the last element, if found; otherwise, -1.</returns>
	public int IndexOf(T item) => _list.FindIndex(x => EqualityComparer<T>.Default.Equals(x.Value, item));

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from index to the last element, if found; otherwise, -1.</returns>
	public int IndexOf(KeyValuePair<K, T> item) => _list.IndexOf(item);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from index to the last element, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int IndexOf(T item, int index) => _list.FindIndex(index, x => EqualityComparer<T>.Default.Equals(x.Value, item));

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the specified index to the last element.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from index to the last element, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int IndexOf(KeyValuePair<K, T> item, int index) => _list.IndexOf(item, index);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that starts at the specified index and contains the specified number of elements.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that starts at index and contains count number of elements, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- index and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int IndexOf(T item, int index, int count)
	{
		var valueComparer = EqualityComparer<T>.Default;
		return _list.FindIndex(index, count, x => valueComparer.Equals(x.Value, item));
	}

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that starts at the specified index and contains the specified number of elements.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that starts at index and contains count number of elements, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- index and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int IndexOf(KeyValuePair<K, T> item, int index, int count) => _list.IndexOf(item, index, count);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the last occurrence of item within the entire the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />, if found; otherwise, -1.</returns>
	public int LastIndexOf(T item)
	{
		var valueComparer = EqualityComparer<T>.Default;
		return _list.FindLastIndex(x => valueComparer.Equals(x.Value, item));
	}

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the last occurrence of item within the entire the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />, if found; otherwise, -1.</returns>
	public int LastIndexOf(KeyValuePair<K, T> item) => _list.LastIndexOf(item);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the first element to the specified index.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the first element to index, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int LastIndexOf(T item, int index)
	{
		var valueComparer = EqualityComparer<T>.Default;
		return _list.FindLastIndex(index, x => valueComparer.Equals(x.Value, item));
	}

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the first element to the specified index.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that extends from the first element to index, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int LastIndexOf(KeyValuePair<K, T> item, int index) => _list.LastIndexOf(item, index);

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last
	/// occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />
	/// that contains the specified number of elements and ends at the specified index.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <returns>
	/// The zero-based index of the last occurrence of item within the range of elements
	/// in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that contains count number of elements
	/// and ends at index, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- index and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int LastIndexOf(T item, int index, int count)
	{
		var valueComparer = EqualityComparer<T>.Default;
		return _list.FindLastIndex(index, count, x => valueComparer.Equals(x.Value, item));
	}

	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the last
	/// occurrence within the range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />
	/// that contains the specified number of elements and ends at the specified index.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. The value can be null for reference types.</param>
	/// <param name="index">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <returns>
	/// The zero-based index of the last occurrence of item within the range of elements
	/// in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> that contains count number of elements
	/// and ends at index, if found; otherwise, -1.</returns>
	/// <exception cref="System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- count is less than 0. -or- index and count do not specify a valid section in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public int LastIndexOf(KeyValuePair<K, T> item, int index, int count) => _list.LastIndexOf(item, index, count);



	/// <summary>
	/// Reverses the order of the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	public void Reverse() => _list.Reverse();

	/// <summary>
	/// Reverses the order of the elements in the specified range.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to reverse.</param>
	/// <param name="count">The number of elements in the range to reverse.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</exception>
	public void Reverse(int index, int count) => _list.Reverse(index, count);

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the specified <see cref="System.Comparison{T}" />.
	/// </summary>
	/// <param name="comparison">The <see cref="System.Comparison{T}" /> to use when comparing elements.</param>
	/// <exception cref="System.ArgumentNullException">comparison is null.</exception>
	/// <exception cref="System.ArgumentException">The implementation of comparison caused an error during the sort. For example, comparison might not return 0 when comparing an item with itself.</exception>
	public void Sort(Comparison<T> comparison) => _list.Sort((a, b) => comparison(a.Value, b.Value));

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the specified <see cref="System.Comparison{KeyValuePair{K, T}}" />.
	/// </summary>
	/// <param name="comparison">The <see cref="System.Comparison{KeyValuePair{K, T}}" /> to use when comparing elements.</param>
	/// <exception cref="System.ArgumentNullException">comparison is null.</exception>
	/// <exception cref="System.ArgumentException">The implementation of comparison caused an error during the sort. For example, comparison might not return 0 when comparing an item with itself.</exception>
	public void Sort(Comparison<KeyValuePair<K, T>> comparison) => _list.Sort(comparison);

	/// <summary>
	/// Sorts the elements in a range of elements in <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the specified comparer.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to sort.</param>
	/// <param name="count">The length of the range to sort.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" />.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not specify a valid range in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	public void Sort(int index, int count, IComparer<T>? comparer) => _list.Sort(index, count, new ComparerAdapter(comparer));

	/// <summary>
	/// Sorts the elements in a range of elements in <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the specified comparer.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to sort.</param>
	/// <param name="count">The length of the range to sort.</param>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" />.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
	/// <exception cref="System.ArgumentException">index and count do not specify a valid range in the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />. -or- The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" /> cannot find implementation of the <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> generic interface or the <see cref="System.IComparable" /> interface for type <see cref="System.Collections.Generic.KeyValuePair{K, T}" />.</exception>
	public void Sort(int index, int count, IComparer<KeyValuePair<K, T>>? comparer) => _list.Sort(index, count, comparer);

	/// <summary>
	/// Sorts the elements by value in the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the default comparer.
	/// </summary>
	/// <exception cref="InvalidOperationException">The default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find an implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	public void Sort() => _list.Sort(new ComparerAdapter());

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the specified comparer.
	/// </summary>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" />.</param>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default" /> cannot find implementation of the <see cref="System.Collections.Generic.IComparer{T}" /> generic interface or the <see cref="System.IComparable" /> interface for type T.</exception>
	/// <exception cref="System.ArgumentException">The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
	public void Sort(IComparer<T>? comparer) => _list.Sort(new ComparerAdapter(comparer));

	/// <summary>
	/// Sorts the elements in the entire <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" /> using the specified comparer.
	/// </summary>
	/// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" />.</param>
	/// <exception cref="InvalidOperationException">comparer is null, and the default comparer <see cref="System.Collections.Generic.Comparer{KeyValuePair{K, T}}.Default" /> cannot find implementation of the <see cref="System.Collections.Generic.IComparer{KeyValuePair{K, T}}" /> generic interface or the <see cref="System.IComparable" /> interface for type <see cref="System.Collections.Generic.KeyValuePair{K, T}" />.</exception>
	/// <exception cref="System.ArgumentException">The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an item with itself.</exception>
	public void Sort(IComparer<KeyValuePair<K, T>>? comparer) => _list.Sort(comparer);



	/// <summary>
	/// Performs the specified action on each element of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="action">The action delegate to perform on each element of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</param>
	/// <exception cref="System.ArgumentNullException">action is null.</exception>
	/// <exception cref="InvalidOperationException">An element in the collection has been modified.</exception>
	public void ForEach(Action<T> action) => _list.ForEach(x => action(x.Value));

	/// <summary>
	/// Performs the specified action on each element of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.
	/// </summary>
	/// <param name="action">The action delegate to perform on each element of the <see cref="QBCore.Extensions.Collections.Generic.OrderedDictionary{K, T}" />.</param>
	/// <exception cref="System.ArgumentNullException">action is null.</exception>
	/// <exception cref="InvalidOperationException">An element in the collection has been modified.</exception>
	public void ForEach(Action<KeyValuePair<K, T>> action) => _list.ForEach(action);



	protected void BuildIndexIfNeeded()
	{
		if (_index == null && _indexCreationThreshold >= 0 && _list.Count >= _indexCreationThreshold)
		{
			_index = new Dictionary<K, T>(_list.Capacity, _comparer);
			foreach (var x in _list)
			{
				_index.Add(x.Key, x.Value);
			}
		}
	}



	private readonly struct ComparerAdapter : IComparer<KeyValuePair<K, T>>
	{
		public readonly Comparison<KeyValuePair<K, T>> Comparison;

		public ComparerAdapter()
		{
			var comparer = Comparer<T>.Default;

			Comparison = (x, y) => comparer.Compare(x.Value, y.Value);
		}

		public ComparerAdapter(IComparer<T>? comparer)
		{
			comparer ??= Comparer<T>.Default;

			Comparison = (x, y) => comparer.Compare(x.Value, y.Value);
		}

		public readonly int Compare(KeyValuePair<K, T> x, KeyValuePair<K, T> y) => Comparison(x, y);
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



	[DebuggerTypeProxy(typeof(DebugViewOfOrderedDictionaryReadOnlyKeyList<,>))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class ReadOnlyKeyList : ICollection<K>, IEnumerable<K>, IEnumerable, IList<K>, IReadOnlyCollection<K>, IReadOnlyList<K>, ICollection, IList
	{
		public int Count => _orderedDictionary.Count;
		public bool IsReadOnly => true;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;
		bool IList.IsFixedSize => false;

		private readonly OrderedDictionary<K, T> _orderedDictionary;
		private object? _syncRoot;

		public ReadOnlyKeyList(OrderedDictionary<K, T> orderedDictionary) => _orderedDictionary = orderedDictionary;

		public K this[int index] => _orderedDictionary._list[index].Key;
		K IList<K>.this[int index]
		{
			get => _orderedDictionary._list[index].Key;
			set => throw new NotSupportedException();
		}
		object? IList.this[int index]
		{
			get => _orderedDictionary._list[index].Key;
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

		public bool Contains(K item) => _orderedDictionary.ContainsKey(item);
		bool IList.Contains(object? value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is not K tkey) throw new ArgumentException(nameof(value));

			return _orderedDictionary.ContainsKey(tkey);
		}

		public int IndexOf(K item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var comparer = _orderedDictionary._comparer;
			return _orderedDictionary._list.FindIndex(x => comparer.Equals(x.Key, item));
		}
		int IList.IndexOf(object? value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is not K tkey) throw new ArgumentException(nameof(value));

			var comparer = _orderedDictionary._comparer;
			return _orderedDictionary._list.FindIndex(x => comparer.Equals(x.Key, tkey));
		}

		/// <summary>
		/// Copies elements to an array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The array that is the destination of the copied elements.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
		/// <exception cref="System.ArgumentException">The number of elements in the source is greater than the available space from arrayIndex to the end of the destination array.</exception>
		public void CopyTo(K[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (array.Length - arrayIndex < _orderedDictionary.Count) throw new ArgumentException(nameof(arrayIndex));

			foreach (var x in _orderedDictionary._list)
			{
				array[arrayIndex++] = x.Key;
			}
		}

		/// <summary>
		/// Copies the entire range of elements to a compatible one-dimensional array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination of the copied elements. The Array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
		/// <exception cref="System.ArgumentException">The number of elements in the source is greater than the available space from arrayIndex to the end of the destination array.</exception>
		public void CopyTo(Array array, int index)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (array.Rank != 1) throw new ArgumentException(nameof(array));
			var lowerBound = array.GetLowerBound(0);
			if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
			{
				if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
				if (_orderedDictionary.Count > 0) throw new ArgumentException(nameof(array));
				return;
			}
			if (array.Length - (index - lowerBound) < _orderedDictionary.Count) throw new ArgumentException();

			foreach (var x in _orderedDictionary._list)
			{
				array.SetValue(x.Key, index++);
			}
		}

		/// <summary>
		/// Copies a range of elements to an array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="index">The zero-based index in the source at which copying begins.</param>
		/// <param name="array">The array that is the destination of the copied elements.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <param name="count">The number of elements to copy.</param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- arrayIndex is less than 0. -or- count is less than 0.</exception>
		/// <exception cref="System.ArgumentException">index is equal to or greater than the Count of the source. -or- The number of elements from index to the end of the source is greater than the available space from arrayIndex to the end of the destination array.</exception>
		public void CopyTo(int index, K[] array, int arrayIndex, int count)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (array.Length - arrayIndex < count) throw new ArgumentException(nameof(arrayIndex));

			count += index;
			if (count > _orderedDictionary._list.Count) throw new ArgumentException(nameof(count));

			for (; index < count; index++)
			{
				array[arrayIndex++] = _orderedDictionary._list[index].Key;
			}
		}

		public IEnumerator<K> GetEnumerator()
		{
			foreach (var item in _orderedDictionary._list)
			{
				yield return item.Key;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}


	[DebuggerTypeProxy(typeof(DebugViewOfOrderedDictionaryReadOnlyValueList<,>))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class ReadOnlyValueList : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
	{
		public int Count => _orderedDictionary.Count;
		public bool IsReadOnly => true;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => _syncRoot ?? Interlocked.CompareExchange(ref _syncRoot, new object(), null) ?? _syncRoot;
		bool IList.IsFixedSize => false;

		private readonly OrderedDictionary<K, T> _orderedDictionary;
		private object? _syncRoot;

		public ReadOnlyValueList(OrderedDictionary<K, T> orderedDictionary) => _orderedDictionary = orderedDictionary;

		public T this[int index] => _orderedDictionary._list[index].Value;
		T IList<T>.this[int index]
		{
			get => _orderedDictionary._list[index].Value;
			set => throw new NotSupportedException();
		}
		object? IList.this[int index]
		{
			get => _orderedDictionary._list[index].Value;
			set => throw new NotSupportedException();
		}

		void ICollection<T>.Add(T item) => throw new NotSupportedException();
		int IList.Add(object? value) => throw new NotSupportedException();
		void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
		void IList.Insert(int index, object? value) => throw new NotSupportedException();
		void ICollection<T>.Clear() => throw new NotSupportedException();
		void IList.Clear() => throw new NotSupportedException();
		bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
		void IList.Remove(object? value) => throw new NotSupportedException();
		void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
		void IList.RemoveAt(int index) => throw new NotSupportedException();

		public bool Contains(T item)
		{
			var valueComparer = EqualityComparer<T>.Default;
			return _orderedDictionary._list.Exists(x => valueComparer.Equals(x.Value, item));
		}
		bool IList.Contains(object? value)
		{
			if (value == null)
			{
				if (default(T) != null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				var valueComparer = EqualityComparer<T>.Default;
				return _orderedDictionary._list.Exists(x => valueComparer.Equals(x.Value, default(T)));
			}
			else if (value is T tvalue)
			{
				var valueComparer = EqualityComparer<T>.Default;
				return _orderedDictionary._list.Exists(x => valueComparer.Equals(x.Value, tvalue));
			}

			throw new ArgumentException(nameof(value));
		}

		public int IndexOf(T item)
		{
			var valueComparer = EqualityComparer<T>.Default;
			return _orderedDictionary._list.FindIndex(x => valueComparer.Equals(x.Value, item));
		}
		int IList.IndexOf(object? value)
		{
			if (value == null)
			{
				if (default(T) != null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				var valueComparer = EqualityComparer<T>.Default;
				return _orderedDictionary._list.FindIndex(x => valueComparer.Equals(x.Value, default(T)));
			}
			else if (value is T tvalue)
			{
				var valueComparer = EqualityComparer<T>.Default;
				return _orderedDictionary._list.FindIndex(x => valueComparer.Equals(x.Value, tvalue));
			}

			throw new ArgumentException(nameof(value));
		}

		/// <summary>
		/// Copies elements to an array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The array that is the destination of the copied elements.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
		/// <exception cref="System.ArgumentException">The number of elements in the source is greater than the available space from arrayIndex to the end of the destination array.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (array.Length - arrayIndex < _orderedDictionary.Count) throw new ArgumentException(nameof(arrayIndex));

			foreach (var x in _orderedDictionary._list)
			{
				array[arrayIndex++] = x.Value;
			}
		}

		/// <summary>
		/// Copies the entire range of elements to a compatible one-dimensional array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination of the copied elements. The Array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
		/// <exception cref="System.ArgumentException">The number of elements in the source is greater than the available space from arrayIndex to the end of the destination array.</exception>
		public void CopyTo(Array array, int index)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (array.Rank != 1) throw new ArgumentException(nameof(array));
			var lowerBound = array.GetLowerBound(0);
			if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
			{
				if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
				if (_orderedDictionary.Count > 0) throw new ArgumentException(nameof(array));
				return;
			}
			if (array.Length - (index - lowerBound) < _orderedDictionary.Count) throw new ArgumentException();

			foreach (var x in _orderedDictionary._list)
			{
				array.SetValue(x.Value, index++);
			}
		}

		/// <summary>
		/// Copies a range of elements to an array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="index">The zero-based index in the source at which copying begins.</param>
		/// <param name="array">The array that is the destination of the copied elements.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <param name="count">The number of elements to copy.</param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- arrayIndex is less than 0. -or- count is less than 0.</exception>
		/// <exception cref="System.ArgumentException">index is equal to or greater than the Count of the source. -or- The number of elements from index to the end of the source is greater than the available space from arrayIndex to the end of the destination array.</exception>
		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (array.Length - arrayIndex < count) throw new ArgumentException(nameof(arrayIndex));

			count += index;
			if (count > _orderedDictionary._list.Count) throw new ArgumentException(nameof(count));

			for (; index < count; index++)
			{
				array[arrayIndex++] = _orderedDictionary._list[index].Value;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach (var item in _orderedDictionary._list)
			{
				yield return item.Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

internal sealed class DebugViewOfOrderedDictionaryReadOnlyKeyList<K, T>
{
	readonly ICollection<K> _collection;

	public DebugViewOfOrderedDictionaryReadOnlyKeyList(ICollection<K> collection)
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

internal sealed class DebugViewOfOrderedDictionaryReadOnlyValueList<K, T>
{
	readonly ICollection<T> _collection;

	public DebugViewOfOrderedDictionaryReadOnlyValueList(ICollection<T> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));

		_collection = collection;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Items
	{
		get
		{
			var count = Math.Min(_collection.Count, 100);
			var items = new T[count];
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
