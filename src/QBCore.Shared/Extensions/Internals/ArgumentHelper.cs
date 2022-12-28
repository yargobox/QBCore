using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using QBCore.Extensions.Collections.Concurrent;

namespace QBCore.Extensions.Internals;

public static class ArgumentHelper
{
	static class Static
	{
		static Static() { }

		public static readonly MethodInfo _makeCastToCollectionAdapterDelegateMethod = typeof(ArgumentHelper).GetMethod(nameof(MakeCastToCollectionAdapterDelegate), BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new ArgumentNullException(nameof(_makeCastToCollectionAdapterDelegateMethod));
		public static readonly MethodInfo _makeConvertFromObjectDelegateMethod = typeof(ArgumentHelper).GetMethod(nameof(MakeConvertFromObjectDelegate), BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new ArgumentNullException(nameof(_makeConvertFromObjectDelegateMethod));
		public static readonly MethodInfo _makeConvertUncheckedFromObjectDelegate = typeof(ArgumentHelper).GetMethod(nameof(MakeConvertUncheckedFromObjectDelegate), BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new ArgumentNullException(nameof(_makeConvertUncheckedFromObjectDelegate));
		public static readonly MethodInfo _makeConvertToCollectionDelegateMethod = typeof(ArgumentHelper).GetMethod(nameof(MakeConvertToCollectionDelegate), BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new ArgumentNullException(nameof(_makeConvertToCollectionDelegateMethod));
		public static readonly MethodInfo _makeConvertUncheckedToCollectionDelegateMethod = typeof(ArgumentHelper).GetMethod(nameof(MakeConvertUncheckedToCollectionDelegate), BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new ArgumentNullException(nameof(_makeConvertUncheckedToCollectionDelegateMethod));

		public static readonly KeyedPool<KeyValuePair<Type, Type>, Func<object, bool, ICollection>> _castAdapters = new();
		public static readonly KeyedPool<KeyValuePair<Type, Type>, Delegate> _objectToValueConvertors = new();
		public static readonly KeyedPool<KeyValuePair<Type, Type>, Delegate> _objectToValueUncheckedConvertors = new();
		public static readonly KeyedPool<KeyValuePair<Type, Type>, Delegate> _objectToCollectionConvertors = new();
		public static readonly KeyedPool<KeyValuePair<Type, Type>, Delegate> _objectToCollectionUncheckedConvertors = new();
	}

    /// <summary>
    /// Ensures that a value has the expected type, or ICollection<> of values of that type, or instead of the expected type, a compliment type to it.
    /// </summary>
    /// <param name="value">Value to check or prepare</param>
    /// <param name="expected">Field type</param>
    /// <param name="underlying">Underlying type of the field type or underlying type of the Nullable<> generic parameter</param>
    /// <param name="compliment">A type of the prepared value, or a type of the element in the prepared collection that is a compliment to the expected one, otherwise null.</param>
    /// <returns>false if value is a single value or null, true if value is a value collection</returns>
    /// <remarks>
    /// The method can also create a proxy to adapt the IEnumerable<>, IEnumerable, or ICollection interfaces to the required ICollection<>.
    /// This may cause IEnumerable<> or IEnumerable to be traversed to get the number of elements or the element type.
    /// </remarks>
    /// <exception cref="ArgumentException">value is not of the expected type</exception>
	public static bool PrepareAsValueOrCollection(ref object? value, Type expected, Type underlying, out Type? compliment)
	{
		compliment = null;

		if (value is null)
		{
			if (expected.IsValueType) throw ArgumentNullExceptionExpectedValueTypeCannotBeNull(expected);

			return false;
		}

		var type = value.GetType();
		if (type == expected)
		{
			return false;
		}

		compliment = underlying;

		if (type == underlying)
		{
			return false;
		}

		if (expected == underlying && underlying.IsValueType)
		{
			compliment = typeof(Nullable<>).MakeGenericType(underlying);

			// In fact, this should never happen, because the object boxes the value itself or null, not the Nullable<> struct.
			//if (type == compliment)
			//{
			//	return null;
			//}
			Debug.Assert(type != compliment);
		}

		bool hasEnumerable = false, hasCollection = false;
		Type? colItemType = null, enItemType = null, temp;
		Type[] interfaces = type.GetInterfaces();

		foreach (var itype in interfaces)
		{
			if (itype.IsGenericType)
			{
				temp = itype.GetGenericTypeDefinition();
				if (temp == typeof(ICollection<>))
				{
					temp = itype.GetGenericArguments()[0];
					if (temp == expected)
					{
						compliment = null;
						return true;
					}
					else if (temp == compliment)
					{
						colItemType = compliment;
					}
					else if (colItemType is null && temp != typeof(object))
					{
						colItemType = temp;
					}
				}
				else if (temp == typeof(IEnumerable<>))
				{
					temp = itype.GetGenericArguments()[0];
					if (temp == expected)
					{
						enItemType = expected;
					}
					else if (temp == compliment)
					{
						enItemType = compliment;
					}
					else if (enItemType is null && temp != typeof(object))
					{
						enItemType = temp;
					}
				}
			}
			else if (itype == typeof(IEnumerable))
			{
				hasEnumerable = true;
			}
			else if (itype == typeof(ICollection))
			{
				hasCollection = true;
			}
		}

		if (enItemType == expected)
		{
			value = GetCastToCollectionAdapter(value, expected, expected, hasCollection);
			compliment = null;
			return true;
		}
		if (colItemType == compliment)
		{
			if (compliment == underlying)
			{
				return true;
			}

			value = GetCastToCollectionAdapter(value, expected, compliment, hasCollection);
			compliment = null;
			return true;
		}
		if (enItemType == compliment)
		{
			if (compliment == underlying)
			{
				value = GetCastToCollectionAdapter(value, compliment, compliment, hasCollection);
				return true;
			}

			value = GetCastToCollectionAdapter(value, expected, compliment, hasCollection);
			compliment = null;
			return true;
		}

		if (colItemType is not null)
		{
			throw ArgumentExceptionExpectedTypeIsThisNotThat(expected, colItemType);
		}
		if (enItemType is not null)
		{
			throw ArgumentExceptionExpectedTypeIsThisNotThat(expected, enItemType);
		}

		if (hasEnumerable || hasCollection)
		{
			temp = GetFirstItemTypeOrDefault((IEnumerable)value);
			if (temp is null || temp == expected || temp == compliment)
			{
				value = GetCastToCollectionAdapter(value, expected, typeof(object), hasCollection);
				compliment = null;
				return true;
			}

			throw ArgumentExceptionExpectedTypeIsThisNotThat(expected, temp);
		}

		throw ArgumentExceptionExpectedTypeIsThisNotThat(expected, type);
	}

	[return: NotNullIfNotNull(nameof(value))]
	public static TResult ConvertToValue<TResult>(object? value)
	{
		if (value is null)
		{
			return default(TResult?)!;
		}

		var key = KeyValuePair.Create(typeof(TResult), value.GetType());
		var pfn = (Func<object, TResult>)Static._objectToValueConvertors.GetOrAdd(key, key => CallMakeConvertFromObjectDelegate(key.Key, key.Value));
		return pfn(value)!;
	}

	[return: NotNullIfNotNull(nameof(value))]
	public static TResult ConvertUncheckedToValue<TResult>(object? value)
	{
		if (value is null)
		{
			return default(TResult?)!;
		}

		var key = KeyValuePair.Create(typeof(TResult), value.GetType());
		var pfn = (Func<object, TResult>)Static._objectToValueUncheckedConvertors.GetOrAdd(key, key => CallMakeConvertUncheckedFromObjectDelegate(key.Key, key.Value));
		return pfn(value)!;
	}

	[return: NotNullIfNotNull(nameof(value))]
	public static ICollection<TResult>? ConvertToCollection<TResult>(object? value, Type itemType)
	{
		if (value is null)
		{
			return null;
		}

		var key = KeyValuePair.Create(typeof(TResult), itemType);
		var pfn = (Func<object, ICollection<TResult>>)Static._objectToCollectionConvertors.GetOrAdd(key, key => CallMakeConvertToCollectionDelegate(key.Key, key.Value));
		return pfn(value);
	}

	[return: NotNullIfNotNull(nameof(value))]
	public static ICollection<TResult>? ConvertUncheckedToCollection<TResult>(object? value, Type itemType)
	{
		if (value is null)
		{
			return null;
		}

		var key = KeyValuePair.Create(typeof(TResult), itemType);
		var pfn = (Func<object, ICollection<TResult>>)Static._objectToCollectionUncheckedConvertors.GetOrAdd(key, key => CallMakeConvertUncheckedToCollectionDelegate(key.Key, key.Value));
		return pfn(value);
	}

	private static ICollection GetCastToCollectionAdapter(object values, Type expected, Type item, bool hasCollection)
	{
		var key = KeyValuePair.Create(expected, item);
		var adapter = Static._castAdapters.GetOrAdd(key, key => CallMakeCastToCollectionAdapterDelegate(key.Key, key.Value));
		return adapter(values, hasCollection);
	}
	private static Func<object, bool, ICollection> CallMakeCastToCollectionAdapterDelegate(Type expected, Type item)
	{
		var method = Static._makeCastToCollectionAdapterDelegateMethod.MakeGenericMethod(expected, item);
		return (Func<object, bool, ICollection>)method.Invoke(null, null)!;
	}
	private static Func<object, bool, ICollection> MakeCastToCollectionAdapterDelegate<Expected, Item>()
	{
		if (typeof(Item) != typeof(object))
		{
			if (typeof(Expected) == typeof(Item))
			{
				return (values, isCollection) =>
				{
					var col = (IEnumerable<Expected>)values;
					var count = isCollection ? ((ICollection)values).Count : col.Count();
					return new CollectionAdapter<Expected>(col, count);
				};
			}
			else if (default(Item) is null && default(Expected) is not null)
			{
				return (values, isCollection) =>
				{
					var col = (IEnumerable<Item>)values;
					var count = isCollection ? ((ICollection)values).Count : col.Count();
					return new CollectionAdapter<Expected>(
						col
							.SelectE(x => x is not null ? x : throw ArgumentNullExceptionExpectedValueTypeCannotBeNull(typeof(Expected)))
							.Cast<Expected>(),
						count);
				};
			}
			else
			{
				return (values, isCollection) =>
				{
					var col = (IEnumerable<Item>)values;
					var count = isCollection ? ((ICollection)values).Count : col.Count();
					return new CollectionAdapter<Expected>(col.Cast<Expected>(), count);
				};
			}
		}
		else if (default(Expected) is not null)
		{
			return (values, isCollection) =>
			{
				var col = (IEnumerable)values;
				var count = isCollection ? ((ICollection)values).Count : col.CountE();
				return new CollectionAdapter<Expected>(
					col
						.SelectE(x => x is not null ? x : throw ArgumentNullExceptionExpectedValueTypeCannotBeNull(typeof(Expected)))
						.Cast<Expected>(),
					count);
			};
		}
		else
		{
			return (values, isCollection) =>
			{
				var col = (IEnumerable)values;
				var count = isCollection ? ((ICollection)values).Count : col.CountE();
				return new CollectionAdapter<Expected>(col.Cast<Expected>(), count);
			};
		}
	}

	private static Delegate CallMakeConvertFromObjectDelegate(Type resultType, Type sourceType)
	{
		var method = Static._makeConvertFromObjectDelegateMethod.MakeGenericMethod(resultType, sourceType);
		return (Delegate)method.Invoke(null, null)!;
	}
	private static Delegate MakeConvertFromObjectDelegate<TResult, TSource>()
	{
		return TResult (object? value) => ConvertTo<TResult>.FromObject<TSource>(value);
	}

	private static Delegate CallMakeConvertUncheckedFromObjectDelegate(Type resultType, Type sourceType)
	{
		var method = Static._makeConvertUncheckedFromObjectDelegate.MakeGenericMethod(resultType, sourceType);
		return (Delegate)method.Invoke(null, null)!;
	}
	private static Delegate MakeConvertUncheckedFromObjectDelegate<TResult, TSource>()
	{
		return TResult (object? value) => ConvertTo<TResult>.FromObjectUnchecked<TSource>(value);
	}

	private static Delegate CallMakeConvertToCollectionDelegate(Type resultType, Type itemType)
	{
		var method = Static._makeConvertToCollectionDelegateMethod.MakeGenericMethod(resultType, itemType);
		return (Delegate)method.Invoke(null, null)!;
	}
	private static Func<object, ICollection<TResult>> MakeConvertToCollectionDelegate<TResult, TItem>()
	{
		return ICollection<TResult> (object values) =>
		{
			if (values is not ICollection<TItem> icol)
			{
				throw ArgumentExceptionExpectedTypeIsThisNotThat(typeof(ICollection<TItem>), values.GetType(), nameof(values));
			}

			return new CollectionAdapter<TResult>(icol.Select(x => ConvertTo<TResult>.From<TItem>(x)), icol.Count);
		};
	}

	private static Delegate CallMakeConvertUncheckedToCollectionDelegate(Type resultType, Type itemType)
	{
		var method = Static._makeConvertUncheckedToCollectionDelegateMethod.MakeGenericMethod(resultType, itemType);
		return (Delegate)method.Invoke(null, null)!;
	}
	private static Func<object, ICollection<TResult>> MakeConvertUncheckedToCollectionDelegate<TResult, TItem>()
	{
		return ICollection<TResult> (object values) =>
		{
			if (values is not ICollection<TItem> icol)
			{
				throw ArgumentExceptionExpectedTypeIsThisNotThat(typeof(ICollection<TItem>), values.GetType(), nameof(values));
			}

			return new CollectionAdapter<TResult>(icol.Select(x => ConvertTo<TResult>.FromUnchecked<TItem>(x)), icol.Count);
		};
	}

	private static int CountE(this IEnumerable col)
	{
		int count = 0;
		var it = col.GetEnumerator();
		using var disposable = it as IDisposable;
		while (it.MoveNext()) count++;
		return count;
	}
	private static IEnumerable SelectE(this IEnumerable source, Func<object?, object?> selector)
	{
		var it = source.GetEnumerator();
		using var disposable = it as IDisposable;
		while (it.MoveNext())
		{
			yield return selector(it.Current);
		}
	}
	private static Type? GetFirstItemTypeOrDefault(IEnumerable col)
	{
		foreach (var obj in col)
		{
			if (obj is null) continue;
			return obj.GetType();
		}
		return null;
	}

	private class CollectionAdapter<Target> : ICollection<Target>, IReadOnlyCollection<Target>, IEnumerable<Target>, ICollection, IEnumerable
	{
		private readonly IEnumerable<Target> _enumerable;
		private readonly int _count;

		public int Count => _count;
		bool ICollection<Target>.IsReadOnly => true;
		bool ICollection.IsSynchronized => true;
		object ICollection.SyncRoot => throw new NotSupportedException();

		public CollectionAdapter(IEnumerable<Target> enumerable, int count)
		{
			_enumerable = enumerable;
			_count = count;
		}

		public bool Contains(Target item) => _enumerable.Contains(item);

		public void CopyTo(Target[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (array.Length - arrayIndex < _count) throw new ArgumentException(nameof(arrayIndex));

			foreach (var value in _enumerable)
			{
				array[arrayIndex++] = value;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < _count) throw new ArgumentException(nameof(index));

			foreach (var value in _enumerable)
			{
				array.SetValue(value, index++);
			}
		}

		public IEnumerator<Target> GetEnumerator() => _enumerable.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_enumerable).GetEnumerator();

		public void Add(Target item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Remove(Target item) => throw new NotSupportedException();
	}

	private static ArgumentNullException ArgumentNullExceptionExpectedValueTypeCannotBeNull(Type type)
	{
		return new ArgumentNullException($"The expected value type '{type.ToPretty()}' cannot be null.");
	}

	private static ArgumentException ArgumentExceptionExpectedTypeIsThisNotThat(Type expected, Type other, string? paramName = null)
	{
		if (paramName is null)
			return new ArgumentException($"The expected value type is '{expected.ToPretty()}', not '{other.ToPretty()}'.");
		else
			return new ArgumentException($"The expected value type is '{expected.ToPretty()}', not '{other.ToPretty()}'.", paramName);
	}
}