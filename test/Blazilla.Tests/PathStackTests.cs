namespace LoreSoft.Blazor.FluentValidation.Tests;

public class PathStackTests
{
    [Fact]
    public void ToString_WhenPathStackIsEmpty_ReturnsEmptyString()
    {
        var pathStack = new PathStack();
        var path = pathStack.ToString();

        path.Should().BeEmpty();
    }

    [Fact]
    public void Pop_WhenPathStackIsEmpty_PathRemainsEmpty()
    {
        var pathStack = new PathStack();
        pathStack.Pop();

        var path = pathStack.ToString();

        path.Should().BeEmpty();
    }

    [Fact]
    public void PushIndex_WhenFirstElement_ReturnsIndexerNotation()
    {
        var pathStack = new PathStack();
        pathStack.PushIndex(1);

        var name = pathStack.CurrentName();
        name.Should().Be("[1]");

        var property = pathStack.CurrentProperty();
        property.Should().BeEmpty();

        var path = pathStack.ToString();
        path.Should().Be("[1]");
    }

    [Fact]
    public void PushProperty_WhenFirstElement_ReturnsPropertyName()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("Name");

        var name = pathStack.CurrentName();
        name.Should().Be("Name");

        var property = pathStack.CurrentProperty();
        property.Should().Be("Name");

        var path = pathStack.ToString();
        path.Should().Be("Name");
    }

    [Fact]
    public void PushProperty_WhenNestedProperties_ReturnsDotNotationPath()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("HomeAddress");
        pathStack.PushProperty("AddressLine1");

        var name = pathStack.CurrentName();
        name.Should().Be("AddressLine1");

        var property = pathStack.CurrentProperty();
        property.Should().Be("AddressLine1");

        var path = pathStack.ToString();
        path.Should().Be("HomeAddress.AddressLine1");

        pathStack.Pop();

        name = pathStack.CurrentName();
        name.Should().Be("HomeAddress");

        property = pathStack.CurrentProperty();
        property.Should().Be("HomeAddress");

        var updatedPath = pathStack.ToString();
        updatedPath.Should().Be("HomeAddress");
    }

    [Fact]
    public void PushPropertyAndIndex_WhenCombined_ReturnsPropertyIndexerNotation()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("Items");
        pathStack.PushIndex(0);

        var name = pathStack.CurrentName();
        name.Should().Be("Items[0]");

        var property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        var path = pathStack.ToString();
        path.Should().Be("Items[0]");

        pathStack.Pop();

        pathStack.PushIndex(1);

        name = pathStack.CurrentName();
        name.Should().Be("Items[1]");

        property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        var updatedPath = pathStack.ToString();
        updatedPath.Should().Be("Items[1]");

        pathStack.PushProperty("Name");

        name = pathStack.CurrentName();
        name.Should().Be("Name");

        property = pathStack.CurrentProperty();
        property.Should().Be("Name");

        var finalPath = pathStack.ToString();
        finalPath.Should().Be("Items[1].Name");
    }

    [Fact]
    public void PushIndex_WhenMultipleIndexers_ReturnsChainedIndexerNotation()
    {
        var pathStack = new PathStack();
        pathStack.PushProperty("Items");
        pathStack.PushIndex(0);

        var name = pathStack.CurrentName();
        name.Should().Be("Items[0]");

        var property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        var path = pathStack.ToString();
        path.Should().Be("Items[0]");

        pathStack.PushIndex(0);

        name = pathStack.CurrentName();
        name.Should().Be("Items[0][0]");

        property = pathStack.CurrentProperty();
        property.Should().Be("Items");

        var updatedPath = pathStack.ToString();
        updatedPath.Should().Be("Items[0][0]");
    }


    [Fact]
    public void Parse_WhenPathIsNull_ReturnsEmpty()
    {
        var result = PathStack.Parse(null!).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenPathIsEmpty_ReturnsEmpty()
    {
        var result = PathStack.Parse("").ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenSimpleProperty_ReturnsSinglePathValue()
    {
        var result = PathStack.Parse("Name").ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Name");
        result[0].Separator.Should().BeNull();
        result[0].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenNestedProperties_ReturnsMultiplePathValues()
    {
        var result = PathStack.Parse("HomeAddress.AddressLine1").ToList();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("HomeAddress");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("AddressLine1");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenSingleIndexer_ReturnsIndexerPathValue()
    {
        var result = PathStack.Parse("[1]").ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("1");
        result[0].Separator.Should().BeNull();
        result[0].Indexer.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenPropertyWithIndexer_ReturnsPropertyAndIndexer()
    {
        var result = PathStack.Parse("Items[0]").ToList();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Items");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("0");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenPropertyWithIndexerAndNestedProperty_ReturnsCorrectSequence()
    {
        var result = PathStack.Parse("Items[1].Name").ToList();

        result.Should().HaveCount(3);

        result[0].Name.Should().Be("Items");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("1");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeTrue();

        result[2].Name.Should().Be("Name");
        result[2].Separator.Should().BeNull();
        result[2].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenMultipleIndexers_ReturnsChainedIndexers()
    {
        var result = PathStack.Parse("Items[0][0]").ToList();

        result.Should().HaveCount(3);

        result[0].Name.Should().Be("Items");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("0");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeTrue();

        result[2].Name.Should().Be("0");
        result[2].Separator.Should().BeNull();
        result[2].Indexer.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenComplexNestedPath_ReturnsCorrectSequence()
    {
        var result = PathStack.Parse("Company.Employees[0].HomeAddress.AddressLine1").ToList();

        result.Should().HaveCount(5);

        result[0].Name.Should().Be("Company");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("Employees");
        result[1].Separator.Should().Be('.');
        result[1].Indexer.Should().BeFalse();

        result[2].Name.Should().Be("0");
        result[2].Separator.Should().BeNull();
        result[2].Indexer.Should().BeTrue();

        result[3].Name.Should().Be("HomeAddress");
        result[3].Separator.Should().Be('.');
        result[3].Indexer.Should().BeFalse();

        result[4].Name.Should().Be("AddressLine1");
        result[4].Separator.Should().BeNull();
        result[4].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenStringIndexer_ReturnsStringValue()
    {
        var result = PathStack.Parse("Dictionary[\"key\"]").ToList();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Dictionary");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("\"key\"");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenIndexerContainsDots_TreatsDotsAsPartOfIndexer()
    {
        var result = PathStack.Parse("Dictionary[\"some.key.with.dots\"]").ToList();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Dictionary");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("\"some.key.with.dots\"");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenInvalidClosingBracket_TreatsAsRegularCharacter()
    {
        var result = PathStack.Parse("Property]").ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Property]");
        result[0].Separator.Should().BeNull();
        result[0].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenEmptyIndexer_ReturnsEmptyIndexerValue()
    {
        var result = PathStack.Parse("Items[]").ToList();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Items");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenPathStartsWithDot_HandlesCorrectly()
    {
        var result = PathStack.Parse(".Property").ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Property");
        result[0].Separator.Should().BeNull();
        result[0].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenPathEndsWithDot_HandlesCorrectly()
    {
        var result = PathStack.Parse("Property.").ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Property");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenMultipleDots_HandlesCorrectly()
    {
        var result = PathStack.Parse("Property..NestedProperty").ToList();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Property");
        result[0].Separator.Should().Be('.');
        result[0].Indexer.Should().BeFalse();

        result[1].Name.Should().Be("NestedProperty");
        result[1].Separator.Should().BeNull();
        result[1].Indexer.Should().BeFalse();
    }
}
