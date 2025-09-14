using System.Collections;

namespace Blazilla.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable"/> to support element access operations.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Returns the element at the specified index in the enumerable sequence.
    /// </summary>
    /// <param name="source">The enumerable sequence to retrieve an element from.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is negative or when the index exceeds the number of elements in the sequence.
    /// </exception>
    public static object ElementAt(this IEnumerable source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var item = source.ElementAtOrDefault(index);

        if (item != null)
            return item!;

        throw new ArgumentOutOfRangeException(nameof(index), "Index exceeds the number of elements.");
    }

    /// <summary>
    /// Returns the element at the specified index in the enumerable sequence, or a default value if the index is out of range.
    /// </summary>
    /// <param name="source">The enumerable sequence to retrieve an element from.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>
    /// The element at the specified index if it exists; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// This method optimizes performance by checking if the source implements <see cref="IList"/>
    /// for direct index access before falling back to enumeration.
    /// </remarks>
    public static object? ElementAtOrDefault(this IEnumerable source, int index)
    {
        if (source == null || index < 0)
            return default;

        // Optimization: Check if source implements IList for direct index access
        if (source is IList list)
            return index < list.Count ? list[index] : default;

        // Fallback to enumeration for other IEnumerable implementations
        int currentIndex = 0;
        foreach (var item in source)
        {
            if (currentIndex == index)
                return item;

            currentIndex++;
        }

        return default;
    }
}
