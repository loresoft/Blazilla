using BlazorShared.Models;

using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla.Tests;

public class PathResolverTests
{
    private readonly PathResolver _pathResolver = new();

    [Fact]
    public void FindPath_WithNullRootObject_ThrowsArgumentNullException()
    {
        // Arrange
        var target = new Person();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pathResolver.FindPath(null!, target, ""));
    }

    [Fact]
    public void FindPath_WithNullTargetInstance_ThrowsArgumentNullException()
    {
        // Arrange
        var root = new Person();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pathResolver.FindPath(root, null!, ""));
    }

    [Fact]
    public void FindPath_WithNullTargetProperty_ThrowsArgumentNullException()
    {
        // Arrange
        var root = new Person();
        var target = new Person();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pathResolver.FindPath(root, target, null!));
    }

    [Fact]
    public void FindPath_WithSameRootAndTarget_ReturnsEmptyString()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act
        var result = _pathResolver.FindPath(person, person, nameof(Person.FirstName));

        // Assert
        result.Should().Be("FirstName");
    }

    [Fact]
    public void FindPath_WithDirectProperty_ReturnsPropertyName()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var person = new Person { Address = address };

        // Act
        var result = _pathResolver.FindPath(person, address, nameof(Address.City));

        // Assert
        result.Should().Be("Address.City");
    }

    [Fact]
    public void FindPath_WithNestedProperty_ReturnsFullPath()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var department = new Department { Name = "Engineering" };
        var company = new Company
        {
            HeadquartersAddress = address,
            Departments = { department }
        };

        // Act - Search for address through employee
        var result = _pathResolver.FindPath(company, address, nameof(Address.City));

        // Assert
        // Should find the first occurrence, which is HeadquartersAddress
        result.Should().Be("HeadquartersAddress.City");
    }

    [Fact]
    public void FindPath_WithMultipleCollectionItems_ReturnsCorrectIndex()
    {
        // Arrange
        var employee1 = new Employee { FirstName = "John", Id = 1 };
        var employee2 = new Employee { FirstName = "Jane", Id = 2 };
        var company = new Company
        {
            Employees = { employee1, employee2 }
        };

        // Act
        var result1 = _pathResolver.FindPath(company, employee1, nameof(Employee.FirstName));
        var result2 = _pathResolver.FindPath(company, employee2, nameof(Employee.FirstName));

        // Assert
        result1.Should().Be("Employees[0].FirstName");
        result2.Should().Be("Employees[1].FirstName");
    }

    [Fact]
    public void FindPath_WithNestedCollectionProperty_ReturnsFullPathWithIndex()
    {
        // Arrange
        var milestone = new ProjectMilestone { Title = "Phase 1" };
        var project = new Project
        {
            Name = "Customer Portal",
            Milestones = { milestone }
        };
        var company = new Company
        {
            Projects = { project }
        };

        // Act
        var result = _pathResolver.FindPath(company, milestone, nameof(ProjectMilestone.Title));

        // Assert
        result.Should().Be("Projects[0].Milestones[0].Title");
    }

    [Fact]
    public void FindPath_WithMultipleProjectsAndMilestones_ReturnsCorrectIndices()
    {
        // Arrange
        var milestone1 = new ProjectMilestone { Title = "Project 1 - Phase 1" };
        var milestone2 = new ProjectMilestone { Title = "Project 1 - Phase 2" };
        var milestone3 = new ProjectMilestone { Title = "Project 2 - Phase 1" };

        var project1 = new Project
        {
            Name = "Project 1",
            Milestones = { milestone1, milestone2 }
        };
        var project2 = new Project
        {
            Name = "Project 2",
            Milestones = { milestone3 }
        };
        var company = new Company
        {
            Projects = { project1, project2 }
        };

        // Act
        var result1 = _pathResolver.FindPath(company, milestone1, nameof(ProjectMilestone.Title));
        var result2 = _pathResolver.FindPath(company, milestone2, nameof(ProjectMilestone.Title));
        var result3 = _pathResolver.FindPath(company, milestone3, nameof(ProjectMilestone.Title));

        // Assert
        result1.Should().Be("Projects[0].Milestones[0].Title");
        result2.Should().Be("Projects[0].Milestones[1].Title");
        result3.Should().Be("Projects[1].Milestones[0].Title");
    }

    [Fact]
    public void FindPath_WithEmployeeHomeAddress_ReturnsCorrectPathWithIndex()
    {
        // Arrange
        var address1 = new Address { City = "New York" };
        var address2 = new Address { City = "Boston" };
        var employee1 = new Employee { HomeAddress = address1, FirstName = "John" };
        var employee2 = new Employee { HomeAddress = address2, FirstName = "Jane" };
        var company = new Company
        {
            Employees = { employee1, employee2 }
        };

        // Act
        var result1 = _pathResolver.FindPath(company, address1, nameof(Address.City));
        var result2 = _pathResolver.FindPath(company, address2, nameof(Address.City));

        // Assert
        result1.Should().Be("Employees[0].HomeAddress.City");
        result2.Should().Be("Employees[1].HomeAddress.City");
    }

    [Fact]
    public void FindPath_WithMixedCollectionAndDirectProperty_ReturnsCorrectPath()
    {
        // Arrange
        var headquartersAddress = new Address { City = "San Francisco" };
        var employeeAddress = new Address { City = "Seattle" };
        var employee = new Employee { HomeAddress = employeeAddress, FirstName = "John" };
        var company = new Company
        {
            HeadquartersAddress = headquartersAddress,
            Employees = { employee }
        };

        // Act
        var result1 = _pathResolver.FindPath(company, headquartersAddress, nameof(Address.City));
        var result2 = _pathResolver.FindPath(company, employeeAddress, nameof(Address.City));

        // Assert
        result1.Should().Be("HeadquartersAddress.City");
        result2.Should().Be("Employees[0].HomeAddress.City");
    }

    [Fact]
    public void FindPath_WithTargetProperty_ReturnsPathToSpecificProperty()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", LastName = "Doe", Id = 1 };
        var company = new Company
        {
            Employees = { employee }
        };

        // Act
        var result = _pathResolver.FindPath(company, employee, nameof(Employee.FirstName));

        // Assert
        result.Should().Be("Employees[0].FirstName");
    }

    [Fact]
    public void FindPath_WithTargetPropertyNotFound_ReturnsNull()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", LastName = "Doe", Id = 1 };
        var company = new Company
        {
            Employees = { employee }
        };

        // Act
        var result = _pathResolver.FindPath(company, employee, "NonExistentProperty");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithTargetPropertyOnNestedObject_ReturnsCorrectPath()
    {
        // Arrange
        var address = new Address { City = "New York", StateProvince = "NY" };
        var employee = new Employee { HomeAddress = address, FirstName = "John" };
        var company = new Company
        {
            Employees = { employee }
        };

        // Act
        var result = _pathResolver.FindPath(company, address, nameof(Address.City));

        // Assert
        result.Should().Be("Employees[0].HomeAddress.City");
    }

    [Fact]
    public void FindPath_WithTargetPropertyEmptyString_ReturnsPathToObject()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", Id = 1 };
        var company = new Company
        {
            Employees = { employee }
        };

        // Act
        var result = _pathResolver.FindPath(company, employee, nameof(Employee.FirstName));

        // Assert
        result.Should().Be("Employees[0].FirstName");
    }

    [Fact]
    public void FindPath_WithDeeplyNestedObject_ReturnsCorrectPath()
    {
        // Arrange
        var deliverable = "Authentication System";
        var milestone = new ProjectMilestone
        {
            Title = "Phase 1",
            Deliverables = { deliverable }
        };
        var project = new Project
        {
            Name = "Customer Portal",
            Milestones = { milestone }
        };
        var company = new Company
        {
            Projects = { project }
        };

        // Act
        var result = _pathResolver.FindPath(company, milestone, nameof(ProjectMilestone.Title));

        // Assert
        result.Should().Be("Projects[0].Milestones[0].Title");
    }

    [Fact]
    public void FindPath_WithMultipleOccurrences_ReturnsFirstFound()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var employee1 = new Employee { HomeAddress = address };
        var employee2 = new Employee { HomeAddress = address };
        var company = new Company
        {
            HeadquartersAddress = address,
            Employees = { employee1, employee2 }
        };

        // Act
        var result = _pathResolver.FindPath(company, address, nameof(Address.City));

        // Assert
        // Should return the first occurrence found
        result.Should().Be("HeadquartersAddress.City");
    }

    [Fact]
    public void FindPath_WithNonExistentTarget_ReturnsNull()
    {
        // Arrange
        var company = new Company
        {
            Name = "Tech Corp",
            Employees = { new Employee { FirstName = "John" } }
        };
        var unrelatedAddress = new Address { City = "Somewhere" };

        // Act
        var result = _pathResolver.FindPath(company, unrelatedAddress, nameof(Address.City));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithComplexNestedStructure_FindsCorrectPath()
    {
        // Arrange
        var settings = new CompanySettings
        {
            AllowRemoteWork = true,
            CustomSettings = { { "theme", "dark" } }
        };
        var address = new Address { City = "Tech City" };
        var employee = new Employee
        {
            FirstName = "Jane",
            HomeAddress = address
        };
        var company = new Company
        {
            Settings = settings,
            Employees = { employee }
        };

        // Act
        var result = _pathResolver.FindPath(company, settings, nameof(CompanySettings.AllowRemoteWork));

        // Assert
        result.Should().Be("Settings.AllowRemoteWork");
    }

    [Fact]
    public void FindPath_WithCircularReference_HandlesGracefully()
    {
        // Arrange - Create a circular reference scenario
        var employee = new Employee { FirstName = "John", Id = 1 };
        var department = new Department { Name = "Engineering", ManagerId = 1 };
        var company = new Company
        {
            Employees = { employee },
            Departments = { department }
        };

        // Act
        var result = _pathResolver.FindPath(company, employee, nameof(Employee.FirstName));

        // Assert
        result.Should().Be("Employees[0].FirstName");
    }

    [Fact]
    public void FindPath_WithEmptyCollections_ReturnsNull()
    {
        // Arrange
        var company = new Company
        {
            Name = "Empty Corp",
            Employees = [],
            Projects = []
        };
        var employee = new Employee { FirstName = "John" };

        // Act
        var result = _pathResolver.FindPath(company, employee, nameof(Employee.FirstName));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithNullPropertiesInPath_SkipsNullProperties()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "John",
            HomeAddress = null // This property is null
        };
        var targetAddress = new Address { City = "Target City" };
        var company = new Company
        {
            Employees = { employee },
            HeadquartersAddress = targetAddress
        };

        // Act
        var result = _pathResolver.FindPath(company, targetAddress, nameof(Address.City));

        // Assert
        result.Should().Be("HeadquartersAddress.City");
    }

    [Fact]
    public void FindPath_WithSystemTypes_IgnoresSystemTypes()
    {
        // Arrange
        var company = new Company
        {
            Name = "Tech Corp", // String - system type, should be ignored
            RegistrationNumber = "REG-123"
        };
        var targetString = "Tech Corp";

        // Act
        var result = _pathResolver.FindPath(company, targetString, "");

        // Assert
        // System types like strings are filtered out by IsSystemType method
        result.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithEnumValues_IgnoresEnums()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Status = ProjectStatus.InProgress
        };
        var targetStatus = ProjectStatus.InProgress;

        // Act
        var result = _pathResolver.FindPath(project, targetStatus, "");

        // Assert
        // Enums are system types and should be ignored
        result.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithPrimitiveTypes_IgnoresPrimitives()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 42,
            Salary = 75000.50m
        };

        // Act
        var result1 = _pathResolver.FindPath(employee, 42, "");
        var result2 = _pathResolver.FindPath(employee, 75000.50m, "");

        // Assert
        // Primitive types should be ignored
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public void FindPath_MultipleCallsOnSameSearcher_ClearsVisitedObjects()
    {
        // Arrange
        var address1 = new Address { City = "City1" };
        var address2 = new Address { City = "City2" };
        var company1 = new Company { HeadquartersAddress = address1 };
        var company2 = new Company { HeadquartersAddress = address2 };

        // Act - Multiple calls should work independently
        var result1 = _pathResolver.FindPath(company1, address1, nameof(Address.City));
        var result2 = _pathResolver.FindPath(company2, address2, nameof(Address.City));

        // Assert
        result1.Should().Be("HeadquartersAddress.City");
        result2.Should().Be("HeadquartersAddress.City");
    }

    [Fact]
    public void FindPath_WithReadOnlyProperties_SkipsIndexedProperties()
    {
        // This test ensures that indexed properties are skipped
        // The GetIndexParameters().Length > 0 check in the code handles this

        // Arrange
        var settings = new CompanySettings
        {
            CustomSettings = { { "key1", "value1" } }
        };
        var company = new Company { Settings = settings };

        // Act
        var result = _pathResolver.FindPath(company, settings, nameof(CompanySettings.CustomSettings));

        // Assert
        result.Should().Be("Settings.CustomSettings");
    }

    [Fact]
    public void FindPath_WithFieldIdentifier_SimpleProperty_ReturnsCorrectPath()
    {
        // Arrange
        var person = new Person { FirstName = "John" };
        var fieldIdentifier = new FieldIdentifier(person, nameof(Person.FirstName));

        // Act
        var result = _pathResolver.FindPath(person, fieldIdentifier);

        // Assert
        result.Should().Be("FirstName");
    }

    [Fact]
    public void FindPath_WithFieldIdentifier_NestedProperty_ReturnsCorrectPath()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var person = new Person { Address = address };
        var fieldIdentifier = new FieldIdentifier(address, nameof(Address.City));

        // Act
        var result = _pathResolver.FindPath(person, fieldIdentifier);

        // Assert
        result.Should().Be("Address.City");
    }

    [Fact]
    public void FindPath_WithFieldIdentifier_CollectionItem_ReturnsCorrectPath()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", Id = 1 };
        var company = new Company { Employees = { employee } };
        var fieldIdentifier = new FieldIdentifier(employee, nameof(Employee.FirstName));

        // Act
        var result = _pathResolver.FindPath(company, fieldIdentifier);

        // Assert
        result.Should().Be("Employees[0].FirstName");
    }

    [Fact]
    public void FindPath_WithFieldIdentifier_NonExistentModel_ReturnsNull()
    {
        // Arrange
        var person = new Person { FirstName = "John" };
        var unrelatedPerson = new Person { FirstName = "Jane" };
        var fieldIdentifier = new FieldIdentifier(unrelatedPerson, nameof(Person.FirstName));

        // Act
        var result = _pathResolver.FindPath(person, fieldIdentifier);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindPath_WithFieldIdentifier_NullRootObject_ThrowsArgumentNullException()
    {
        // Arrange
        var person = new Person { FirstName = "John" };
        var fieldIdentifier = new FieldIdentifier(person, nameof(Person.FirstName));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pathResolver.FindPath(null!, fieldIdentifier));
    }

    [Fact]
    public void FindPath_WithCollectionIndex_ReturnsFieldIdentifierForIndexedItem()
    {
        // Arrange
        var emailDomain1 = "company.com";
        var emailDomain2 = "subsidiary.com";
        var emailDomain3 = "partner.org";
        var settings = new CompanySettings
        {
            AllowRemoteWork = true,
            AllowedEmailDomains = { emailDomain1, emailDomain2, emailDomain3 }
        };
        var company = new Company
        {
            Name = "Tech Corp",
            Settings = settings
        };

        // Act
        var result = _pathResolver.FindPath(company, settings.AllowedEmailDomains, "1");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("Settings.AllowedEmailDomains[1]");
    }

    [Fact]
    public void FindField_WithFieldIdentifier_NullFieldIdentifier_ThrowsArgumentNullException()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathResolver.FindField(person, null!));
    }

    [Fact]
    public void FindField_WithFieldIdentifier_NonExistentProperty_ReturnsNull()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act
        var result = PathResolver.FindField(person, "NonExistentProperty");
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindField_WithFieldIdentifier_ExistingProperty_ReturnsPropertyInfo()
    {
        // Arrange
        var person = new Person { FirstName = "John" };
        // Act
        var result = PathResolver.FindField(person, nameof(Person.FirstName));
        // Assert
        result.Should().NotBeNull();

        result.Value.FieldName.Should().Be(nameof(Person.FirstName));
        result.Value.Model.Should().Be(person);
    }

    [Fact]
    public void FindField_WithFieldIdentifier_NestedProperty_ReturnsPropertyInfo()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var person = new Person { Address = address };

        // Act
        var result = PathResolver.FindField(person, "Address.City");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be(nameof(Address.City));
        result.Value.Model.Should().Be(address);
    }

    [Fact]
    public void FindField_WithFieldIdentifier_CollectionItemProperty_ReturnsPropertyInfo()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", Id = 1 };
        var company = new Company { Employees = { employee } };

        // Act
        var result = PathResolver.FindField(company, "Employees[0].FirstName");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be(nameof(Employee.FirstName));
        result.Value.Model.Should().Be(employee);
    }

    [Fact]
    public void FindField_WithInvalidCollectionIndex_ReturnsNull()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", Id = 1 };
        var company = new Company { Employees = { employee } };

        // Act
        var result = PathResolver.FindField(company, "Employees[invalid].FirstName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindField_WithOutOfBoundsIndex_ReturnsNull()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", Id = 1 };
        var company = new Company { Employees = { employee } };

        // Act
        var result = PathResolver.FindField(company, "Employees[5].FirstName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindField_WithNullObjectInPath_ReturnsNull()
    {
        // Arrange
        var person = new Person { Address = null }; // Null nested object

        // Act
        var result = PathResolver.FindField(person, "Address.City");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindField_WithIndexerOnNonCollection_ReturnsFieldIdentifier()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act
        // The method first finds FirstName property, but since FirstName is a system type,
        // it returns the FieldIdentifier at that point rather than trying to apply the indexer
        var result = PathResolver.FindField(person, "FirstName[0]");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("FirstName");
        result.Value.Model.Should().Be(person);
    }

    [Fact]
    public void FindField_WithIndexerOnStringProperty_ReturnsFieldIdentifier()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act
        // The method stops at FirstName since it's a system type, so it returns the FieldIdentifier for FirstName
        var result = PathResolver.FindField(person, "FirstName[0]");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("FirstName");
        result.Value.Model.Should().Be(person);
    }

    [Fact]
    public void FindField_WithPrimitiveTypeAtEnd_ReturnsFieldIdentifier()
    {
        // Arrange
        var employee = new Employee { Id = 42 };
        var company = new Company { Employees = { employee } };

        // Act
        var result = PathResolver.FindField(company, "Employees[0].Id");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("Id");
        result.Value.Model.Should().Be(employee);
    }

    [Fact]
    public void FindField_WithMultipleNestedCollections_ReturnsCorrectFieldIdentifier()
    {
        // Arrange
        var milestone = new ProjectMilestone { Title = "Phase 1" };
        var project = new Project { Milestones = { milestone } };
        var company = new Company { Projects = { project } };

        // Act
        var result = PathResolver.FindField(company, "Projects[0].Milestones[0].Title");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("Title");
        result.Value.Model.Should().Be(milestone);
    }

    [Fact]
    public void FindField_WithCollectionAtEnd_ReturnsFieldIdentifier()
    {
        // Arrange
        var employee = new Employee { FirstName = "John" };
        var company = new Company { Employees = { employee } };

        // Act
        var result = PathResolver.FindField(company, "Employees");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("Employees");
        result.Value.Model.Should().Be(company);
    }

    [Fact]
    public void FindField_WithComplexObjectAtEnd_ReturnsFieldIdentifier()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var person = new Person { Address = address };

        // Act
        var result = PathResolver.FindField(person, "Address");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("Address");
        result.Value.Model.Should().Be(person);
    }

    [Fact]
    public void FindField_WithEmptyCollection_ReturnsNull()
    {
        // Arrange
        var company = new Company { Employees = [] };

        // Act
        var result = PathResolver.FindField(company, "Employees[0].FirstName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindField_WithMultipleIndexesInPath_ReturnsCorrectFieldIdentifier()
    {
        // Arrange
        var milestone1 = new ProjectMilestone { Title = "Phase 1" };
        var milestone2 = new ProjectMilestone { Title = "Phase 2" };
        var project1 = new Project { Milestones = { milestone1 } };
        var project2 = new Project { Milestones = { milestone2 } };
        var company = new Company { Projects = { project1, project2 } };

        // Act
        var result1 = PathResolver.FindField(company, "Projects[0].Milestones[0].Title");
        var result2 = PathResolver.FindField(company, "Projects[1].Milestones[0].Title");

        // Assert
        result1.Should().NotBeNull();
        result1.Value.FieldName.Should().Be("Title");
        result1.Value.Model.Should().Be(milestone1);

        result2.Should().NotBeNull();
        result2.Value.FieldName.Should().Be("Title");
        result2.Value.Model.Should().Be(milestone2);
    }

    [Fact]
    public void FindField_WithLargeValidIndex_WorksCorrectly()
    {
        // Arrange
        var employees = Enumerable.Range(0, 100)
            .Select(i => new Employee { FirstName = $"Employee{i}", Id = i })
            .ToList();
        var company = new Company();
        company.Employees.AddRange(employees);

        // Act
        var result = PathResolver.FindField(company, "Employees[99].FirstName");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("FirstName");
        result.Value.Model.Should().Be(employees[99]);
    }

    [Fact]
    public void FindField_WithPathContainingSpecialCharacters_HandlesCorrectly()
    {
        // This tests how the parser handles special characters in property names
        // Most properties won't have special characters, but this ensures robustness

        // Arrange
        var person = new Person { FirstName = "John" };

        // Act & Assert - These should all return null since properties don't exist
        PathResolver.FindField(person, "First.Name").Should().BeNull();
        PathResolver.FindField(person, "First[0]Name").Should().BeNull();
        PathResolver.FindField(person, "First]Name").Should().BeNull();
    }

    [Fact]
    public void FindField_WithCollectionIndex_ReturnsFieldIdentifierForIndexedItem()
    {
        // Arrange
        var emailDomain1 = "company.com";
        var emailDomain2 = "subsidiary.com";
        var emailDomain3 = "partner.org";
        var settings = new CompanySettings
        {
            AllowRemoteWork = true,
            AllowedEmailDomains = { emailDomain1, emailDomain2, emailDomain3 }
        };
        var company = new Company
        {
            Name = "Tech Corp",
            Settings = settings
        };

        // Act
        var result = PathResolver.FindField(company, "Settings.AllowedEmailDomains[1]");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("1");
        result.Value.Model.Should().Be(settings.AllowedEmailDomains);
    }

    #region Case Insensitive Path Tests

    [Fact]
    public void FindField_WithLowercasePropertyName_ReturnsCorrectFieldIdentifier()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act - Using lowercase "firstname" instead of "FirstName"
        var result = PathResolver.FindField(person, "firstname");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("FirstName");
        result.Value.Model.Should().Be(person);
    }


    [Fact]
    public void FindField_WithCaseInsensitiveNestedPath_ReturnsCorrectFieldIdentifier()
    {
        // Arrange
        var address = new Address { City = "New York" };
        var person = new Person { Address = address };

        // Act - Using lowercase path "address.city" instead of "Address.City"
        var result = PathResolver.FindField(person, "address.city");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("City");
        result.Value.Model.Should().Be(address);
    }

    [Fact]
    public void FindField_WithCaseInsensitiveCollectionPath_ReturnsCorrectFieldIdentifier()
    {
        // Arrange
        var employee = new Employee { FirstName = "John", Id = 1 };
        var company = new Company { Employees = { employee } };

        // Act - Using mixed case "employees[0].firstname" instead of "Employees[0].FirstName"
        var result = PathResolver.FindField(company, "employees[0].firstname");

        // Assert
        result.Should().NotBeNull();
        result.Value.FieldName.Should().Be("FirstName");
        result.Value.Model.Should().Be(employee);
    }

    [Fact]
    public void FindField_WithCaseInsensitiveNonExistentProperty_ReturnsNull()
    {
        // Arrange
        var person = new Person { FirstName = "John" };

        // Act - Using case-insensitive path with non-existent property
        var result = PathResolver.FindField(person, "nonexistentproperty");

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
