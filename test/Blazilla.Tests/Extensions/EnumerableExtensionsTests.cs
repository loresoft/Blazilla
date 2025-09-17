using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blazilla.Extensions;

namespace Blazilla.Tests.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ElementAt_WithValidIndex_ReturnsCorrectElement()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAt(1);

        // Assert
        Assert.Equal("second", result);
    }

    [Fact]
    public void ElementAt_WithZeroIndex_ReturnsFirstElement()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAt(0);

        // Assert
        Assert.Equal("first", result);
    }

    [Fact]
    public void ElementAt_WithLastIndex_ReturnsLastElement()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAt(2);

        // Assert
        Assert.Equal("third", result);
    }

    [Fact]
    public void ElementAt_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable source = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => source.ElementAt(0));
    }

    [Fact]
    public void ElementAt_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.ElementAt(-1));
    }

    [Fact]
    public void ElementAt_WithIndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.ElementAt(10));
    }

    [Fact]
    public void ElementAt_WithEmptyCollection_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var list = new ArrayList();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.ElementAt(0));
    }

    [Fact]
    public void ElementAt_WithNonListEnumerable_ReturnsCorrectElement()
    {
        // Arrange
        var enumerable = CreateCustomEnumerable("a", "b", "c");

        // Act
        var result = enumerable.ElementAt(1);

        // Assert
        Assert.Equal("b", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithValidIndex_ReturnsCorrectElement()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAtOrDefault(1);

        // Assert
        Assert.Equal("second", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithZeroIndex_ReturnsFirstElement()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAtOrDefault(0);

        // Assert
        Assert.Equal("first", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithLastIndex_ReturnsLastElement()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAtOrDefault(2);

        // Assert
        Assert.Equal("third", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithNullSource_ReturnsDefault()
    {
        // Arrange
        IEnumerable source = null!;

        // Act
        var result = source.ElementAtOrDefault(0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ElementAtOrDefault_WithNegativeIndex_ReturnsDefault()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAtOrDefault(-1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ElementAtOrDefault_WithIndexOutOfRange_ReturnsDefault()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAtOrDefault(10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ElementAtOrDefault_WithEmptyCollection_ReturnsDefault()
    {
        // Arrange
        var list = new ArrayList();

        // Act
        var result = list.ElementAtOrDefault(0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ElementAtOrDefault_WithIListImplementation_UsesDirectAccess()
    {
        // Arrange
        var list = new ArrayList { "first", "second", "third" };

        // Act
        var result = list.ElementAtOrDefault(1);

        // Assert
        Assert.Equal("second", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithNonListEnumerable_ReturnsCorrectElement()
    {
        // Arrange
        var enumerable = CreateCustomEnumerable("a", "b", "c");

        // Act
        var result = enumerable.ElementAtOrDefault(1);

        // Assert
        Assert.Equal("b", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithNonListEnumerable_IndexOutOfRange_ReturnsDefault()
    {
        // Arrange
        var enumerable = CreateCustomEnumerable("a", "b", "c");

        // Act
        var result = enumerable.ElementAtOrDefault(10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ElementAtOrDefault_WithSingleElementCollection_ReturnsElement()
    {
        // Arrange
        var list = new ArrayList { "only" };

        // Act
        var result = list.ElementAtOrDefault(0);

        // Assert
        Assert.Equal("only", result);
    }

    [Fact]
    public void ElementAtOrDefault_WithSingleElementCollection_IndexOutOfRange_ReturnsDefault()
    {
        // Arrange
        var list = new ArrayList { "only" };

        // Act
        var result = list.ElementAtOrDefault(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ElementAtOrDefault_WithNullElements_ReturnsNull()
    {
        // Arrange
        var list = new ArrayList { "first", null, "third" };

        // Act
        var result = list.ElementAtOrDefault(1);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Creates a custom enumerable that doesn't implement IList to test enumeration fallback
    /// </summary>
    private static IEnumerable CreateCustomEnumerable(params object[] items)
    {
        return new CustomEnumerable(items);
    }

    private class CustomEnumerable : IEnumerable
    {
        private readonly object[] _items;

        public CustomEnumerable(object[] items)
        {
            _items = items;
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
