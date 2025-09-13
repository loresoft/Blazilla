using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class ProjectMilestoneValidator : AbstractValidator<ProjectMilestone>
{
    public const string TitleRequired = "Milestone title is required";
    public const string DueDateRequired = "Milestone due date is required";
    public const string DeliverablesRequired = "Milestone must have at least one deliverable";

    public ProjectMilestoneValidator()
    {
        RuleFor(m => m.Title)
            .NotEmpty().WithMessage(TitleRequired)
            .MaximumLength(100);

        RuleFor(m => m.Description)
            .MaximumLength(500);

        RuleFor(m => m.DueDate)
            .NotEmpty().WithMessage(DueDateRequired);

        RuleFor(m => m.Deliverables)
            .NotEmpty().WithMessage(DeliverablesRequired);

        RuleForEach(m => m.Deliverables)
            .NotEmpty().WithMessage("Deliverable cannot be empty");
    }
}
