namespace QBCore.Extensions.Linq;

public static class ExtensionsForLinq
{
	public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
	{
		T tmp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = tmp;
		return list;
	}

	public static IEnumerable<T> Next<T, T1>(this IEnumerable<T> @this, IEnumerable<T1>? next) where T1 : T
	{
		if (next != null) using (var p = next.GetEnumerator()) while (p.MoveNext()) yield return p.Current;
	}
}