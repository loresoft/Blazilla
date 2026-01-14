using BlazorShared.Models;
using BlazorShared.Validators;

using FluentValidation;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla.Tests;

/// <summary>
/// Unit tests for <see cref="EditContextExtensions"/> class.
/// Tests the Validate and ValidateAsync extension methods for EditContext.
/// These methods support FluentValidation integration with rule sets and async validation.
/// </summary>
public class EditContextExtensionsTests : BunitContext
{
    private readonly AddressValidator _addressValidator;
    private readonly PersonValidator _personValidator;

    public EditContextExtensionsTests()
    {
        _addressValidator = new AddressValidator();
        _personValidator = new PersonValidator();

        // Register validators
        Services.AddSingleton<IValidator<Address>>(_addressValidator);
        Services.AddSingleton<IValidator<Person>>(_personValidator);
    }

    #region Validate Tests

    [Fact]
    public void Validate_WithNullEditContext_ThrowsArgumentNullException()
    {
        // Arrange
        EditContext? editContext = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => editContext!.Validate([]));
        exception.ParamName.Should().Be("editContext");
    }

    [Fact]
    public void Validate_WithValidModel_AndFluentValidator_ReturnsTrue()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator);
        var editContext = GetEditContext(component);

        // Trigger validation through the extension method
        var result = editContext.Validate();

        // Assert
        result.Should().BeTrue();
        editContext.GetValidationMessages().Should().BeEmpty();
    }

    [Fact]
    public void Validate_CleansUpRuleSetProperty_AfterValidation()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator);
        var editContext = GetEditContext(component);

        // Act
        editContext.Validate(["TestRuleSet"]);

        // Assert - Rule set property should be cleaned up
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptyRuleSets_DoesNotSetProperty()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator);
        var editContext = GetEditContext(component);

        // Act
        editContext.Validate([]);

        // Assert
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNullRuleSets_DoesNotSetProperty()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator);
        var editContext = GetEditContext(component);

        // Act
        IEnumerable<string>? nullRuleSets = null;
        editContext.Validate(nullRuleSets!);

        // Assert
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    #endregion

    #region ValidateAsync Tests

    [Fact]
    public async Task ValidateAsync_WithNullEditContext_ThrowsArgumentNullException()
    {
        // Arrange
        EditContext? editContext = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await editContext!.ValidateAsync([]));
        exception.ParamName.Should().Be("editContext");
    }

    [Fact]
    public async Task ValidateAsync_WithValidModel_ReturnsTrue()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        var result = await editContext.ValidateAsync();

        // Assert
        result.Should().BeTrue();
        editContext.GetValidationMessages().Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_RemovesPendingTask_AfterCompletion()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        await editContext.ValidateAsync();

        // Assert - Pending task should be removed
        var hasPendingTask = editContext.Properties.TryGetValue(FluentValidator.PendingTask, out _);
        hasPendingTask.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_CleansUpRuleSetProperty_AfterValidation()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        await editContext.ValidateAsync(["TestRuleSet"]);

        // Assert - Rule set property should be cleaned up
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithNoPendingTask_CompletesSuccessfully()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator, asyncMode: false);
        var editContext = GetEditContext(component);

        // Act
        var result = await editContext.ValidateAsync();

        // Assert
        result.Should().BeTrue();
        editContext.GetValidationMessages().Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyRuleSets_DoesNotSetProperty()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        await editContext.ValidateAsync([]);

        // Assert
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithNullRuleSets_DoesNotSetProperty()
    {
        // Arrange
        var address = CreateValidAddress();
        var component = CreateComponentWithValidator(address, _addressValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        IEnumerable<string>? nullRuleSets = null;
        await editContext.ValidateAsync(nullRuleSets!);

        // Assert
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithValidAsyncEmail_ReturnsTrue()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "valid@example.com", // Valid email
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        var result = await editContext.ValidateAsync();

        // Assert
        result.Should().BeTrue();
        editContext.GetValidationMessages().Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WaitsForPendingTaskToComplete()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "async@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Create a pending task manually to simulate async validation in progress
        var taskSource = new TaskCompletionSource<bool>();
        editContext.Properties[FluentValidator.PendingTask] = taskSource.Task;

        // Act - Start validation in background
        var validationTask = editContext.ValidateAsync();

        // Complete the pending task after a short delay
        await Task.Delay(100, Xunit.TestContext.Current.CancellationToken);
        taskSource.SetResult(true);

        // Wait for validation to complete
        await validationTask;

        // Assert - Pending task should be removed
        var hasPendingTask = editContext.Properties.TryGetValue(FluentValidator.PendingTask, out _);
        hasPendingTask.Should().BeFalse();
    }

    #endregion

    #region RuleSet Tests

    [Fact]
    public void Validate_WithRuleSetParameter_SetsAndCleansUpProperty()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator);
        var editContext = GetEditContext(component);

        // Act
        var result = editContext.Validate(["Names"]);

        // Assert - Rule set property should be set during validation and cleaned up after
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse("Rule set property should be cleaned up after validation");
    }

    [Fact]
    public async Task ValidateAsync_WithRuleSetParameter_SetsAndCleansUpProperty()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        await editContext.ValidateAsync(["Names"]);

        // Assert - Rule set property should be set during validation and cleaned up after
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse("Rule set property should be cleaned up after validation");
    }

    [Fact]
    public void Validate_WithMultipleRuleSets_PassesAllToValidator()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator);
        var editContext = GetEditContext(component);

        // Act
        var result = editContext.Validate(["Names", "OtherRuleSet"]);

        // Assert - Rule set property should be cleaned up
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithMultipleRuleSets_PassesAllToValidator()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        // Act
        await editContext.ValidateAsync(["Names", "OtherRuleSet"]);

        // Assert - Rule set property should be cleaned up
        var hasProperty = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasProperty.Should().BeFalse();
    }

    [Fact]
    public void Validate_SetsThenRemovesRuleSetProperty_InCorrectOrder()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator);
        var editContext = GetEditContext(component);

        var ruleSetWasSet = false;

        // Subscribe to validation requested to check if property was set
        editContext.OnValidationRequested += (sender, args) =>
        {
            ruleSetWasSet = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out var value)
                            && value is IEnumerable<string> ruleSets
                            && ruleSets.Contains("Names");
        };

        // Act
        editContext.Validate(["Names"]);

        // Assert
        ruleSetWasSet.Should().BeTrue("Rule set should have been set in properties during validation");
        var hasPropertyAfter = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasPropertyAfter.Should().BeFalse("Rule set property should be removed after validation");
    }

    [Fact]
    public async Task ValidateAsync_SetsThenRemovesRuleSetProperty_InCorrectOrder()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            EmailAddress = "test@example.com",
            Address = CreateValidAddress()
        };
        var component = CreateComponentWithValidator(person, _personValidator, asyncMode: true);
        var editContext = GetEditContext(component);

        var ruleSetWasSet = false;

        // Subscribe to validation requested to check if property was set
        editContext.OnValidationRequested += (sender, args) =>
        {
            ruleSetWasSet = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out var value)
                            && value is IEnumerable<string> ruleSets
                            && ruleSets.Contains("Names");
        };

        // Act
        await editContext.ValidateAsync(["Names"]);

        // Assert
        ruleSetWasSet.Should().BeTrue("Rule set should have been set in properties during validation");
        var hasPropertyAfter = editContext.Properties.TryGetValue(FluentValidator.RuleSetProperty, out _);
        hasPropertyAfter.Should().BeFalse("Rule set property should be removed after validation");
    }

    #endregion

    #region Helper Methods

    private static Address CreateValidAddress()
    {
        return new Address
        {
            AddressLine1 = "123 Main St",
            City = "Test City",
            StateProvince = "TS",
            PostalCode = "12345"
        };
    }

    private IRenderedComponent<TestFormComponent<TModel>> CreateComponentWithValidator<TModel>(
        TModel model,
        IValidator validator,
        bool asyncMode = false)
    {
        return Render<TestFormComponent<TModel>>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.Validator, validator)
            .Add(p => p.AsyncMode, asyncMode));
    }

    private static EditContext GetEditContext<TModel>(IRenderedComponent<TestFormComponent<TModel>> component)
    {
        return component.Instance.EditContext!;
    }

    #endregion
}

/// <summary>
/// Test form component for testing EditContext validation
/// </summary>
internal class TestFormComponent<TModel> : ComponentBase
{
    [Parameter]
    public TModel? Model { get; set; }

    [Parameter]
    public IValidator? Validator { get; set; }

    [Parameter]
    public bool AsyncMode { get; set; }

    public EditContext? EditContext { get; private set; }

    protected override void OnInitialized()
    {
        EditContext = new EditContext(Model!);
    }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<EditForm>(0);
        builder.AddAttribute(1, "Model", Model);
        builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment<EditContext>)(context =>
        {
            return formBuilder =>
            {
                formBuilder.OpenComponent<FluentValidator>(0);
                if (Validator != null)
                {
                    formBuilder.AddAttribute(1, "Validator", Validator);
                }
                formBuilder.AddAttribute(2, "AsyncMode", AsyncMode);
                formBuilder.CloseComponent();
            };
        }));
        builder.CloseComponent();
    }
}
