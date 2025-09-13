using System.Text;

namespace LoreSoft.Blazor.FluentValidation;

/// <summary>Provide a cached reusable instance of StringBuilder per thread.</summary>
internal static class StringBuilderCache
{
    // The value 360 was chosen in discussion with performance experts as a compromise between using
    // as little memory per thread as possible and still covering a large part of short-lived
    // StringBuilder creations on the startup path of VS designers.
    internal const int MaxBuilderSize = 360;
    private const int DefaultCapacity = 16; // == StringBuilder.DefaultCapacity

    [ThreadStatic]
    private static StringBuilder? t_cachedInstance;

    /// <summary>Get a StringBuilder for the specified capacity.</summary>
    /// <remarks>If a StringBuilder of an appropriate size is cached, it will be returned and the cache emptied.</remarks>
    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
        if (capacity > MaxBuilderSize)
            return new StringBuilder(capacity);

        var sb = t_cachedInstance;
        if (sb is null)
            return new StringBuilder(capacity);

        // Avoid StringBuilder block fragmentation by getting a new StringBuilder
        // when the requested size is larger than the current capacity
        if (capacity > sb.Capacity)
            return new StringBuilder(capacity);

        t_cachedInstance = null;
        sb.Clear();

        return sb;

    }

    /// <summary>Release StringBuilder to the cache, and return the resulting string.</summary>
    public static string Release(this StringBuilder stringBuilder)
    {
        if (stringBuilder is null)
            throw new ArgumentNullException(nameof(stringBuilder));

        string result = stringBuilder.ToString();

        if (stringBuilder.Capacity <= MaxBuilderSize)
            t_cachedInstance = stringBuilder;

        return result;
    }
}
