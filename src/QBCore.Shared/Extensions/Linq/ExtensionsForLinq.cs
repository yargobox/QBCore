using System.Diagnostics.CodeAnalysis;

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

	public static int CountTryGetNonEnumerated<T>(this IEnumerable<T> @this)
	{
		return (@this as ICollection<T>)?.Count ?? @this.Count();
	}

	public static bool IsNullEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? @this)
	{
		if (@this is not null)
		{
			foreach (var _ in @this) return false;
		}
		return true;
	}

	public static int CountUpTo<T>(this IEnumerable<T>? @this, int maxCount)
	{
		if (@this != null)
		{
			int counter = 0;
			foreach (var _ in @this)
			{
				if (++counter >= maxCount) return counter;
			}
			return counter;
		}
		return 0;
	}
}