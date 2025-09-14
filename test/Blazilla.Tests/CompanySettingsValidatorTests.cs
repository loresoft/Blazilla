using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation.TestHelper;

namespace Blazilla.Tests;

public class CompanySettingsValidatorTests
{
    private readonly CompanySettingsValidator _validator = new();

    [Fact]
    public void CompanySettings_WithValidData_ShouldPass()
    {
        // Arrange
        var settings = new CompanySettings
        {
            AllowRemoteWork = true,
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com", "test.com" },
            CustomSettings = { { "theme", "dark" }, { "language", "en-US" } }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MaxVacationDays_WhenTooLow_ShouldFail()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 5,
            AllowedEmailDomains = { "company.com" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.MaxVacationDays)
            .WithErrorMessage(CompanySettingsValidator.MaxVacationDaysMin);
    }

    [Fact]
    public void MaxVacationDays_WhenTooHigh_ShouldFail()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 60,
            AllowedEmailDomains = { "company.com" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.MaxVacationDays)
            .WithErrorMessage(CompanySettingsValidator.MaxVacationDaysMax);
    }

    [Fact]
    public void MaxVacationDays_WhenAtMinimum_ShouldPass()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 10,
            AllowedEmailDomains = { "company.com" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(s => s.MaxVacationDays);
    }

    [Fact]
    public void MaxVacationDays_WhenAtMaximum_ShouldPass()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 50,
            AllowedEmailDomains = { "company.com" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(s => s.MaxVacationDays);
    }

    [Fact]
    public void AllowedEmailDomains_WhenEmpty_ShouldFail()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.AllowedEmailDomains)
            .WithErrorMessage(CompanySettingsValidator.EmailDomainsRequired);
    }

    [Fact]
    public void AllowedEmailDomains_WhenContainsEmptyDomain_ShouldFail()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com", "" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor("AllowedEmailDomains[1]")
            .WithErrorMessage("Email domain cannot be empty");
    }

    [Fact]
    public void AllowedEmailDomains_WhenContainsInvalidDomain_ShouldFail()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com", "invaliddomain" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor("AllowedEmailDomains[1]")
            .WithErrorMessage("Email domain must be valid (contain a dot)");
    }

    [Fact]
    public void AllowedEmailDomains_WhenAllValid_ShouldPass()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com", "test.org", "example.net" }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(s => s.AllowedEmailDomains);
    }

    [Fact]
    public void CustomSettings_WhenContainsEmptyKey_ShouldFail()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com" },
            CustomSettings = { { "", "value" }, { "validkey", "validvalue" } }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor("CustomSettings[0]")
            .WithErrorMessage("Custom setting key cannot be empty");
    }

    [Fact]
    public void CustomSettings_WhenAllKeysValid_ShouldPass()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com" },
            CustomSettings = { { "theme", "dark" }, { "language", "en-US" }, { "timezone", "UTC" } }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(s => s.CustomSettings);
    }

    [Fact]
    public void CustomSettings_WhenEmpty_ShouldPass()
    {
        // Arrange
        var settings = new CompanySettings
        {
            MaxVacationDays = 25,
            AllowedEmailDomains = { "company.com" },
            CustomSettings = { }
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
