using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace QBCore.Extensions.Collections.Generic;

[DebuggerTypeProxy(typeof(DebugViewOfDictionary<,>))]
[DebuggerDisplay("Count = {Count}")]
public class OrderedDictionary<TKey, TValue> :
	ICollection<KeyValuePair<TKey, TValue>>,
	IEnumerable<KeyValuePair<TKey, TValue>>,
	IEnumerable,
	IDictionary<TKey, TValue>,
	IOrderedDictionary<TKey, TValue>,
	IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
	IReadOnlyDictionary<TKey, TValue>,
	ICollection,
	IDictionary
	where TKey : notnull
{
	readonly Dictionary<TKey, int> _index;
	readonly List<KeyValuePair<TKey, TValue>> _list;
	object? _syncRoot = null;

	public OrderedDictionary()
	{
		_list = new List<KeyValuePair<TKey, TValue>>();
		_index = new Dictionary<TKey, int>();
	}
	public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
	{
		_list = dictionary.ToList();
		_index = new Dictionary<TKey, int>(_list.Count);
		int i = 0;
		foreach (var p in _list) _index.Add(p.Key, i++);
	}
	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
	{
		_list = collection.ToList();
		_index = new Dictionary<TKey, int>(_list.Count);
		int i = 0;
		foreach (var p in _list) _index.Add(p.Key, i++);
	}
	public OrderedDictionary(IEqualityComparer<TKey>? comparer)
	{
		_list = new List<KeyValuePair<TKey, TValue>>();
		_index = new Dictionary<TKey, int>(comparer);
	}
	public OrderedDictionary(int capacity)
	{
		_list = new List<KeyValuePair<TKey, TValue>>(capacity);
		_index = new Dictionary<TKey, int>(capacity);
	}
	public OrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer)
	{
		_list = dictionary.ToList();
		_index = new Dictionary<TKey, int>(_list.Count, comparer);
		int i = 0;
		foreach (var p in _list) _index.Add(p.Key, i++);
	}
	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
	{
		_list = collection.ToList();
		_index = new Dictionary<TKey, int>(_list.Count, comparer);
		int i = 0;
		foreach (var p in _list) _index.Add(p.Key, i++);
	}
	public OrderedDictionary(int capacity, IEqualityComparer<TKey>? comparer)
	{
		_list = new List<KeyValuePair<TKey, TValue>>(capacity);
		_index = new Dictionary<TKey, int>(comparer);
	}

	public TValue this[TKey key] { get => _list[_index[key]].Value; set => _Set(key, value); }

	public object? this[object key]
	{
		get
		{
			if (!IsCompatibleKey(key)) throw new ArgumentException(nameof(key));
			return _list[_index[(TKey)key]].Value;
		}
		set
		{
			if (!IsCompatibleKey(key)) throw new ArgumentException(nameof(key));
			if (!IsCompatibleValue(value)) throw new ArgumentException(nameof(value));
			_Set((TKey)key, (TValue)value!);
		}
	}

	TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => _list[_index[key]].Value;

	public ICollection<TKey> Keys => new KeyCollection(this);

	public ICollection<TValue> Values => new ValueCollection(this);

	public int Count => _list.Count;

	public bool IsReadOnly => false;

	public bool IsFixedSize => false;

	public bool IsSynchronized => false;

	public object SyncRoot
	{
		get
		{
			if (_syncRoot == null) System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot!, new Object(), null!);
			return _syncRoot;
		}
	}

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => new KeyCollection(this);

	ICollection IDictionary.Keys => new KeyCollection(this);

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => new ValueCollection(this);

	ICollection IDictionary.Values => new ValueCollection(this);

	public void Add(TKey key, TValue value) => _Add(new KeyValuePair<TKey, TValue>(key, value));

	public void Add(KeyValuePair<TKey, TValue> item) => _Add(item);

	public void Add(object key, object? value)
	{
		if (!IsCompatibleKey(key)) throw new ArgumentException(nameof(key));
		if (!IsCompatibleValue(value)) throw new ArgumentException(nameof(value));
		_Add(new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value!));
	}

	public void Clear()
	{
		_list.Clear();
		_index.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		int index;
		return _index.TryGetValue(item.Key, out index) && EqualityComparer<TValue>.Default.Equals(_list[index].Value, item.Value);
	}

	public bool Contains(object key) => IsCompatibleKey(key) && _index.ContainsKey((TKey)key);

	public bool ContainsKey(TKey key) => _index.ContainsKey(key);

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

	public void CopyTo(Array array, int index)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (array.Rank != 1) throw new ArgumentException(nameof(array));
		var lowerBound = array.GetLowerBound(0);
		if (index < lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
		if (array.GetUpperBound(0) - lowerBound + 1 <= 0)
		{
			if (index > lowerBound) throw new ArgumentOutOfRangeException(nameof(index));
			if (_list.Count > 0) throw new ArgumentException();
			return;
		}
		if (array.Length - (index - lowerBound) < _list.Count) throw new ArgumentException();

		foreach (var p in _list) array.SetValue(p, index++);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();

	public IReadOnlyList<KeyValuePair<TKey, TValue>> List => _list;

	public int IndexOf(TKey key) => _index[key];

	public void Insert(int index, TKey key, TValue value) => _Insert(index, new KeyValuePair<TKey, TValue>(key, value));

	public TKey KeyOf(int index) => _list[index].Key;

	public bool Remove(TKey key) => _RemoveByKey(key);

	public bool Remove(KeyValuePair<TKey, TValue> item) => _RemoveByKey(item.Key);

	public void Remove(object key)
	{
		if (!IsCompatibleKey(key)) throw new ArgumentException(nameof(key));
		_RemoveByKey((TKey)key);
	}

	public void RemoveAt(int index) => _RemoveByIndex(index);

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		int index;
		if (_index.TryGetValue(key, out index))
		{
			value = _list[index].Value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(_list.GetEnumerator());

	IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

	public void Sort()
	{
		_list.Sort();
		for (int i = 0; i < _list.Count; i++) _index[_list[i].Key] = i;
	}

	public void Sort(Comparison<KeyValuePair<TKey, TValue>> comparer)
	{
		_list.Sort(comparer);
		for (int i = 0; i < _list.Count; i++) _index[_list[i].Key] = i;
	}

	public void Sort(IComparer<KeyValuePair<TKey, TValue>>? comparer)
	{
		_list.Sort(comparer);
		for (int i = 0; i < _list.Count; i++) _index[_list[i].Key] = i;
	}

	public void Sort(int index, int count, IComparer<KeyValuePair<TKey, TValue>>? comparer)
	{
		_list.Sort(index, count, comparer);
		count += index;
		for (int i = index; i < count; i++) _index[_list[i].Key] = i;
	}


	bool _RemoveByKey(TKey key)
	{
		int index;
		if (_index.Remove(key, out index))
		{
			_list.RemoveAt(index);
			return true;
		}
		return false;
	}
	void _RemoveByIndex(int index)
	{
		_index.Remove(_list[index].Key);
		_list.RemoveAt(index);
	}
	bool _ContainsKey(TKey key) => _index.ContainsKey(key);
	void _Set(TKey key, TValue value)
	{
		int index;
		if (_index.TryGetValue(key, out index))
		{
			_list[index] = new KeyValuePair<TKey, TValue>(key, value);
		}
		else
		{
			_index.Add(key, _list.Count);
			_list.Add(new KeyValuePair<TKey, TValue>(key, value));
		}

	}
	void _Insert(int index, KeyValuePair<TKey, TValue> item)
	{
		_list.Insert(index, item);
		_index.Add(item.Key, index);
	}
	void _Add(KeyValuePair<TKey, TValue> item)
	{
		_index.Add(item.Key, _list.Count);
		_list.Add(item);
	}

	static bool IsCompatibleKey(object key) => key is TKey;
	static bool IsCompatibleValue(object? value) => ((value is TValue) || (value == null && default(TValue) == null));

	[DebuggerTypeProxy(typeof(DebugViewOfDictionaryKeyCollection<,>))]
	[DebuggerDisplay("Count = {Count}")]
	sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection
	{
		readonly OrderedDictionary<TKey, TValue> _dictionary;

		public KeyCollection(OrderedDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

		public int Count => _dictionary.Count;

		public bool IsReadOnly => true;

		public bool IsSynchronized => false;

		public object SyncRoot => _dictionary.SyncRoot;

		public void Add(TKey item) => throw new InvalidOperationException();
		public void Clear() => throw new InvalidOperationException();

		public bool Contains(TKey item) => _dictionary.ContainsKey(item);

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (array.Length - arrayIndex < _dictionary.Count) throw new ArgumentException();

			foreach (var p in _dictionary._list) array[arrayIndex++] = p.Key;
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
				if (_dictionary.Count > 0) throw new ArgumentException();
				return;
			}
			if (array.Length - (index - lowerBound) < _dictionary.Count) throw new ArgumentException();

			foreach (var p in _dictionary._list) array.SetValue(p.Key, index++);
		}

		public IEnumerator<TKey> GetEnumerator() => _dictionary._list.Select(x => x.Key).GetEnumerator();

		public bool Remove(TKey item) => throw new InvalidOperationException();

		IEnumerator IEnumerable.GetEnumerator() => _dictionary._list.Select(x => x.Key).GetEnumerator();
	}

	[DebuggerTypeProxy(typeof(DebugViewOfDictionaryValueCollection<,>))]
	[DebuggerDisplay("Count = {Count}")]
	sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection
	{
		readonly OrderedDictionary<TKey, TValue> _dictionary;

		public ValueCollection(OrderedDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

		public int Count => _dictionary.Count;

		public bool IsReadOnly => true;

		public bool IsSynchronized => false;

		public object SyncRoot => _dictionary.SyncRoot;

		public void Add(TValue item) => throw new InvalidOperationException();
		public void Clear() => throw new InvalidOperationException();

		public bool Contains(TValue item) => _dictionary._list.Any(x => EqualityComparer<TValue>.Default.Equals(x.Value, x.Value));

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (array.Length - arrayIndex < _dictionary.Count) throw new ArgumentException();

			foreach (var p in _dictionary._list) array[arrayIndex++] = p.Value;
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
				if (_dictionary.Count > 0) throw new ArgumentException();
				return;
			}
			if (array.Length - (index - lowerBound) < _dictionary.Count) throw new ArgumentException();

			foreach (var p in _dictionary._list) array.SetValue(p.Value, index++);
		}

		public IEnumerator<TValue> GetEnumerator() => _dictionary._list.Select(x => x.Value).GetEnumerator();

		public bool Remove(TValue item) => throw new InvalidOperationException();

		IEnumerator IEnumerable.GetEnumerator() => _dictionary._list.Select(x => x.Value).GetEnumerator();
	}

	sealed class DictionaryEnumerator : IDictionaryEnumerator, IDisposable
	{
		readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
		public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator) => _enumerator = enumerator;
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
}

internal sealed class DebugViewOfDictionary<K, V> where K : notnull
{
	readonly IDictionary<K, V> dict;

	public DebugViewOfDictionary(IDictionary<K, V> dictionary)
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));

		dict = dictionary;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public KeyValuePair<K, V>[] Items
	{
		get
		{
			var items = new KeyValuePair<K, V>[dict.Count];
			dict.CopyTo(items, 0);
			return items;
		}
	}
}

internal sealed class DebugViewOfDictionaryKeyCollection<K, V> where K : notnull
{
	readonly ICollection<K> _collection;

	public DebugViewOfDictionaryKeyCollection(ICollection<K> collection)
	{
		if (collection == null)
			throw new ArgumentNullException(nameof(collection));

		_collection = collection;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public K[] Items
	{
		get
		{
			K[] items = new K[_collection.Count];
			_collection.CopyTo(items, 0);
			return items;
		}
	}
}

internal sealed class DebugViewOfDictionaryValueCollection<K, V> where K : notnull
{
	readonly ICollection<V> _collection;

	public DebugViewOfDictionaryValueCollection(ICollection<V> collection)
	{
		if (collection == null)
			throw new ArgumentNullException(nameof(collection));

		_collection = collection;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public V[] Items
	{
		get
		{
			V[] items = new V[_collection.Count];
			_collection.CopyTo(items, 0);
			return items;
		}
	}
}