using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation.TestHelper;

namespace LoreSoft.Blazor.FluentValidation.Tests;

public class CompanyValidatorTests
{
    private readonly CompanyValidator _companyValidator = new();

    [Fact]
    public void Company_WithValidData_ShouldPass()
    {
        // Arrange
        var company = CreateValidCompany();

        // Act
        var result = _companyValidator.TestValidate(company);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Company_WithEmptyName_ShouldFail()
    {
        // Arrange
        var company = CreateValidCompany();
        company.Name = "";

        // Act
        var result = _companyValidator.TestValidate(company);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage(CompanyValidator.NameRequired);
    }

    [Fact]
    public void Company_WithInvalidRegistrationNumber_ShouldFail()
    {
        // Arrange
        var company = CreateValidCompany();
        company.RegistrationNumber = "INVALID";

        // Act
        var result = _companyValidator.TestValidate(company);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.RegistrationNumber)
            .WithErrorMessage(CompanyValidator.RegistrationPattern);
    }

    private static Company CreateValidCompany()
    {
        var department = new Department
        {
            Id = 1,
            Name = "Engineering",
            Description = "Software Development Department",
            Budget = 500000
        };

        var employee = CreateValidEmployee();
        employee.DepartmentId = department.Id;

        department.ManagerId = employee.Id;

        var project = CreateValidProject();

        employee.AssignedProjects.Add(project.Id);

        return new Company
        {
            Name = "Tech Corp",
            RegistrationNumber = "REG-12345678",
            HeadquartersAddress = new Address
            {
                AddressLine1 = "123 Main St",
                AddressLine2 = "Suite 100",
                City = "Tech City",
                StateProvince = "CA",
                PostalCode = "12345"
            },
            Departments = { department },
            Projects = { project },
            Settings = new CompanySettings
            {
                AllowRemoteWork = true,
                MaxVacationDays = 25,
                AllowedEmailDomains = { "techcorp.com", "company.com" },
                CustomSettings = { { "theme", "dark" }, { "language", "en-US" } }
            }
        };
    }

    private static Employee CreateValidEmployee()
    {
        return new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@techcorp.com",
            PhoneNumber = "555-123-4567",
            Salary = 75000,
            HireDate = DateTime.Today.AddYears(-2),
            DepartmentId = 1,
            Skills = { "C#", "Blazor", "ASP.NET Core" },
            HomeAddress = new Address
            {
                AddressLine1 = "456 Oak St",
                City = "Hometown",
                StateProvince = "CA",
                PostalCode = "54321"
            }
        };
    }

    private static Project CreateValidProject()
    {
        return new Project
        {
            Id = 1,
            Name = "Customer Portal",
            Description = "New customer-facing web portal",
            StartDate = DateTime.Today.AddMonths(-6),
            EndDate = DateTime.Today.AddMonths(6),
            Budget = 200000,
            Status = ProjectStatus.InProgress,
            Milestones =
            {
                new ProjectMilestone
                {
                    Id = 1,
                    Title = "Phase 1 Complete",
                    Description = "Complete initial setup and authentication",
                    DueDate = DateTime.Today.AddMonths(2),
                    IsCompleted = false,
                    Deliverables = { "Authentication System", "User Management", "Basic UI" }
                }
            }
        };
    }
}
