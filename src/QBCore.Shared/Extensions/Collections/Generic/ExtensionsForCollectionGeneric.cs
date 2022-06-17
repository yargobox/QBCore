namespace QBCore.Extensions.Collections.Generic;

public static class ExtensionsForCollectionGeneric
{
	public static int GetNonEnumeratedCountOrCount<TSource>(this IEnumerable<TSource> source)
	{
		int count;
		return source.TryGetNonEnumeratedCount(out count) ? count : source.Count();
	}
}