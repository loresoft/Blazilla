using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class CompanyValidator : AbstractValidator<Company>
{
    public const string NameRequired = "Company name is required";
    public const string NameMaxLength = "Company name cannot exceed 100 characters";
    public const string RegistrationRequired = "Registration number is required";
    public const string RegistrationPattern = "Registration number must be in format: REG-XXXXXXXX";
    public const string EmployeesRequired = "Company must have at least one employee";
    public const string DepartmentsRequired = "Company must have at least one department";
    public const string ProjectsLimit = "Company cannot have more than 50 active projects";

    public CompanyValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(NameRequired)
            .MaximumLength(100).WithMessage(NameMaxLength);

        RuleFor(c => c.RegistrationNumber)
            .NotEmpty().WithMessage(RegistrationRequired)
            .Matches(@"^REG-\d{8}$").WithMessage(RegistrationPattern);

        RuleFor(c => c.HeadquartersAddress)
            .SetValidator(new AddressValidator()!)
            .When(c => c.HeadquartersAddress != null);

        RuleFor(c => c.Departments)
            .NotEmpty().WithMessage(DepartmentsRequired);

        RuleForEach(c => c.Departments)
            .SetValidator(new DepartmentValidator());

        RuleFor(c => c.Projects)
            .Must(projects => projects.Count(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Planning) <= 50)
            .WithMessage(ProjectsLimit);

        RuleForEach(c => c.Projects)
            .SetValidator(new ProjectValidator());

        RuleFor(c => c.Settings)
            .SetValidator(new CompanySettingsValidator()!)
            .When(c => c.Settings != null);
    }
}
