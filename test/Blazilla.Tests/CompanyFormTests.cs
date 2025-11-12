using BlazorShared.Models;
using BlazorShared.Pages;
using BlazorShared.Validators;

namespace Blazilla.Tests;

public class CompanyFormTests : BunitContext
{
    private readonly ITestOutputHelper _outputHelper;

    public CompanyFormTests(ITestOutputHelper outputHelper)
    {
        // Register FluentValidation services
        Services.AddSingleton<IValidator<Company>, CompanyValidator>();
        Services.AddSingleton<IValidator<Address>, AddressValidator>();
        Services.AddSingleton<IValidator<Employee>, EmployeeValidator>();
        Services.AddSingleton<IValidator<Department>, DepartmentValidator>();
        Services.AddSingleton<IValidator<Project>, ProjectValidator>();
        Services.AddSingleton<IValidator<ProjectMilestone>, ProjectMilestoneValidator>();
        Services.AddSingleton<IValidator<CompanySettings>, CompanySettingsValidator>();

        _outputHelper = outputHelper;
    }

    [Fact]
    public void CompanyForm_ShowsValidationMessages_WhenSubmittedWithEmptyRequiredFields()
    {
        // Arrange
        var component = Render<CompanyForm>();
        var form = component.Find("form");

        // Act - Submit empty form
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            var validationMessages = component.FindAll(".validation-message");
            for (var i = 0; i < validationMessages.Count; i++)
            {
                _outputHelper.WriteLine("Validation Message {0}: {1}", i, validationMessages[i].TextContent);
            }

            // Should have validation errors for company name, registration number, address fields, etc.
            component.Markup.Should().Contain(CompanyValidator.NameRequired);
            component.Markup.Should().Contain(CompanyValidator.RegistrationRequired);

            component.Markup.Should().Contain(AddressValidator.Line1Required);
            component.Markup.Should().Contain(AddressValidator.CityRequired);
            component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);

            component.Markup.Should().Contain(DepartmentValidator.NameRequired);
            component.Markup.Should().Contain(DepartmentValidator.BudgetMin);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidatesCompanyBasicFields()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Fill only company name
        component.Find("#companyName").Change("Test Company");

        var form = component.Find("form");
        form.Submit();

        // Assert - Company name should be valid, but other fields should show errors
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().NotContain(CompanyValidator.NameRequired);
            component.Markup.Should().Contain(CompanyValidator.RegistrationRequired);
            component.Markup.Should().Contain(AddressValidator.Line1Required);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidatesRegistrationNumberFormat()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Enter invalid registration number format
        component.Find("#companyName").Change("Test Company");
        component.Find("#registrationNumber").Change("INVALID-FORMAT");

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().Contain(CompanyValidator.RegistrationPattern);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidatesAddressFields()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Fill company fields but leave address incomplete
        component.Find("#companyName").Change("Test Company");
        component.Find("#registrationNumber").Change("REG-12345678");
        component.Find("#addressLine1").Change("123 Main St");
        // Leave city and postal code empty

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().NotContain(CompanyValidator.NameRequired);
            component.Markup.Should().NotContain(CompanyValidator.RegistrationRequired);
            component.Markup.Should().NotContain(AddressValidator.Line1Required);
            component.Markup.Should().Contain(AddressValidator.CityRequired);
            component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidatesDepartmentFields()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Fill company and address fields, but leave department name empty
        FillBasicCompanyData(component);

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().Contain(DepartmentValidator.NameRequired);
            component.Markup.Should().Contain(DepartmentValidator.BudgetMin);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidatesProjectFields()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Fill other data but leave project fields empty
        FillBasicCompanyData(component);

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().Contain(ProjectValidator.NameRequired);
            component.Markup.Should().Contain(ProjectValidator.BudgetMin);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidationTriggeredOnFieldChange()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Change company name field and blur
        component.Find("#companyName").Change("");

        // Assert - Should show validation for touched field only initially
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().Contain(CompanyValidator.NameRequired);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompanyForm_ValidRegistrationNumberFormatPasses()
    {
        // Arrange
        var component = Render<CompanyForm>();

        // Act - Enter valid registration number format
        component.Find("#companyName").Change("Test Company");
        component.Find("#registrationNumber").Change("REG-12345678");

        var form = component.Find("form");
        form.Submit();

        // Assert - Should not show registration pattern error
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().NotContain(CompanyValidator.RegistrationPattern);
        }, TimeSpan.FromSeconds(1));
    }

    private static void FillBasicCompanyData(IRenderedComponent<CompanyForm> component)
    {
        component.Find("#companyName").Change("Test Company");
        component.Find("#registrationNumber").Change("REG-12345678");
        component.Find("#addressLine1").Change("123 Main St");
        component.Find("#city").Change("Test City");
        component.Find("#postalCode").Change("12345");
    }
}
