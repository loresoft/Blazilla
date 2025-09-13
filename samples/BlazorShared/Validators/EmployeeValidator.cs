using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class EmployeeValidator : AbstractValidator<Employee>
{
    public const string FirstNameRequired = "Employee first name is required";
    public const string LastNameRequired = "Employee last name is required";
    public const string EmailRequired = "Employee email is required";
    public const string EmailValid = "Employee email must be valid";
    public const string PhonePattern = "Phone number must be in format: XXX-XXX-XXXX";
    public const string SalaryMin = "Salary must be greater than 0";
    public const string SalaryMax = "Salary cannot exceed $1,000,000";
    public const string HireDateFuture = "Hire date cannot be in the future";
    public const string SkillsRequired = "Employee must have at least one skill";

    public EmployeeValidator()
    {
        RuleFor(e => e.FirstName)
            .NotEmpty().WithMessage(FirstNameRequired)
            .MaximumLength(50);

        RuleFor(e => e.LastName)
            .NotEmpty().WithMessage(LastNameRequired)
            .MaximumLength(50);

        RuleFor(e => e.Email)
            .NotEmpty().WithMessage(EmailRequired)
            .EmailAddress().WithMessage(EmailValid);

        RuleFor(e => e.PhoneNumber)
            .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage(PhonePattern)
            .When(e => !string.IsNullOrEmpty(e.PhoneNumber));

        RuleFor(e => e.Salary)
            .GreaterThan(0).WithMessage(SalaryMin)
            .LessThanOrEqualTo(1000000).WithMessage(SalaryMax);

        RuleFor(e => e.HireDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage(HireDateFuture);

        RuleFor(e => e.Skills)
            .NotEmpty().WithMessage(SkillsRequired);

        RuleForEach(e => e.Skills)
            .NotEmpty().WithMessage("Skill cannot be empty");

        RuleFor(e => e.HomeAddress)
            .SetValidator(new AddressValidator()!)
            .When(e => e.HomeAddress != null);
    }
}
