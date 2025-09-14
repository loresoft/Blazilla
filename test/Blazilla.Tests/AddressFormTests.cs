using BlazorShared.Models;
using BlazorShared.Pages;
using BlazorShared.Validators;

namespace LoreSoft.Blazor.FluentValidation.Tests;

public class AddressFormTests : TestContext
{
    public AddressFormTests()
    {
        // Register FluentValidation services
        Services.AddSingleton<IValidator<Address>, AddressValidator>();
    }

    [Fact]
    public void AddressForm_ShowsValidationMessages_WhenSubmittedWithEmptyRequiredFields()
    {
        // Arrange
        var component = RenderComponent<AddressForm>();
        var form = component.Find("form");

        // Act - Submit empty form
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            var validationMessages = component.FindAll(".validation-message");
            validationMessages.Should().NotBeEmpty();

            component.Markup.Should().Contain(AddressValidator.Line1Required);
            component.Markup.Should().Contain(AddressValidator.CityRequired);
            component.Markup.Should().Contain(AddressValidator.StateProvinceRequired);
            component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddressForm_DoesNotShowValidationMessages_WhenValidDataIsEntered()
    {
        // Arrange
        var component = RenderComponent<AddressForm>();

        // Act - Fill in valid data
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            var validationMessages = component.FindAll(".validation-message");
            foreach (var message in validationMessages)
            {
                message.InnerHtml.Should().BeEmpty();
            }
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddressForm_SubmitsSuccessfully_WithValidData()
    {
        // Arrange
        var component = RenderComponent<AddressForm>();

        // Act - Fill in valid data
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");

        var form = component.Find("form");
        form.Submit();

        // Assert - Check for success message (this will appear after async delay)
        // We need to wait for the async operation to complete
        component.WaitForAssertion(() =>
        {
            var successAlert = component.Find(".alert-success");
            successAlert.Should().NotBeNull();
            successAlert.InnerHtml.Should().Contain("Address Submitted Successfully!");
            successAlert.InnerHtml.Should().Contain("123 Main Street");
            successAlert.InnerHtml.Should().Contain("New York, NY 10001");
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddressForm_ValidationTriggeredOnFieldChange()
    {
        // Arrange
        var component = RenderComponent<AddressForm>();
        var addressLine1Input = component.Find("#addressLine1");

        // Act - Focus and blur without entering data
        component.Find("#addressLine1").Change("");

        // Assert - Validation should be triggered
        component.WaitForAssertion(() =>
        {
            // only address touched
            component.Markup.Should().Contain(AddressValidator.Line1Required);
            component.Markup.Should().NotContain(AddressValidator.CityRequired);
            component.Markup.Should().NotContain(AddressValidator.StateProvinceRequired);
            component.Markup.Should().NotContain(AddressValidator.PostalCodeRequired);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddressForm_PartialDataValidation_ShowsAppropriateErrors()
    {
        // Arrange
        var component = RenderComponent<AddressForm>();

        // Act - Fill only some required fields
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        // Leave StateProvince and PostalCode empty

        var form = component.Find("form");
        form.Submit();

        // Assert
        component.WaitForAssertion(() =>
        {
            component.Markup.Should().NotContain(AddressValidator.Line1Required);
            component.Markup.Should().NotContain(AddressValidator.CityRequired);
            component.Markup.Should().Contain(AddressValidator.StateProvinceRequired);
            component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddressForm_OptionalFields_DoNotShowValidationErrors()
    {
        // Arrange
        var component = RenderComponent<AddressForm>();

        // Act - Fill only required fields, leave optional fields empty
        component.Find("#addressLine1").Change("123 Main Street");
        component.Find("#city").Change("New York");
        component.Find("#stateProvince").Change("NY");
        component.Find("#postalCode").Change("10001");
        // AddressLine2 and AddressLine3 are optional and should remain empty

        var form = component.Find("form");
        form.Submit();

        // Assert - Should not show validation errors for optional fields
        component.WaitForAssertion(() =>
        {
            var validationMessages = component.FindAll(".validation-message");
            foreach (var message in validationMessages)
            {
                message.InnerHtml.Should().BeEmpty();
            }

            // Should submit successfully
            var successAlert = component.Find(".alert-success");
            successAlert.Should().NotBeNull();
        }, TimeSpan.FromSeconds(1));
    }
}
