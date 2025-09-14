using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation.TestHelper;

namespace Blazilla.Tests;

public class ProjectMilestoneValidatorTests
{
    private readonly ProjectMilestoneValidator _validator = new();

    [Fact]
    public void ProjectMilestone_WithValidData_ShouldPass()
    {
        // Arrange
        var milestone = new ProjectMilestone
        {
            Id = 1,
            Title = "Phase 1 Complete",
            Description = "Complete initial setup and authentication",
            DueDate = DateTime.Today.AddMonths(2),
            IsCompleted = false,
            Deliverables = { "Authentication System", "User Management", "Basic UI" }
        };

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Title_WhenEmpty_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Title = "";

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.Title)
            .WithErrorMessage(ProjectMilestoneValidator.TitleRequired);
    }

    [Fact]
    public void Title_WhenNull_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Title = null!;

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.Title)
            .WithErrorMessage(ProjectMilestoneValidator.TitleRequired);
    }

    [Fact]
    public void Title_WhenWhitespace_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Title = "   ";

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.Title)
            .WithErrorMessage(ProjectMilestoneValidator.TitleRequired);
    }

    [Fact]
    public void Title_WhenTooLong_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Title = new string('A', 101); // 101 characters

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.Title);
    }

    [Fact]
    public void Title_WhenAtMaximumLength_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Title = new string('A', 100); // Exactly 100 characters

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Title);
    }

    [Fact]
    public void Title_WhenValid_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Title = "Sprint 1 Completion";

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Title);
    }

    [Fact]
    public void Description_WhenTooLong_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Description = new string('A', 501); // 501 characters

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.Description);
    }

    [Fact]
    public void Description_WhenAtMaximumLength_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Description = new string('A', 500); // Exactly 500 characters

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Description);
    }

    [Fact]
    public void Description_WhenNull_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Description = null;

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Description);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Description = "";

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Description);
    }

    [Fact]
    public void DueDate_WhenEmpty_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.DueDate = default;

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.DueDate)
            .WithErrorMessage(ProjectMilestoneValidator.DueDateRequired);
    }

    [Fact]
    public void DueDate_WhenValid_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.DueDate = DateTime.Today.AddDays(30);

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.DueDate);
    }

    [Fact]
    public void DueDate_WhenInPast_ShouldPass()
    {
        // Arrange - Past dates should be allowed (for completed milestones)
        var milestone = CreateValidMilestone();
        milestone.DueDate = DateTime.Today.AddDays(-30);

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.DueDate);
    }

    [Fact]
    public void Deliverables_WhenEmpty_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Deliverables.Clear();

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.Deliverables)
            .WithErrorMessage(ProjectMilestoneValidator.DeliverablesRequired);
    }

    [Fact]
    public void Deliverables_WhenContainsEmptyString_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Deliverables.Add("");

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor("Deliverables[1]")
            .WithErrorMessage("Deliverable cannot be empty");
    }

    [Fact]
    public void Deliverables_WhenContainsWhitespace_ShouldFail()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Deliverables.Add("   ");

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldHaveValidationErrorFor("Deliverables[1]")
            .WithErrorMessage("Deliverable cannot be empty");
    }

    [Fact]
    public void Deliverables_WhenAllValid_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Deliverables.Clear();
        milestone.Deliverables.AddRange(new[] { "Feature A", "Feature B", "Documentation", "Testing" });

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Deliverables);
    }

    [Fact]
    public void Deliverables_WithSingleValidItem_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.Deliverables.Clear();
        milestone.Deliverables.Add("Single Deliverable");

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.Deliverables);
    }

    [Fact]
    public void IsCompleted_WhenTrue_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.IsCompleted = true;

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IsCompleted_WhenFalse_ShouldPass()
    {
        // Arrange
        var milestone = CreateValidMilestone();
        milestone.IsCompleted = false;

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProjectMilestone_WithMinimalValidData_ShouldPass()
    {
        // Arrange
        var milestone = new ProjectMilestone
        {
            Id = 1,
            Title = "M1",
            DueDate = DateTime.Today,
            IsCompleted = false,
            Deliverables = { "Deliverable" }
        };

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProjectMilestone_WithCompleteData_ShouldPass()
    {
        // Arrange
        var milestone = new ProjectMilestone
        {
            Id = 5,
            Title = "Major Release Milestone",
            Description = "Complete all features for the major product release including testing and documentation",
            DueDate = DateTime.Today.AddMonths(3),
            IsCompleted = false,
            Deliverables = {
                "Core Features Implementation",
                "User Interface Design",
                "API Documentation",
                "Unit Tests",
                "Integration Tests",
                "Performance Testing",
                "Security Review"
            }
        };

        // Act
        var result = _validator.TestValidate(milestone);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static ProjectMilestone CreateValidMilestone()
    {
        return new ProjectMilestone
        {
            Id = 1,
            Title = "Test Milestone",
            Description = "Test milestone description",
            DueDate = DateTime.Today.AddMonths(1),
            IsCompleted = false,
            Deliverables = { "Test Deliverable" }
        };
    }
}
