using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation.TestHelper;

namespace Blazilla.Tests;

public class DepartmentValidatorTests
{
    private readonly DepartmentValidator _validator = new();

    [Fact]
    public void Department_WithValidData_ShouldPass()
    {
        // Arrange
        var department = new Department
        {
            Id = 1,
            Name = "Engineering",
            Description = "Software Development Department",
            Budget = 500000,
            ManagerId = 1
        };

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Name_WhenEmpty_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Name = "";

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Name)
            .WithErrorMessage(DepartmentValidator.NameRequired);
    }

    [Fact]
    public void Name_WhenNull_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Name = null!;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Name)
            .WithErrorMessage(DepartmentValidator.NameRequired);
    }

    [Fact]
    public void Name_WhenWhitespace_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Name = "   ";

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Name)
            .WithErrorMessage(DepartmentValidator.NameRequired);
    }

    [Fact]
    public void Name_WhenTooLong_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Name = new string('A', 101); // 101 characters

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Name);
    }

    [Fact]
    public void Name_WhenAtMaximumLength_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Name = new string('A', 100); // Exactly 100 characters

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Name);
    }

    [Fact]
    public void Name_WhenValid_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Name = "Human Resources";

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Name);
    }

    [Fact]
    public void Description_WhenTooLong_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Description = new string('A', 501); // 501 characters

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Description);
    }

    [Fact]
    public void Description_WhenAtMaximumLength_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Description = new string('A', 500); // Exactly 500 characters

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Description);
    }

    [Fact]
    public void Description_WhenNull_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Description = null;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Description);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Description = "";

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Description);
    }

    [Fact]
    public void Budget_WhenZero_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Budget = 0;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Budget)
            .WithErrorMessage(DepartmentValidator.BudgetMin);
    }

    [Fact]
    public void Budget_WhenNegative_ShouldFail()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Budget = -1000;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Budget)
            .WithErrorMessage(DepartmentValidator.BudgetMin);
    }

    [Fact]
    public void Budget_WhenPositive_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Budget = 50000;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Budget);
    }

    [Fact]
    public void Budget_WhenVerySmallPositive_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Budget = 0.01m;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Budget);
    }

    [Fact]
    public void Budget_WhenVeryLarge_ShouldPass()
    {
        // Arrange
        var department = CreateValidDepartment();
        department.Budget = 999999999.99m;

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveValidationErrorFor(d => d.Budget);
    }

    [Fact]
    public void Department_WithMinimalValidData_ShouldPass()
    {
        // Arrange
        var department = new Department
        {
            Id = 1,
            Name = "IT",
            Budget = 1
        };

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Department_WithAllFieldsPopulated_ShouldPass()
    {
        // Arrange
        var department = new Department
        {
            Id = 5,
            Name = "Research and Development",
            Description = "Responsible for innovation, product development, and technology research initiatives",
            Budget = 750000.50m,
            ManagerId = 42
        };

        // Act
        var result = _validator.TestValidate(department);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static Department CreateValidDepartment()
    {
        return new Department
        {
            Id = 1,
            Name = "Engineering",
            Description = "Software Development Department",
            Budget = 500000,
            ManagerId = 1
        };
    }
}
