using System.Reflection;

namespace QBCore.Extensions.Collections.Generic;

[DefaultMember("Item")]
public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
	TKey KeyOf(int index);
	int IndexOf(TKey key);
	void Insert(int index, TKey key, TValue value);
	void RemoveAt(int index);
	void Sort();
	void Sort(Comparison<KeyValuePair<TKey, TValue>> comparer);
	void Sort(IComparer<KeyValuePair<TKey, TValue>>? comparer);
	void Sort(int index, int count, IComparer<KeyValuePair<TKey, TValue>>? comparer);
}