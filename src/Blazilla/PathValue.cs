namespace Blazilla;

/// <summary>
/// Represents a component of an object graph path expression with metadata about its type and formatting.
/// </summary>
/// <param name="Name">The name or value of the path component (e.g., property name, indexer value, or key).</param>
/// <param name="Separator">The separator character to use when building the path string. Typically a dot (.) for properties.</param>
/// <param name="Indexer">Indicates whether this path component represents an indexer access (enclosed in brackets) or a regular property access.</param>
public readonly record struct PathValue(
    string Name,
    char? Separator = null,
    bool? Indexer = false
);
