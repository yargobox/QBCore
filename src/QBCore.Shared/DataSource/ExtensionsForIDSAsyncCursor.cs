using System.Data;
using System.Runtime.CompilerServices;

namespace QBCore.DataSource;

public static class ExtensionsForIDSAsyncCursor
{
	/// <summary>
	/// Determines whether a cursor contains any elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IDSAsyncCursor{T}"/> to check for emptiness.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is true if the source cursor contains any elements; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static async Task<bool> AnyAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			return await source.MoveNextAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
    /// Determines whether any element of a cursor satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IDSAsyncCursor{T}"/> whose elements to apply the predicate to.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> whose result is true if the source cursor is not empty and at least one of its elements passes the test in the specified predicate; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static async Task<bool> AnyAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current)) return true;
			}
		}

		return false;
	}

	/// <summary>
    /// Determines whether all elements of a cursor satisfy a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IDSAsyncCursor{T}"/> that contains the elements to apply the predicate to.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> whose result is true if every element of the source cursor passes the test in the specified predicate,
    /// or if the cursor is empty; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static async Task<bool> AllAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (!predicate(source.Current)) return false;
			}
		}

		return true;
	}

	/// <summary>
    /// Returns the number of elements in a cursor.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">A cursor that contains elements to be counted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> whose result is the number of elements in the cursor.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="OverflowException">The number of elements in <paramref name="source"/> is larger than <see cref="Int32.MaxValue"/>.</exception>
	public static async Task<int> CountAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		var index = 0;

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				index++;
			}
		}

		return index;
	}

	/// <summary>
    /// Returns a number that represents how many elements in the specified cursor satisfy a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">A cursor that contains elements to be tested and counted.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> whose result is a number that represents how many elements in the cursor satisfy the condition in the predicate function.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="OverflowException">The number of elements in <paramref name="source"/> is larger than <see cref="Int32.MaxValue"/>.</exception>
	public static async Task<int> CountAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		var index = 0;

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current))
				{
					index++;
				}
			}
		}

		return index;
	}

	/// <summary>
	/// Returns the first element of a cursor.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IDSAsyncCursor{T}"/> to return the first element of.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the first element in the specified cursor.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The source cursor is empty.</exception>
	public static async Task<T> FirstAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			return await source.MoveNextAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false)
				? source.Current
				: throw new InvalidOperationException("Cursor contains no elements");
		}
	}

	/// <summary>
	/// Returns the first element in a cursor that satisfies a specified condition.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return an element from.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the first element in the cursor that passes the test in the specified predicate function.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No element satisfies the condition in <paramref name="predicate"/>. -or- The <paramref name="source"/> cursor is empty.</exception>
	public static async Task<T> FirstAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current)) return source.Current;
			}
		}

		throw new InvalidOperationException("Cursor contains no elements");
	}

	/// <summary>
	/// Returns the first element of a cursor, or a default value if the cursor contains no elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IDSAsyncCursor{T}"/> to return the first element of.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is default(<typeparamref name="T"/>) if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static async Task<T?> FirstOrDefaultAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			return await source.MoveNextAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false) ? source.Current : default(T);
		}
	}

	/// <summary>
	/// Returns the first element of a cursor, or a specified default value if the cursor contains no elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IDSAsyncCursor{T}"/> to return the first element of.</param>
    /// <param name="defaultValue">The default value to return if the cursor is empty.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is <paramref name="defaultValue"/> if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static async Task<T?> FirstOrDefaultAsync<T>(this IDSAsyncCursor<T> source, T defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			return await source.MoveNextAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false) ? source.Current : defaultValue;
		}
	}

	/// <summary>
	/// Returns the first element in a cursor that satisfies a specified condition.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return an element from.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the first element in the cursor that passes the test in the specified predicate function.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static async Task<T?> FirstOrDefaultAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current)) return source.Current;
			}
		}

		return default(T);
	}

	/// <summary>
	/// Returns the first element of the cursor that satisfies a condition, or a specified default value if no such element is found.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return an element from.</param>
    /// <param name="defaultValue">The default value to return if the cursor is empty.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is <paramref name="defaultValue"/> if <paramref name="source"/> is empty or if no element passes
    /// the test specified by <paramref name="predicate"/>; otherwise, the first element in <paramref name="source"/> that passes the test specified by <paramref name="predicate"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static async Task<T?> FirstOrDefaultAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, T defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current)) return source.Current;
			}
		}

		return defaultValue;
	}

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IDSAsyncCursor{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> whose elements to perform the specified action to.</param>
	/// <param name="processor">The <see cref="Func{T, Task}"/> delegate to perform on each element of the <see cref="IDSAsyncCursor{T}"/>.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> that completes when all the elements have been processed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="processor"/> is null.</exception>
	public static async Task ForEachAsync<T>(this IDSAsyncCursor<T> source, Func<T, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (processor == null) throw new ArgumentNullException(nameof(processor));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				await processor(source.Current).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IDSAsyncCursor{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> whose elements to perform the specified action to.</param>
	/// <param name="processor">The <see cref="Func{T, int, Task}"/> delegate to perform on each element of the <see cref="IDSAsyncCursor{T}"/>.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> that completes when all the elements have been processed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="processor"/> is null.</exception>
	public static async Task ForEachAsync<T>(this IDSAsyncCursor<T> source, Func<T, int, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (processor == null) throw new ArgumentNullException(nameof(processor));

		await using (source)
		{
			var index = 0;

			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				await processor(source.Current, index++).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IDSAsyncCursor{T}"/>.
	/// </summary>
	/// <remarks>
	/// If your delegate is going to take a long time to execute or is going to block
	/// consider using a different overload of ForEachAsync that uses a delegate that
	/// returns a Task instead.
	/// </remarks>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> whose elements to perform the specified action to.</param>
	/// <param name="processor">The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="IDSAsyncCursor{T}"/>.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> that completes when all the elements have been processed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="processor"/> is null.</exception>
	public static async Task ForEachAsync<T>(this IDSAsyncCursor<T> source, Action<T> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (processor == null) throw new ArgumentNullException(nameof(processor));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				processor(source.Current);
			}
		}
	}

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IDSAsyncCursor{T}"/>.
	/// </summary>
	/// <remarks>
	/// If your delegate is going to take a long time to execute or is going to block
	/// consider using a different overload of ForEachAsync that uses a delegate that
	/// returns a Task instead.
	/// </remarks>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> whose elements to perform the specified action to.</param>
	/// <param name="processor">The <see cref="Action{T, int}"/> delegate to perform on each element of the <see cref="IDSAsyncCursor{T}"/>.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> that completes when all the elements have been processed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="processor"/> is null.</exception>
	public static async Task ForEachAsync<T>(this IDSAsyncCursor<T> source, Action<T, int> processor, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (processor == null) throw new ArgumentNullException(nameof(processor));

		await using (source)
		{
			var index = 0;

			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				processor(source.Current, index++);
			}
		}
	}

	/// <summary>
	/// Returns the only element of a cursor, and throws an exception if there is not exactly one element in the cursor.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the single element of.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the single element of the cursor.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The cursor contains more than one element. -or- The cursor is empty.</exception>
	public static async Task<T> SingleAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			if (!await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false)) throw new InvalidOperationException("Cursor contains no elements");

			var result = source.Current;

			if (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false)) throw new InvalidOperationException("Cursor contains more than one element");

			return result;
		}
	}

	/// <summary>
	/// Returns the only element of a cursor that satisfies a specified condition, and throws an exception if more than one such element exists.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return a single element from.</param>
    /// <param name="predicate">A function to test an element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the single element of the cursor that satisfies a condition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No element satisfies the condition in predicate. -or- More than one element satisfies the condition in predicate. -or- The source cursor is empty.</exception>
	public static async Task<T> SingleAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current))
				{
					var result = source.Current;

					while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
					{
						if (predicate(source.Current)) throw new InvalidOperationException("Cursor contains more than one element");
					}

					return result;
				}
			}

			throw new InvalidOperationException("Cursor contains no elements");
		}
	}

	/// <summary>
	/// Returns the only element of a cursor, or a default value if the cursor is empty; this method throws an exception if there is more than one element in the cursor.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the single element of.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the single element of the cursor, or default(<typeparamref name="T"/>) if the cursor contains no elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The cursor contains more than one element.</exception>
	public static async Task<T?> SingleOrDefaultAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			if (!await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false)) return default(T);

			var result = source.Current;

			if (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false)) throw new InvalidOperationException("Cursor contains more than one element");

			return result;
		}
	}

	/// <summary>
	/// Returns the only element of a cursor, or a specified default value if the cursor is empty; this method throws an exception if there is more than one element in the cursor.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the single element of.</param>
    /// <param name="defaultValue">The default value to return if the cursor is empty.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the single element of the cursor, or <paramref name="defaultValue"/> if the cursor contains no elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The cursor contains more than one element.</exception>
	public static async Task<T?> SingleOrDefaultAsync<T>(this IDSAsyncCursor<T> source, T defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			if (!await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false)) return defaultValue;

			var result = source.Current;

			if (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false)) throw new InvalidOperationException("Cursor contains more than one element");

			return result;
		}
	}

	/// <summary>
	/// Returns the only element of a cursor that satisfies a specified condition or a default value if no such element exists;
    /// this method throws an exception if more than one element satisfies the condition.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the single element of.</param>
    /// <param name="predicate">A function to test an element for a condition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the single element of the cursor that satisfies the condition, or default(<typeparamref name="T"/>) if no such element is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="InvalidOperationException">More than one element satisfies the condition in predicate.</exception>
	public static async Task<T?> SingleOrDefaultAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current))
				{
					var result = source.Current;

					while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
					{
						if (predicate(source.Current)) throw new InvalidOperationException("Cursor contains more than one element");
					}

					return result;
				}
			}

			return default(T);
		}
	}

	/// <summary>
	/// Returns the only element of a cursor that satisfies a specified condition, or a specified default value
    /// if no such element exists; this method throws an exception if more than one element satisfies the condition.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the single element of.</param>
    /// <param name="predicate">A function to test an element for a condition.</param>
    /// <param name="defaultValue">The default value to return if the cursor is empty.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose result is the single element of the cursor that satisfies the condition, or <paramref name="defaultValue"/> if no such element is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="InvalidOperationException">More than one element satisfies the condition in predicate.</exception>
	public static async Task<T?> SingleOrDefaultAsync<T>(this IDSAsyncCursor<T> source, Func<T, bool> predicate, T defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				if (predicate(source.Current))
				{
					var result = source.Current;

					while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
					{
						if (predicate(source.Current)) throw new InvalidOperationException("Cursor contains more than one element");
					}

					return result;
				}
			}

			return defaultValue;
		}
	}

	/// <summary>
	/// Represents a cursor as <see cref="IAsyncEnumerable{T}"/> and allows to specify a cancellation token for it.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The source cursor to type as <see cref="IAsyncEnumerable{T}"/>.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The cursor typed as <see cref="IAsyncEnumerable{T}"/>.</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IDSAsyncCursor<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				yield return source.Current;
			}
		}
	}

	/// <summary>
	/// Represents a cursor as <see cref="IEnumerable{T}"/> and allows to specify a cancellation token for it.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The source cursor to type as <see cref="IEnumerable{T}"/>.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The cursor typed as <see cref="IEnumerable{T}"/>.</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static IEnumerable<T> AsEnumerable<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		using (source)
		{
			while (source.MoveNext(CommandBehavior.Default, cancellationToken))
			{
				yield return source.Current;
			}
		}
	}

	/// <summary>
	/// Creates a <see cref="List{T}"/> from an <see cref="IDSAsyncCursor{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IDSAsyncCursor{T}"/> to create a <see cref="List{T}"/> from.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A <see cref="Task"/> whose value is the <see cref="List{T}"/> that contains elements from the cursor.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static async Task<List<T>> ToListAsync<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		var list = new List<T>();

		await using (source)
		{
			while (await source.MoveNextAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				list.Add(source.Current);
			}
		}

		return list;
	}

	/// <summary>
	/// Creates a <see cref="List{T}"/> from an <see cref="IDSAsyncCursor{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IDSAsyncCursor{T}"/> to create a <see cref="List{T}"/> from.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The <see cref="List{T}"/> that contains elements from the cursor.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static List<T> ToList<T>(this IDSAsyncCursor<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		var list = new List<T>();

		using (source)
		{
			while (source.MoveNext(CommandBehavior.Default, cancellationToken))
			{
				list.Add(source.Current);
			}
		}

		return list;
	}

	/// <summary>
    /// Sets a callback action to get a mark whether the given page is the last one. It is called when a cursor reaches the end.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the last page mark of.</param>
    /// <param name="lastPageCallback">Callback action to call when the cursor reaches the end.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="lastPageCallback"/> is null.</exception>
	/// <exception cref="NotSupportedException">Obtaining the last page mark is not supported by <paramref name="source"/>.</exception>
	public static IDSAsyncCursor<T> GetLastPageMark<T>(this IDSAsyncCursor<T> source, Action<bool> lastPageCallback)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		source.OnLastPage += lastPageCallback;
		return source;
	}

	/// <summary>
    /// Sets a callback action to get the total number of rows. It is called when a cursor reaches the end.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IDSAsyncCursor{T}"/> to return the total number of rows of.</param>
    /// <param name="totalCountCallback">Callback action to call when the cursor reaches the end.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="totalCountCallback"/> is null.</exception>
	/// <exception cref="NotSupportedException">Obtaining the total number of rows is not supported by <paramref name="source"/>.</exception>
	public static IDSAsyncCursor<T> GetTotalCount<T>(this IDSAsyncCursor<T> source, Action<long> totalCountCallback)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		source.OnTotalCount += totalCountCallback;
		return source;
	}
}