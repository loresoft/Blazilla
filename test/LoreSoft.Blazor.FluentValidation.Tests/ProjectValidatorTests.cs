using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation.TestHelper;

namespace LoreSoft.Blazor.FluentValidation.Tests;

public class ProjectValidatorTests
{
    private readonly ProjectValidator _validator = new();

    [Fact]
    public void Project_WithValidData_ShouldPass()
    {
        // Arrange
        var project = new Project
        {
            Id = 1,
            Name = "Customer Portal",
            Description = "New customer-facing web portal",
            StartDate = DateTime.Today.AddMonths(-1),
            EndDate = DateTime.Today.AddMonths(6),
            Budget = 100000,
            Status = ProjectStatus.InProgress,
            Milestones =
            [
                new ProjectMilestone
                {
                    Id = 1,
                    Title = "Phase 1",
                    DueDate = DateTime.Today.AddMonths(2),
                    IsCompleted = false,
                    Deliverables = { "Authentication", "Basic UI" }
                }
            ]
        };

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Name_WhenEmpty_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.Name = "";

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Name)
            .WithErrorMessage(ProjectValidator.NameRequired);
    }

    [Fact]
    public void Name_WhenNull_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.Name = null!;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Name)
            .WithErrorMessage(ProjectValidator.NameRequired);
    }

    [Fact]
    public void Name_WhenTooLong_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.Name = new string('A', 201); // 201 characters

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Name);
    }

    [Fact]
    public void Name_WhenWithinMaxLength_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.Name = new string('A', 200); // Exactly 200 characters

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Name);
    }

    [Fact]
    public void Description_WhenTooLong_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.Description = new string('A', 1001); // 1001 characters

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Description);
    }

    [Fact]
    public void Description_WhenWithinMaxLength_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.Description = new string('A', 1000); // Exactly 1000 characters

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Description);
    }

    [Fact]
    public void Description_WhenNull_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.Description = null;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Description);
    }

    [Fact]
    public void StartDate_WhenEmpty_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.StartDate = default;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.StartDate)
            .WithErrorMessage(ProjectValidator.StartDateValid);
    }

    [Fact]
    public void EndDate_WhenBeforeStartDate_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.StartDate = DateTime.Today;
        project.EndDate = DateTime.Today.AddDays(-1);

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.EndDate)
            .WithErrorMessage(ProjectValidator.EndDateAfterStart);
    }

    [Fact]
    public void EndDate_WhenAfterStartDate_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.StartDate = DateTime.Today;
        project.EndDate = DateTime.Today.AddDays(30);

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.EndDate);
    }

    [Fact]
    public void EndDate_WhenNull_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.EndDate = null;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.EndDate);
    }

    [Fact]
    public void Budget_WhenZero_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.Budget = 0;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Budget)
            .WithErrorMessage(ProjectValidator.BudgetMin);
    }

    [Fact]
    public void Budget_WhenNegative_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.Budget = -1000;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Budget)
            .WithErrorMessage(ProjectValidator.BudgetMin);
    }

    [Fact]
    public void Budget_WhenPositive_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.Budget = 50000;

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Budget);
    }

    [Fact]
    public void Milestones_WithValidData_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.Milestones.Add(new ProjectMilestone
        {
            Id = 2,
            Title = "Phase 2",
            DueDate = DateTime.Today.AddMonths(4),
            IsCompleted = false,
            Deliverables = { "Advanced Features" }
        });

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Milestones);
    }

    [Fact]
    public void Milestones_WhenEmpty_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.Milestones.Clear();

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Milestones);
    }

    [Fact]
    public void MilestoneTimelineConsistency_WhenMilestoneAfterProjectEnd_ShouldFail()
    {
        // Arrange
        var project = CreateValidProject();
        project.StartDate = DateTime.Today;
        project.EndDate = DateTime.Today.AddMonths(3);
        project.Milestones.Add(new ProjectMilestone
        {
            Id = 1,
            Title = "Late Milestone",
            DueDate = DateTime.Today.AddMonths(6), // After project end date
            IsCompleted = false,
            Deliverables = { "Something" }
        });

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldHaveValidationErrorFor("MilestoneTimelineConsistency")
            .WithErrorMessage("All milestone due dates must be before project end date");
    }

    [Fact]
    public void MilestoneTimelineConsistency_WhenMilestoneBeforeProjectEnd_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.StartDate = DateTime.Today;
        project.EndDate = DateTime.Today.AddMonths(6);
        project.Milestones.Add(new ProjectMilestone
        {
            Id = 1,
            Title = "Early Milestone",
            DueDate = DateTime.Today.AddMonths(3), // Before project end date
            IsCompleted = false,
            Deliverables = { "Something" }
        });

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor("MilestoneTimelineConsistency");
    }

    [Fact]
    public void MilestoneTimelineConsistency_WhenProjectHasNoEndDate_ShouldPass()
    {
        // Arrange
        var project = CreateValidProject();
        project.EndDate = null;
        project.Milestones.Add(new ProjectMilestone
        {
            Id = 1,
            Title = "Any Milestone",
            DueDate = DateTime.Today.AddYears(10), // Far in the future
            IsCompleted = false,
            Deliverables = { "Something" }
        });

        // Act
        var result = _validator.TestValidate(project);

        // Assert
        result.ShouldNotHaveValidationErrorFor("MilestoneTimelineConsistency");
    }

    private static Project CreateValidProject()
    {
        return new Project
        {
            Id = 1,
            Name = "Test Project",
            Description = "Test project description",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(6),
            Budget = 100000,
            Status = ProjectStatus.Planning,
            Milestones = []
        };
    }
}
