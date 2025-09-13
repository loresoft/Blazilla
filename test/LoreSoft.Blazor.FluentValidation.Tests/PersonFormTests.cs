using BlazorShared.Models;
using BlazorShared.Pages;
using BlazorShared.Validators;

namespace LoreSoft.Blazor.FluentValidation.Tests;

public class PersonFormTests : TestContext
{
    public PersonFormTests()
    {
        // Register FluentValidation services
        Services.AddSingleton<IValidator<Person>, PersonValidator>();
        Services.AddSingleton<IValidator<Address>, AddressValidator>();
    }

    [Fact]
    public void PersonForm_ShowsValidationMessages_WhenSubmittedWithEmptyRequiredFields()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();
        var form = component.Find("form");

        // Act - Submit empty form
        form.Submit();

        // Assert
        var validationMessages = component.FindAll(".validation-message");

        validationMessages.Should().NotBeEmpty(); // FirstName, LastName, Age, Email, Address fields are required

        //component.Markup.Should().Contain(PersonValidator.FirstNameRequired);
        //component.Markup.Should().Contain(PersonValidator.LastNameRequired);
        component.Markup.Should().Contain(PersonValidator.AgeRequired);
        component.Markup.Should().Contain(PersonValidator.EmailRequired);

        component.Markup.Should().Contain(AddressValidator.Line1Required);
        component.Markup.Should().Contain(AddressValidator.CityRequired);
        component.Markup.Should().Contain(AddressValidator.StateProvinceRequired);
        component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);
    }

    [Fact]
    public void PersonForm_DoesNotShowValidationMessages_WhenValidDataIsEntered()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Fill in valid data
        component.Find("#firstName").Change("John");
        component.Find("#lastName").Change("Doe");
        component.Find("#age").Change("30");
        component.Find("#email").Change("john.doe@example.com");
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");

        var form = component.Find("form");
        form.Submit();

        // Assert
        var validationMessages = component.FindAll(".validation-message");
        foreach (var message in validationMessages)
        {
            message.InnerHtml.Should().BeEmpty();
        }
    }

    [Fact]
    public void PersonForm_ValidationTriggeredOnFieldChange()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Focus and blur without entering data for first name
        component.Find("#firstName").Change("");

        // Assert - Validation should be triggered
        component.WaitForAssertion(() =>
        {
            // only firstName touched
            component.Markup.Should().Contain(PersonValidator.FirstNameRequired);

            component.Markup.Should().NotContain(PersonValidator.LastNameRequired);
            component.Markup.Should().NotContain(PersonValidator.AgeRequired);
            component.Markup.Should().NotContain(PersonValidator.EmailRequired);
            component.Markup.Should().NotContain(AddressValidator.Line1Required);
            component.Markup.Should().NotContain(AddressValidator.CityRequired);
            component.Markup.Should().NotContain(AddressValidator.StateProvinceRequired);
            component.Markup.Should().NotContain(AddressValidator.PostalCodeRequired);

        });
    }

    [Fact]
    public void PersonForm_ValidationTriggeredOnNestedChange()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Focus and blur without entering data for first name
        component.Find("#addressLine1").Change("");

        // Assert - Validation should be triggered
        component.WaitForAssertion(() =>
        {
            // only addressLine1 touched
            component.Markup.Should().Contain(AddressValidator.Line1Required);

            component.Markup.Should().NotContain(PersonValidator.FirstNameRequired);
            component.Markup.Should().NotContain(PersonValidator.LastNameRequired);
            component.Markup.Should().NotContain(PersonValidator.AgeRequired);
            component.Markup.Should().NotContain(PersonValidator.EmailRequired);
            component.Markup.Should().NotContain(AddressValidator.CityRequired);
            component.Markup.Should().NotContain(AddressValidator.StateProvinceRequired);
            component.Markup.Should().NotContain(AddressValidator.PostalCodeRequired);

        });
    }
    [Fact]
    public void PersonForm_PartialDataValidation_ShowsAppropriateErrors()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Fill only some required fields
        component.Find("#firstName").Change("John");
        component.Find("#lastName").Change("Doe");
        // Leave Age and Email empty

        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        // Leave StateProvince and PostalCode empty

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.Markup.Should().NotContain(PersonValidator.FirstNameRequired);
        component.Markup.Should().NotContain(PersonValidator.LastNameRequired);
        component.Markup.Should().Contain(PersonValidator.AgeRequired);
        component.Markup.Should().Contain(PersonValidator.EmailRequired);

        component.Markup.Should().NotContain(AddressValidator.Line1Required);
        component.Markup.Should().NotContain(AddressValidator.CityRequired);
        component.Markup.Should().Contain(AddressValidator.StateProvinceRequired);
        component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);
    }

    [Fact]
    public void PersonForm_OptionalFields_DoNotShowValidationErrors()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Fill only required fields, leave optional fields empty
        component.Find("#firstName").Change("John");
        component.Find("#lastName").Change("Doe");
        component.Find("#age").Change("30");
        component.Find("#email").Change("john.doe@example.com");
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");
        // AddressLine2 and AddressLine3 are optional and should remain empty

        var form = component.Find("form");
        form.Submit();

        // Assert - Should not show validation errors for optional fields
        var validationMessages = component.FindAll(".validation-message");
        foreach (var message in validationMessages)
        {
            message.InnerHtml.Should().BeEmpty();
        }

        // Should submit successfully
        component.WaitForAssertion(() =>
        {
            var successAlert = component.Find(".alert-success");
            successAlert.Should().NotBeNull();
        });
    }

    [Fact]
    public void PersonForm_ShowsEmailValidationError_ForInvalidEmail()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Fill in data with invalid email
        component.Find("#firstName").Change("John");
        component.Find("#lastName").Change("Doe");
        component.Find("#age").Change("30");
        component.Find("#email").Change("invalid-email");
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.Markup.Should().Contain(PersonValidator.EmailValid);
    }

    [Fact]
    public void PersonForm_ShowsUniqueEmailValidationError_ForDuplicateEmail()
    {
        // Arrange
        var component = RenderComponent<PersonForm>();

        // Act - Fill in data with duplicate email
        component.Find("#firstName").Change("John");
        component.Find("#lastName").Change("Doe");
        component.Find("#age").Change("30");
        component.Find("#email").Change(PersonValidator.DuplicateEmail);
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");

        var form = component.Find("form");
        form.Submit();

        // Assert - Wait for async validation to complete
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().Contain(PersonValidator.EmailUnique);
        }, TimeSpan.FromSeconds(1));
    }
}
