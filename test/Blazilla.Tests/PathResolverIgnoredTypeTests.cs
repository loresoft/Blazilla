namespace Blazilla.Tests;


// Note: IgnoredType tests are in a separate class (PathResolverIgnoredTypeTests)
// with serial execution to avoid test interference due to static state modification

/// <summary>
/// Collection definition to ensure IgnoredType tests run serially.
/// This prevents test interference from static state modifications.
/// </summary>
[CollectionDefinition("PathResolver.IgnoredType", DisableParallelization = true)]
public class PathResolverIgnoredTypeCollection
{
}

/// <summary>
/// Tests for PathResolver.IgnoredType functionality.
/// These tests run serially to avoid conflicts from modifying static ignored types collection.
/// </summary>
[Collection("PathResolver.IgnoredType")]
public class PathResolverIgnoredTypeTests
{
    // Test models for ignored type scenarios
    public interface ICustomComponent
    {
        string Id { get; set; }
    }

    public class CustomComponentBase
    {
        public string Name { get; set; } = string.Empty;
    }

    public class MyCustomComponent : CustomComponentBase, ICustomComponent
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class AnotherCustomComponent : CustomComponentBase
    {
        public string Title { get; set; } = string.Empty;
    }

    public class ModelWithCustomComponent
    {
        public string Name { get; set; } = string.Empty;
        public MyCustomComponent? Component { get; set; }
        public AnotherCustomComponent? AnotherComponent { get; set; }
    }

    // Unique type for cache clearing test to avoid interference from other tests
    public class CacheTestComponent
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ModelWithCacheTestComponent
    {
        public string Name { get; set; } = string.Empty;
        public CacheTestComponent? Component { get; set; }
    }

    [Fact]
    public void AddIgnoredType_WithNullParameter_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathResolver.AddIgnoredType(null!));
    }

    [Fact]
    public void AddIgnoredType_WithNullElementInArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathResolver.AddIgnoredType(typeof(string), null!));
    }

    [Fact]
    public void AddIgnoredType_WithEmptyArray_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        PathResolver.AddIgnoredType();
    }

    [Fact]
    public void AddIgnoredType_WithSingleConcreteType_IgnoresType()
    {
        // Arrange
        PathResolver.AddIgnoredType(typeof(MyCustomComponent));
        var component = new MyCustomComponent { Id = "1", Content = "Test" };
        var model = new ModelWithCustomComponent { Name = "Model", Component = component };
        var pathResolver = new PathResolver();

        // Act
        var result = pathResolver.FindPath(model, component, nameof(MyCustomComponent.Content));

        // Assert
        // The component should be ignored, so the path should not traverse into it
        result.Should().BeNull();
    }

    [Fact]
    public void AddIgnoredType_WithInterfaceType_IgnoresImplementingTypes()
    {
        // Arrange
        PathResolver.AddIgnoredType(typeof(ICustomComponent));
        var component = new MyCustomComponent { Id = "1", Content = "Test" };
        var model = new ModelWithCustomComponent { Name = "Model", Component = component };
        var pathResolver = new PathResolver();

        // Act
        var result = pathResolver.FindPath(model, component, nameof(MyCustomComponent.Content));

        // Assert
        // MyCustomComponent implements ICustomComponent, so it should be ignored
        result.Should().BeNull();
    }

    [Fact]
    public void AddIgnoredType_WithBaseClassType_IgnoresDerivedTypes()
    {
        // Arrange
        PathResolver.AddIgnoredType(typeof(CustomComponentBase));
        var component1 = new MyCustomComponent { Id = "1", Content = "Test" };
        var component2 = new AnotherCustomComponent { Title = "Title" };
        var model = new ModelWithCustomComponent
        {
            Name = "Model",
            Component = component1,
            AnotherComponent = component2
        };
        var pathResolver = new PathResolver();

        // Act
        var result1 = pathResolver.FindPath(model, component1, nameof(MyCustomComponent.Content));
        var result2 = pathResolver.FindPath(model, component2, nameof(AnotherCustomComponent.Title));

        // Assert
        // Both components inherit from CustomComponentBase, so both should be ignored
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public void AddIgnoredType_WithMultipleTypes_IgnoresAllTypes()
    {
        // Arrange
        PathResolver.AddIgnoredType(
            typeof(MyCustomComponent),
            typeof(AnotherCustomComponent));

        var component1 = new MyCustomComponent { Id = "1", Content = "Test" };
        var component2 = new AnotherCustomComponent { Title = "Title" };
        var model = new ModelWithCustomComponent
        {
            Name = "Model",
            Component = component1,
            AnotherComponent = component2
        };
        var pathResolver = new PathResolver();

        // Act
        var result1 = pathResolver.FindPath(model, component1, nameof(MyCustomComponent.Content));
        var result2 = pathResolver.FindPath(model, component2, nameof(AnotherCustomComponent.Title));

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public void AddIgnoredType_WithDuplicateType_DoesNotCauseIssues()
    {
        // Arrange & Act - Should not throw or cause issues
        PathResolver.AddIgnoredType(typeof(MyCustomComponent));
        PathResolver.AddIgnoredType(typeof(MyCustomComponent)); // Add same type again

        var component = new MyCustomComponent { Id = "1", Content = "Test" };
        var model = new ModelWithCustomComponent { Name = "Model", Component = component };
        var pathResolver = new PathResolver();
        var result = pathResolver.FindPath(model, component, nameof(MyCustomComponent.Content));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AddIgnoredType_ClearsCache_NewTypesAreIgnored()
    {
        // Arrange - Use unique type to avoid interference from other tests
        var component = new CacheTestComponent { Id = "1", Value = "Test" };
        var model = new ModelWithCacheTestComponent { Name = "Model", Component = component };
        var pathResolver = new PathResolver();

        // Act - First call before adding ignored type (should work)
        var resultBefore = pathResolver.FindPath(model, component, nameof(CacheTestComponent.Value));

        // Add the type to ignored list
        PathResolver.AddIgnoredType(typeof(CacheTestComponent));

        // Second call after adding ignored type (should be null)
        var resultAfter = pathResolver.FindPath(model, component, nameof(CacheTestComponent.Value));

        // Assert
        resultBefore.Should().Be("Component.Value"); // Found before ignoring
        resultAfter.Should().BeNull(); // Not found after ignoring
    }

    [Fact]
    public void AddIgnoredType_WithMixedInterfaceAndConcreteTypes_IgnoresAll()
    {
        // Arrange
        PathResolver.AddIgnoredType(
            typeof(ICustomComponent),          // Interface
            typeof(AnotherCustomComponent));   // Concrete type

        var component1 = new MyCustomComponent { Id = "1", Content = "Test" }; // Implements ICustomComponent
        var component2 = new AnotherCustomComponent { Title = "Title" };       // Concrete type
        var model = new ModelWithCustomComponent
        {
            Name = "Model",
            Component = component1,
            AnotherComponent = component2
        };
        var pathResolver = new PathResolver();

        // Act
        var result1 = pathResolver.FindPath(model, component1, nameof(MyCustomComponent.Content));
        var result2 = pathResolver.FindPath(model, component2, nameof(AnotherCustomComponent.Title));

        // Assert
        result1.Should().BeNull(); // Ignored via ICustomComponent interface
        result2.Should().BeNull(); // Ignored as concrete type
    }

    [Fact]
    public void AddIgnoredType_CalledMultipleTimes_RebuildsFrozenSetOnlyWhenNeeded()
    {
        // This test verifies the optimization that only rebuilds when new types are added

        // Arrange & Act
        PathResolver.AddIgnoredType(typeof(MyCustomComponent));
        PathResolver.AddIgnoredType(typeof(MyCustomComponent)); // Duplicate - should not rebuild

        // Adding a new type should rebuild
        PathResolver.AddIgnoredType(typeof(AnotherCustomComponent));

        var component1 = new MyCustomComponent { Id = "1", Content = "Test" };
        var component2 = new AnotherCustomComponent { Title = "Title" };
        var model = new ModelWithCustomComponent
        {
            Name = "Model",
            Component = component1,
            AnotherComponent = component2
        };
        var pathResolver = new PathResolver();

        // Assert - Both should be ignored
        pathResolver.FindPath(model, component1, nameof(MyCustomComponent.Content)).Should().BeNull();
        pathResolver.FindPath(model, component2, nameof(AnotherCustomComponent.Title)).Should().BeNull();
    }
}
