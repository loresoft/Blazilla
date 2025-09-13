using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class ProjectValidator : AbstractValidator<Project>
{
    public const string NameRequired = "Project name is required";
    public const string StartDateValid = "Start date must be valid";
    public const string EndDateAfterStart = "End date must be after start date";
    public const string BudgetMin = "Project budget must be greater than 0";
    public const string TeamMembersRequired = "Project must have at least one team member";

    public ProjectValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage(NameRequired)
            .MaximumLength(200);

        RuleFor(p => p.Description)
            .MaximumLength(1000);

        RuleFor(p => p.StartDate)
            .NotEmpty().WithMessage(StartDateValid);

        RuleFor(p => p.EndDate)
            .GreaterThan(p => p.StartDate).WithMessage(EndDateAfterStart)
            .When(p => p.EndDate.HasValue);

        RuleFor(p => p.Budget)
            .GreaterThan(0).WithMessage(BudgetMin);

        RuleForEach(p => p.Milestones)
            .SetValidator(new ProjectMilestoneValidator());

        // Ensure milestones are within project timeline
        RuleFor(p => p)
            .Must(p => !p.EndDate.HasValue || p.Milestones.All(m => m.DueDate <= p.EndDate))
            .WithMessage("All milestone due dates must be before project end date")
            .WithName("MilestoneTimelineConsistency")
            .When(p => p.EndDate.HasValue);
    }
}
