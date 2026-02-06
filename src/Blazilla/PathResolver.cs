using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;

using Blazilla.Extensions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla;

/// <summary>
/// Provides functionality to resolve property paths within object graphs for validation purposes.
/// This class can traverse complex object hierarchies including collections and nested objects
/// to find the path to a specific property instance.
/// </summary>
/// <remarks>
/// This PathResolver is required because <see cref="FieldIdentifier"/> only contains the target model
/// and field name, but does not contain information about the parent object graph or the path
/// to reach that model from the root validation object. This class reconstructs that path by
/// traversing the object graph to find where the target instance is located.
/// </remarks>
public class PathResolver
{
    // Cache for type properties to avoid repeated reflection calls
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();

    // Lock for thread-safe updates to the ignored types set
#if NET9_0_OR_GREATER
    private static readonly Lock _ignoredLock = new();
#else
    private static readonly object _ignoredLock = new();
#endif

    // Frozen set of types that should be ignored during path resolution traversal
    // Optimized for read-heavy access patterns with zero thread-safety overhead
    private static FrozenSet<Type> _ignoredTypes;

    // Cache for IsSystemType results to avoid repeated IsAssignableFrom checks
    // Cleared when ignored types are modified to ensure consistency
    private static readonly ConcurrentDictionary<Type, bool> _isSystemTypeCache = new();

    // Track visited objects to prevent infinite loops in circular references
    private readonly HashSet<object> _visitedObjects = [];

    // Track path segments
    private readonly PathStack _pathStack = new();

    // Maximum object graph depth to traverse before giving up
    // Prevents excessive recursion in pathological cases (20 levels is sufficient for even very complex models)
    private const int MaxDepth = 20;
    private int _currentDepth = 0;

    static PathResolver()
    {
        // Initialize with default types to ignore
        _ignoredTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(Uri),
            typeof(Type),
            typeof(IComponent),
        }
        .ToFrozenSet();
    }

    /// <summary>
    /// Finds the property path for a field identifier within an object graph.
    /// </summary>
    /// <param name="rootObject">The root object to start the search from.</param>
    /// <param name="fieldIdentifier">The field identifier containing the target model and field name.</param>
    /// <returns>The property path as a string if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootObject"/> is null.</exception>
    public string? FindPath(object rootObject, in FieldIdentifier fieldIdentifier)
    {
        ArgumentNullException.ThrowIfNull(rootObject);

        return FindPath(rootObject, fieldIdentifier.Model!, fieldIdentifier.FieldName);
    }

    /// <summary>
    /// Finds the property path to a specific property on a target instance within an object graph.
    /// </summary>
    /// <param name="rootObject">The root object to start the search from.</param>
    /// <param name="targetInstance">The target object instance containing the property.</param>
    /// <param name="targetProperty">The name of the target property to find.</param>
    /// <returns>The property path as a string if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rootObject"/>, <paramref name="targetInstance"/>,
    /// or <paramref name="targetProperty"/> is null.
    /// </exception>
    public string? FindPath(object rootObject, object targetInstance, string targetProperty)
    {
        ArgumentNullException.ThrowIfNull(rootObject);
        ArgumentNullException.ThrowIfNull(targetInstance);
        ArgumentNullException.ThrowIfNull(targetProperty);

        // reset state
        _visitedObjects.Clear();
        _pathStack.Clear();
        _currentDepth = 0;

        if (TryFindInCurrent(rootObject, targetInstance, targetProperty))
            return _pathStack.ToString();

        return null;
    }


    /// <summary>
    /// Finds the instance object that contains the specified property path.
    /// This method navigates through the object graph but stops before the final property access,
    /// returning the object that would be used to access the target property.
    /// </summary>
    /// <param name="rootObject">The root object to start the search from.</param>
    /// <param name="path">The property path to navigate. The method returns the object that contains the final property access.</param>
    /// <returns>The object instance that contains the target property if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootObject"/> or <paramref name="path"/> is null.</exception>
    public static FieldIdentifier? FindField(object rootObject, string path)
    {
        ArgumentNullException.ThrowIfNull(rootObject);
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrEmpty(path))
            return null;

        var current = rootObject;
        var segments = PathStack.Parse(path);

        for (int i = 0; i < segments.Count; i++)
        {
            if (current == null)
                return null;

            var segment = segments[i];

            // Handle indexer access
            if (segment.Indexer == true)
            {
                // Current object should be a collection for indexer access
                if (current is not IEnumerable enumerable || current is string)
                    return null;

                // Parse the index value
                if (!int.TryParse(segment.Name, CultureInfo.InvariantCulture, out var index))
                    return null;

                // last segment, return the FieldIdentifier
                if (i == segments.Count - 1)
                    return new FieldIdentifier(current, segment.Name);

                current = enumerable.ElementAtOrDefault(index);
            }
            else
            {
                // Handle property access
                var properties = GetCachedProperties(current.GetType());
                if (!properties.TryGetValue(segment.Name, out var property))
                    return null;

                // last segment, return the FieldIdentifier
                if (i == segments.Count - 1 || IsSystemType(property.PropertyType))
                    return new FieldIdentifier(current, property.Name);

                current = property.GetValue(current);
            }
        }

        return null;
    }


    /// <summary>
    /// Attempts to find the target property within the current object and its nested properties.
    /// Uses depth-first search with circular reference protection.
    /// </summary>
    /// <param name="current">The current object being examined.</param>
    /// <param name="target">The target object instance to find.</param>
    /// <param name="targetProperty">The name of the target property to find.</param>
    /// <returns>True if the target property is found; otherwise, false.</returns>
    private bool TryFindInCurrent(object current, object target, string targetProperty)
    {
        // Prevent excessive recursion depth
        if (_currentDepth >= MaxDepth)
            return false;

        if (ReferenceEquals(current, target))
            return TryFindTargetProperty(current, targetProperty);

        // Avoid revisiting the same object to prevent infinite loops
        if (_visitedObjects.Contains(current))
            return false;

        _currentDepth++;
        _visitedObjects.Add(current);

        try
        {
            var properties = GetCachedProperties(current.GetType());

            foreach (var property in properties.Values)
            {
                if (TryFindInProperty(property, current, target, targetProperty))
                    return true;
            }

            return false;
        }
        finally
        {
            _visitedObjects.Remove(current);
            _currentDepth--;
        }
    }

    /// <summary>
    /// Attempts to find the target property within a specific property of the current object.
    /// Handles both regular object properties and collections.
    /// </summary>
    /// <param name="property">The property to examine.</param>
    /// <param name="current">The current object containing the property.</param>
    /// <param name="target">The target object instance to find.</param>
    /// <param name="targetProperty">The name of the target property to find.</param>
    /// <returns>True if the target property is found within this property; otherwise, false.</returns>
    private bool TryFindInProperty(PropertyInfo property, object current, object target, string targetProperty)
    {
        try
        {
            // Skip indexer properties (properties with parameters)
            if (property.GetIndexParameters().Length > 0)
                return false;

            // Skip system types that won't contain object references
            if (IsSystemType(property.PropertyType))
                return false;

            var value = property.GetValue(current);
            if (value == null)
                return false;

            // Handle collections separately to track indices
            if (value is IEnumerable enumerable and not string)
                return TryFindInCollection(property, enumerable, target, targetProperty);

            // Handle regular object properties
            _pathStack.PushProperty(property.Name);

            var found = TryFindInCurrent(value, target, targetProperty);
            if (!found)
                _pathStack.Pop();

            return found;
        }
        catch (Exception)
        {
            // Skip properties that throw exceptions when accessed
            return false;
        }
    }

    /// <summary>
    /// Attempts to find the target property within a collection property.
    /// Iterates through collection items and tracks indices in the path.
    /// </summary>
    /// <param name="property">The collection property being examined.</param>
    /// <param name="enumerable">The enumerable collection to search through.</param>
    /// <param name="target">The target object instance to find.</param>
    /// <param name="targetProperty">The name of the target property to find.</param>
    /// <returns>True if the target property is found within the collection; otherwise, false.</returns>
    private bool TryFindInCollection(PropertyInfo property, IEnumerable enumerable, object target, string targetProperty)
    {
        var index = 0;

        _pathStack.PushProperty(property.Name);

        if (ReferenceEquals(enumerable, target))
            return TryFindTargetIndexer(enumerable, targetProperty);

        foreach (var item in enumerable)
        {
            // Skip null items
            if (item == null)
            {
                index++;
                continue;
            }

            _pathStack.PushIndex(index);

            if (TryFindInCurrent(item, target, targetProperty))
                return true;

            _pathStack.Pop(); // Remove index

            index++;
        }

        _pathStack.Pop(); // Remove property

        return false;
    }

    /// <summary>
    /// Attempts to find the target indexer within a collection property.
    /// </summary>
    /// <param name="enumerable">The enumerable collection to search through.</param>
    /// <param name="targetProperty">The name of the target property to find.</param>
    /// <returns>True if the target indexer is found within the collection; otherwise, false.</returns>
    private bool TryFindTargetIndexer(IEnumerable enumerable, string targetProperty)
    {
        // indexer only supports integer indices
        if (!int.TryParse(targetProperty, CultureInfo.InvariantCulture, out int index) || index < 0)
            return false;

        // Optimization: Check if source implements IList for direct index access
        if (enumerable is IList list)
        {
            if (index >= list.Count)
                return false;

            // Index is valid, add to path and return
            _pathStack.PushIndex(index);
            return true;
        }

        // Fallback to enumeration for other IEnumerable implementations
        var currentIndex = 0;
        foreach (var item in enumerable)
        {
            // current index matches target index, add to path and return
            if (currentIndex == index)
            {
                _pathStack.PushIndex(index);
                return true;
            }

            currentIndex++;
        }

        return false;
    }

    /// <summary>
    /// Attempts to find the target property directly on the target object.
    /// This method is called when the target object instance has been found.
    /// </summary>
    /// <param name="targetObject">The target object to examine.</param>
    /// <param name="targetProperty">The name of the target property to find.</param>
    /// <returns>True if the target property exists on the target object; otherwise, false.</returns>
    private bool TryFindTargetProperty(object targetObject, string targetProperty)
    {
        var properties = GetCachedProperties(targetObject.GetType());
        if (!properties.TryGetValue(targetProperty, out var property))
            return false;

        _pathStack.PushProperty(property.Name);
        return true;
    }

    /// <summary>
    /// Gets cached property information for a type, using reflection to discover properties if not already cached.
    /// Only includes public, readable instance properties.
    /// </summary>
    /// <param name="type">The type to get properties for.</param>
    /// <returns>A dictionary mapping property names to PropertyInfo objects.</returns>
    private static Dictionary<string, PropertyInfo> GetCachedProperties(Type type)
    {
        return _propertyCache.GetOrAdd(type, t =>
        {
            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        });
    }


    /// <summary>
    /// Adds one or more types to the list of types that should be ignored during path resolution traversal.
    /// Types can be concrete types or interface/base types. For interface and base types,
    /// any type that implements or inherits from them will also be ignored.
    /// This method is thread-safe and can be called at application startup to configure custom types to skip.
    /// When adding multiple types, the FrozenSet is rebuilt only once for optimal performance.
    /// </summary>
    /// <param name="types">One or more types to ignore during traversal.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="types"/> is null or contains null elements.</exception>
    public static void AddIgnoredType(params Type[] types)
    {
        ArgumentNullException.ThrowIfNull(types);

        // Validate all types are non-null
        foreach (var type in types)
            ArgumentNullException.ThrowIfNull(type, nameof(types));

        if (types.Length == 0)
            return;

        lock (_ignoredLock)
        {
            // Create new set with all new types
            var newSet = _ignoredTypes.ToHashSet();
            var anyAdded = false;

            foreach (var type in types)
            {
                if (newSet.Add(type))
                    anyAdded = true;
            }

            // Only rebuild the frozen set if we actually added new types
            if (!anyAdded)
                return;

            _ignoredTypes = newSet.ToFrozenSet();

            // Clear the cache since new ignored types might change IsSystemType results
            _isSystemTypeCache.Clear();
        }
    }

    /// <summary>
    /// Determines if a type is a system type that typically won't contain object references
    /// that need to be traversed during path resolution.
    /// Results are cached to avoid expensive reflection operations on subsequent calls.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a system type that should be skipped; otherwise, false.</returns>
    private static bool IsSystemType(Type type)
    {
        // Fast path: primitives, value types, and enums (no cache needed, extremely fast)
        if (type.IsPrimitive || type.IsValueType || type.IsEnum)
            return true;

        // Fast path: nullable types (no cache needed, very fast)
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return true;

        // Check cache first to avoid expensive operations
        if (_isSystemTypeCache.TryGetValue(type, out var cached))
            return cached;

        // Compute the result (expensive operations)
        var result = IsSystemTypeCore(type);

        // Cache the result for subsequent calls
        _isSystemTypeCache.TryAdd(type, result);

        return result;
    }

    /// <summary>
    /// Core logic for determining if a type should be ignored during traversal.
    /// This method performs expensive reflection operations and should only be called
    /// when the result is not cached.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type should be ignored; otherwise, false.</returns>
    private static bool IsSystemTypeCore(Type type)
    {
        // Check if exact type is in the ignored list
        if (_ignoredTypes.Contains(type))
            return true;

        // Check if type implements/inherits any ignored interface or base types
        // This is expensive (reflection-based) but only called once per type
        // IsAssignableFrom works for both interfaces and base classes
        foreach (var ignoredType in _ignoredTypes)
        {
            // Skip checking the exact type we already checked above
            // IsAssignableFrom returns true for exact matches too
            if (ignoredType != type && ignoredType.IsAssignableFrom(type))
                return true;
        }

        return false;
    }
}
