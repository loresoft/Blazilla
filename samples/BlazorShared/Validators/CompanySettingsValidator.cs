using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class CompanySettingsValidator : AbstractValidator<CompanySettings>
{
    public const string MaxVacationDaysMin = "Max vacation days must be at least 10";
    public const string MaxVacationDaysMax = "Max vacation days cannot exceed 50";
    public const string EmailDomainsRequired = "At least one email domain must be specified";

    public CompanySettingsValidator()
    {
        RuleFor(s => s.MaxVacationDays)
            .GreaterThanOrEqualTo(10).WithMessage(MaxVacationDaysMin)
            .LessThanOrEqualTo(50).WithMessage(MaxVacationDaysMax);

        RuleFor(s => s.AllowedEmailDomains)
            .NotEmpty().WithMessage(EmailDomainsRequired);

        RuleForEach(s => s.AllowedEmailDomains)
            .NotEmpty().WithMessage("Email domain cannot be empty")
            .Must(domain => domain.Contains('.'))
            .WithMessage("Email domain must be valid (contain a dot)");

        RuleForEach(s => s.CustomSettings)
            .Must(kvp => !string.IsNullOrEmpty(kvp.Key))
            .WithMessage("Custom setting key cannot be empty");
    }
}
