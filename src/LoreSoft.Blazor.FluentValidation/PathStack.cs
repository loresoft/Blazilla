using System.Collections.Concurrent;
using System.Text;

namespace LoreSoft.Blazor.FluentValidation;

/// <summary>
/// Path stack structure used to compute object graph paths expressions
/// </summary>
public class PathStack
{
    private const char DOT_SEPARATOR = '.';
    private const char OPENING_BRACKET = '[';
    private const char CLOSING_BRACKET = ']';

    private readonly ConcurrentStack<PathValue> _pathStack = [];

    /// <summary>
    /// Push a property name to the stack
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    public void PushProperty(string propertyName)
        => _pathStack.Push(new(propertyName ?? string.Empty, Separator: DOT_SEPARATOR));

    /// <summary>
    /// Push an indexer to the stack. Will be converted to string.
    /// </summary>
    /// <typeparam name="T">The type of the indexer</typeparam>
    /// <param name="index">The indexer value. Will be converted to string</param>
    public void PushIndex<T>(T index)
        => _pathStack.Push(new(index?.ToString() ?? string.Empty, Indexer: true));

    /// <summary>
    /// Push a key indexer to the stack. Will be converted to string.
    /// </summary>
    /// <typeparam name="T">The type of the key indexer</typeparam>
    /// <param name="key">The key indexer value. Will be converted to string.</param>
    public void PushKey<T>(T key)
        => _pathStack.Push(new(key?.ToString() ?? string.Empty, Indexer: true));

    /// <summary>
    /// Pop the last path off the stack
    /// </summary>
    public void Pop()
        => _pathStack.TryPop(out _);

    /// <summary>
    /// Clear the path stack
    /// </summary>
    public void Clear()
        => _pathStack.Clear();

    /// <summary>
    /// Gets the top path name from the stack.
    /// </summary>
    /// <returns>The top path name</returns>
    public string CurrentName()
    {
        if (_pathStack.IsEmpty)
            return string.Empty;

        if (!_pathStack.TryPeek(out var peeked))
            return string.Empty;

        // not an indexer, use as is
        if (peeked.Indexer != true)
            return peeked.Name;

        // only item, use indexer path
        if (_pathStack.Count == 1)
            return $"{OPENING_BRACKET}{peeked.Name}{CLOSING_BRACKET}";

        // add indexers till property is reached
        var paths = new List<PathValue>();
        var pathList = _pathStack.ToList();

        for (int i = 0; i < pathList.Count; i++)
        {
            var path = pathList[i];
            paths.Add(path);
            if (path.Indexer != true)
                break;
        }

        // create path expression
        return ToPath([.. paths]);
    }

    /// <summary>
    /// Gets the top property name from the stack
    /// </summary>
    /// <returns>The top property name</returns>
    public string CurrentProperty()
    {
        if (_pathStack.Count == 0)
            return string.Empty;

        // find first none indexer path
        var lastProperty = _pathStack.FirstOrDefault(p => p.Indexer != true);
        return lastProperty.Name ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var array = _pathStack.ToArray();
        return ToPath(array);
    }


    private static string ToPath(PathValue[] values)
    {
        var sb = StringBuilderCache.Acquire();

        // stack is in reverse order
        for (int i = values.Length - 1; i >= 0; i--)
        {
            var value = values[i];

            if (sb.Length > 0 && value.Separator.HasValue)
                sb.Append(value.Separator);

            if (value.Indexer == true)
                sb.Append(OPENING_BRACKET).Append(value.Name).Append(CLOSING_BRACKET);
            else
                sb.Append(value.Name);
        }

        return sb.Release();
    }


    /// <summary>
    /// Parses a path string into a collection of <see cref="PathValue"/> components.
    /// </summary>
    /// <param name="path">The path string to parse. Can contain property names separated by dots and indexers enclosed in brackets.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of <see cref="PathValue"/> objects representing the parsed path components.
    /// Returns an empty list if the path is null or empty.
    /// </returns>
    /// <remarks>
    /// This method parses object graph path expressions commonly used in validation scenarios.
    /// It handles both property access (using dot notation) and indexer access (using bracket notation).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Parsing Logic")]
    public static IReadOnlyList<PathValue> Parse(string path)
    {
        var result = new List<PathValue>();
        if (string.IsNullOrEmpty(path))
            return result;

        var span = path.AsSpan();
        var inIndexer = false;
        var tokenStart = 0;

        for (var i = 0; i < span.Length; i++)
        {
            char current = span[i];

            if (current == DOT_SEPARATOR)
            {
                if (inIndexer)
                    continue; // Dot inside indexer is part of the indexer value

                // save current token as property and start new token
                var propertyToken = CreateProperty(span, tokenStart, i);
                if (propertyToken.HasValue)
                    result.Add(propertyToken.Value);

                tokenStart = i + 1;
            }
            else if (current == OPENING_BRACKET)
            {
                if (inIndexer)
                    continue; // Nested brackets - treat as part of indexer value

                // save current token as property and start new token
                var propertyToken = CreateProperty(span, tokenStart, i);
                if (propertyToken.HasValue)
                    result.Add(propertyToken.Value);

                tokenStart = i + 1;
                inIndexer = true;
            }
            else if (current == CLOSING_BRACKET)
            {
                if (!inIndexer)
                    continue; // Invalid - closing bracket without opening - treat as regular character

                // save current token as indexer and start new token
                var indexerToken = CreateIndexer(span, tokenStart, i);
                if (indexerToken.HasValue)
                    result.Add(indexerToken.Value);

                tokenStart = i + 1;
                inIndexer = false;
            }
        }

        // Handle final token if any
        if (tokenStart < span.Length)
        {
            var tokenValue = span[tokenStart..].ToString();
            result.Add(new PathValue(tokenValue));
        }

        return result;
    }

    private static PathValue? CreateProperty(ReadOnlySpan<char> span, int tokenStart, int tokenEnd)
    {
        if (tokenEnd > tokenStart)
        {
            var tokenValue = span[tokenStart..tokenEnd].ToString();
            return new PathValue(tokenValue, Separator: DOT_SEPARATOR);
        }

        return null;
    }

    private static PathValue? CreateIndexer(ReadOnlySpan<char> span, int tokenStart, int tokenEnd)
    {
        if (tokenEnd > tokenStart)
        {
            var tokenValue = span[tokenStart..tokenEnd].ToString();
            return new PathValue(tokenValue, Indexer: true);
        }

        // Empty indexer
        return new PathValue(string.Empty, Indexer: true);
    }
}
