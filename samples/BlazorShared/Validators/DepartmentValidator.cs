using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class DepartmentValidator : AbstractValidator<Department>
{
    public const string NameRequired = "Department name is required";
    public const string BudgetMin = "Department budget must be greater than 0";
    public const string EmployeesLimit = "Department cannot have more than 100 employees";

    public DepartmentValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty().WithMessage(NameRequired)
            .MaximumLength(100);

        RuleFor(d => d.Description)
            .MaximumLength(500);

        RuleFor(d => d.Budget)
            .GreaterThan(0).WithMessage(BudgetMin);
    }
}
