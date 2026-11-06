using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation;

using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla.Tests;

/// <summary>
/// Regression tests for the race between populating the validation message store and raising the
/// validation state change that renders those messages.
/// <see cref="EditContextExtensions.ValidateAsync(EditContext)"/> must not return until
/// <see cref="EditContext.NotifyValidationStateChanged"/> has run, otherwise a caller that inspects
/// the rendered output right after awaiting validation races the re-render.
/// </summary>
public class ValidationNotificationTests : BunitContext
{
    // repeat the timing sensitive assertions to catch the ordering race,
    // which previously surfaced only intermittently
    private const int RaceIterations = 25;

    private readonly AddressValidator _addressValidator = new();

    public ValidationNotificationTests()
    {
        Services.AddSingleton<IValidator<Address>>(_addressValidator);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateAsync_RaisesValidationStateChanged_BeforeReturning(bool asyncMode)
    {
        for (var iteration = 0; iteration < RaceIterations; iteration++)
        {
            // Arrange - an address that fails validation
            var component = RenderForm(new Address(), asyncMode);
            var editContext = component.Instance.EditContext!;

            var notifications = 0;
            editContext.OnValidationStateChanged += (_, _) => notifications++;

            // Act
            var result = await editContext.ValidateAsync();

            // Assert - the state change must already have been raised, without waiting for it
            result.Should().BeFalse();
            notifications.Should().BeGreaterThan(0, "ValidateAsync must not return before the validation state change is raised");
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateAsync_RendersValidationMessages_BeforeReturning(bool asyncMode)
    {
        for (var iteration = 0; iteration < RaceIterations; iteration++)
        {
            // Arrange - an address that fails validation
            var component = RenderForm(new Address(), asyncMode);
            var editContext = component.Instance.EditContext!;

            // Act
            var result = await editContext.ValidateAsync();

            // Assert - no WaitForAssertion polling: the messages must already be rendered
            result.Should().BeFalse();
            component.Markup.Should().Contain(AddressValidator.Line1Required);
            component.Markup.Should().Contain(AddressValidator.CityRequired);
            component.Markup.Should().Contain(AddressValidator.StateProvinceRequired);
            component.Markup.Should().Contain(AddressValidator.PostalCodeRequired);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateAsync_ClearsRenderedMessages_BeforeReturning(bool asyncMode)
    {
        // Arrange - start invalid so messages are rendered, then make the model valid
        var address = new Address();
        var component = RenderForm(address, asyncMode);
        var editContext = component.Instance.EditContext!;

        await editContext.ValidateAsync();
        component.Markup.Should().Contain(AddressValidator.CityRequired);

        address.AddressLine1 = "123 Main St";
        address.City = "Test City";
        address.StateProvince = "TS";
        address.PostalCode = "12345";

        // Act
        var result = await editContext.ValidateAsync();

        // Assert - the cleared store must already be reflected in the markup
        result.Should().BeTrue();
        component.Markup.Should().NotContain(AddressValidator.CityRequired);
    }

    private IRenderedComponent<ValidationNotificationFormComponent<Address>> RenderForm(Address address, bool asyncMode)
    {
        return Render<ValidationNotificationFormComponent<Address>>(parameters => parameters
            .Add(p => p.Model, address)
            .Add(p => p.Validator, _addressValidator)
            .Add(p => p.AsyncMode, asyncMode));
    }
}
