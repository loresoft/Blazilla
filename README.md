# Blazilla

A library for using FluentValidation with Blazor

[![Nuget version](https://img.shields.io/nuget/v/Blazilla.svg?logo=nuget)](https://www.nuget.org/packages/Blazilla/)
[![Build Status](https://github.com/loresoft/Blazilla/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Blazilla/actions)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/Blazilla/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Blazilla?branch=main)
[![License](https://img.shields.io/github/license/loresoft/Blazilla.svg)](LICENSE)

## Overview

Blazilla provides seamless integration between [FluentValidation](https://fluentvalidation.net/) and Blazor's `EditForm` component. This library enables you to use FluentValidation's powerful and flexible validation rules with Blazor forms, supporting both Blazor Server and Blazor WebAssembly applications.

## Features

- **Real-time validation** - Validate fields as users type or change values
- **Form-level validation** - Full model validation on form submission
- **Nested object validation** - Support for complex object hierarchies
- **Asynchronous validation** - Built-in support for async validation rules, with no extra configuration
- **Native .NET 11 async form validation** - `EditForm` waits for async rules before `OnValidSubmit` runs
- **Cancellation** - Superseded field validations are canceled on .NET 11
- **Rule sets** - Execute specific groups of validation rules
- **Custom validator selectors** - Fine-grained control over which rules to execute
- **Dependency injection integration** - Automatic validator resolution from DI container
- **AOT compatible** - Supports Blazor WebAssembly AOT and iOS-friendly usage patterns

## Installation

Install the package via NuGet:

```bash
dotnet add package Blazilla
```

Or via Package Manager Console:

```bash
Install-Package Blazilla
```

Blazilla supports .NET 8, .NET 9, .NET 10, and .NET 11.

> **Upgrading from Blazilla 2.x?** Version 3.0 adds .NET 11 support, makes the `AsyncMode` parameter obsolete, and changes how asynchronous validation runs. See the [Migration Guide from 2.x to 3.0](MIGRATION.md).

## Quick Start

### 1. Create a Model

```csharp
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    public string? EmailAddress { get; set; }
}
```

### 2. Create a FluentValidation Validator

```csharp
using FluentValidation;

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(p => p.Age)
            .GreaterThanOrEqualTo(0).WithMessage("Age must be greater than or equal to 0")
            .LessThan(150).WithMessage("Age must be less than 150");

        RuleFor(p => p.EmailAddress)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Please provide a valid email address");
    }
}
```

### 3. Register the Validator

#### Blazor Server (`Program.cs`)

```csharp
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register FluentValidation validators as singletons for better performance
builder.Services.AddSingleton<IValidator<Person>, PersonValidator>();

var app = builder.Build();
// ... rest of configuration
```

#### Blazor WebAssembly (`Program.cs`)

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register FluentValidation validators as singletons for better performance
builder.Services.AddSingleton<IValidator<Person>, PersonValidator>();

await builder.Build().RunAsync();
```

### 4. Use in Blazor Component

```razor
@page "/person-form"

<h3>Person Form</h3>

<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    <ValidationSummary />

    <div class="mb-3">
        <label for="firstName" class="form-label">First Name</label>
        <InputText id="firstName" class="form-control" @bind-Value="person.FirstName" />
        <ValidationMessage For="@(() => person.FirstName)" />
    </div>

    <div class="mb-3">
        <label for="lastName" class="form-label">Last Name</label>
        <InputText id="lastName" class="form-control" @bind-Value="person.LastName" />
        <ValidationMessage For="@(() => person.LastName)" />
    </div>

    <div class="mb-3">
        <label for="age" class="form-label">Age</label>
        <InputNumber id="age" class="form-control" @bind-Value="person.Age" />
        <ValidationMessage For="@(() => person.Age)" />
    </div>

    <div class="mb-3">
        <label for="email" class="form-label">Email</label>
        <InputText id="email" class="form-control" @bind-Value="person.EmailAddress" />
        <ValidationMessage For="@(() => person.EmailAddress)" />
    </div>

    <button type="submit" class="btn btn-primary">Submit</button>
</EditForm>

@code {
    private Person person = new();

    private async Task HandleValidSubmit()
    {
        // Handle successful form submission
        Console.WriteLine("Form submitted successfully!");
    }
}
```

> **Note**: If your validators contain async rules (such as `MustAsync`) and your app targets .NET 8, 9, or 10, see [Asynchronous Validation](#asynchronous-validation).

## Advanced Usage

### Asynchronous Validation

Blazilla supports FluentValidation's async rules (`MustAsync`, `CustomAsync`, and so on) with no extra configuration. Field-level validation always runs through FluentValidation's asynchronous pipeline. How you handle form submission depends on your target framework.

Create a validator with async rules:

```csharp
public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(p => p.EmailAddress)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MustAsync(async (email, cancellationToken) =>
            {
                // Simulate async validation (e.g., database check)
                await Task.Delay(500, cancellationToken);
                return !email?.Equals("admin@example.com", StringComparison.OrdinalIgnoreCase) ?? true;
            }).WithMessage("This email address is not available");
    }
}
```

> **Tip**: Always forward the `CancellationToken` to your async rules and the I/O they perform. On .NET 11, Blazilla cancels a field validation when the user edits the field again, and cancels pending field validations when the form is submitted.

#### .NET 11 and later

.NET 11 adds native asynchronous form validation. Blazilla registers its validation with the `EditContext`, so `EditForm` waits for all validation, including async rules, before it invokes `OnValidSubmit` or `OnInvalidSubmit`. Use `OnValidSubmit` as you would with synchronous rules:

```razor
<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    <ValidationSummary />

    <div class="mb-3">
        <label for="email" class="form-label">Email</label>
        <InputText id="email" class="form-control" @bind-Value="person.EmailAddress" />
        <ValidationMessage For="@(() => person.EmailAddress)" />
    </div>

    <button type="submit" class="btn btn-primary">Submit</button>
</EditForm>

@code {
    private Person person = new();

    private async Task HandleValidSubmit()
    {
        // All validation, including async rules, has passed
        await SaveAsync();
    }
}
```

On .NET 11, you can also use the native `EditContext` APIs to show validation progress and errors:

- `IsValidationPending(() => person.EmailAddress)` returns `true` while a field's async validation is running.
- `IsValidationPending()` returns `true` while form-level validation is running.
- `IsValidationFaulted()` returns `true` if a validator threw an exception during the last form validation. In this case, `ValidateAsync` returns `false`, even if there are no validation messages.

```razor
<EditForm EditContext="@editContext" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />

    <InputText @bind-Value="person.EmailAddress" />
    @if (editContext.IsValidationPending(() => person.EmailAddress))
    {
        <span class="text-muted">Checking availability...</span>
    }
    <ValidationMessage For="@(() => person.EmailAddress)" />

    @if (editContext.IsValidationFaulted())
    {
        <div class="alert alert-danger">Validation couldn't complete. Please try again.</div>
    }

    <button type="submit" disabled="@editContext.IsValidationPending()">Submit</button>
</EditForm>

@code {
    private Person person = new();
    private EditContext editContext = default!;

    protected override void OnInitialized()
    {
        editContext = new EditContext(person);

        // Re-render when async validation starts and completes
        editContext.OnValidationStateChanged += (_, _) => InvokeAsync(StateHasChanged);
    }
}
```

> **Note**: `EditContext` raises `OnValidationStateChanged` when async validation starts and when it completes. Subscribe to it, as shown above, so the component that reads `IsValidationPending` or `IsValidationFaulted` re-renders. Unsubscribe in `Dispose` if the `EditContext` can outlive the component. See the [Person form sample](samples/BlazorShared/Pages/PersonForm.razor) for a complete example.

#### .NET 8, .NET 9, and .NET 10

On .NET 8–10, Blazor's built-in `EditForm` validates synchronously before it invokes `OnValidSubmit`, so it can't wait for async rules. If a validator with async rules runs synchronously, FluentValidation throws `AsyncValidatorInvokedSynchronouslyException`.

Use `OnSubmit` instead of `OnValidSubmit`, and await `ValidateAsync`. This pattern also works on .NET 11, so use it in code that multi-targets:

```razor
<EditForm Model="@person" OnSubmit="@HandleSubmit">
    <FluentValidator />
    <ValidationSummary />
    
    <div class="mb-3">
        <label for="email" class="form-label">Email</label>
        <InputText id="email" class="form-control" @bind-Value="person.EmailAddress" />
        <ValidationMessage For="@(() => person.EmailAddress)" />
    </div>
    
    <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
        @(isSubmitting ? "Validating..." : "Submit")
    </button>
</EditForm>

@code {
    private Person person = new();
    private bool isSubmitting = false;

    private async Task HandleSubmit(EditContext editContext)
    {
        isSubmitting = true;
        StateHasChanged();
        
        try
        {
            // Use ValidateAsync to ensure all async validation completes
            var isValid = await editContext.ValidateAsync();
            
            if (isValid)
            {
                // Form is valid, proceed with submission
                await ProcessValidForm();
            }
            // If invalid, validation messages will be displayed automatically
        }
        finally
        {
            isSubmitting = false;
            StateHasChanged();
        }
    }
    
    private async Task ProcessValidForm()
    {
        // Handle successful form submission
        Console.WriteLine("Form submitted successfully!");
        await Task.Delay(1000); // Simulate form processing
    }
}
```

`ValidateAsync` runs form-level validation asynchronously, waits for every rule (sync and async) to complete, and returns `true` only when there are no validation messages.

> **Important**: On .NET 8–10, use `OnSubmit` with `ValidateAsync` instead of `OnValidSubmit` when validators contain async rules. On .NET 11, `OnValidSubmit` works with async rules.

#### EditContext Extension Methods

| Method | Description |
| ------ | ----------- |
| `ValidateAsync()` | Validates the form and waits for all async rules to complete. On .NET 11, this calls the native `EditContext.ValidateAsync()`. |
| `ValidateAsync(CancellationToken)` | Same as `ValidateAsync()`, with cancellation. On .NET 11, this calls the native method. On .NET 8–10, the token isn't observed. |
| `ValidateAsync(params IEnumerable<string> ruleSets)` | Validates the form with the specified rule sets, overriding the component's `RuleSets`. |
| `ValidateAsync(IEnumerable<string>? ruleSets, CancellationToken)` | Validates the form with the specified rule sets and cancellation. |
| `Validate(params IEnumerable<string> ruleSets)` | Validates the form synchronously. Doesn't support async rules. Obsolete on .NET 11. |

### Rule Sets

Use rule sets to execute specific groups of validation rules:

```csharp
public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        // Default rules (always executed)
        RuleFor(p => p.FirstName).NotEmpty();

        // Rules in specific rule sets
        RuleSet("Create", () =>
        {
            RuleFor(p => p.EmailAddress)
                .NotEmpty()
                .EmailAddress();
        });

        RuleSet("Update", () =>
        {
            RuleFor(p => p.LastName).NotEmpty();
        });
    }
}
```

```razor
<!-- Execute only the "Create" rule set -->
<FluentValidator RuleSets="@(new[] { "Create" })" />

<!-- Execute all rules including those in rule sets -->
<FluentValidator AllRules="true" />
```

To use different rule sets for a single validation call, pass them to `ValidateAsync`. They override the component's `RuleSets` for that call only:

```csharp
// Validate using only the "Create" rule set
var isValid = await editContext.ValidateAsync("Create");

// With cancellation
var isValid = await editContext.ValidateAsync(["Create"], cancellationToken);
```

### Nested Object Validation

Validate complex objects with nested properties:

```csharp
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.Street).NotEmpty().WithMessage("Street is required");
        RuleFor(a => a.City).NotEmpty().WithMessage("City is required");
        RuleFor(a => a.PostalCode).NotEmpty().WithMessage("Postal code is required");
    }
}

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(p => p.FirstName).NotEmpty();
        RuleFor(p => p.LastName).NotEmpty();
        
        // Validate nested Address object
        RuleFor(p => p.Address!)
            .SetValidator(new AddressValidator())
            .When(p => p.Address is not null);
    }
}
```

```razor
<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    
    <!-- Person fields -->
    <InputText @bind-Value="person.FirstName" />
    <ValidationMessage For="@(() => person.FirstName)" />
    
    <!-- Nested address fields -->
    <InputText @bind-Value="person.Address!.Street" />
    <ValidationMessage For="@(() => person.Address!.Street)" />
    
    <InputText @bind-Value="person.Address!.City" />
    <ValidationMessage For="@(() => person.Address!.City)" />
</EditForm>
```

### WebAssembly AOT and iOS

Blazilla supports AOT environments such as Blazor WebAssembly AOT and iOS. For the most trim-friendly path, manually register validators or pass the validator directly to the component.

For Blazor WebAssembly AOT, publish with the Blazor WebAssembly AOT property:

```bash
dotnet publish -c Release -p:RunAOTCompilation=true
```

Recommended registration:

```csharp
using FluentValidation;

builder.Services.AddSingleton<IValidator<Person>, PersonValidator>();
builder.Services.AddSingleton<IValidator<Address>, AddressValidator>();
```

Recommended direct component usage when convenient:

```razor
<FluentValidator Validator="@validator" />

@code {
    private readonly PersonValidator validator = new();
}
```

The assembly-scanning registration helpers remain available, but they are annotated with trim warnings because reflection-based scanning cannot guarantee that validators are preserved after trimming. Avoid assembly scanning for WebAssembly AOT and iOS apps unless you also provide linker preservation for the validator types.

Nested field mapping is preserved in AOT builds. It uses public-property reflection to map Blazor `FieldIdentifier` values to FluentValidation paths, so trimmed apps must preserve public properties on validation model types when those properties are only accessed through reflection.

#### AOT and Trimming Limitations

- Blazilla does not use runtime expression compilation, but it still creates FluentValidation context types for the runtime model type. The model type and FluentValidation types used by your app must be preserved by the application.
- Automatic validator resolution uses the runtime model type to resolve `IValidator<T>` from dependency injection. This works best when validators are manually registered as closed generic services. Passing `Validator` directly is the most explicit AOT-safe option.
- Assembly scanning APIs such as `AddValidatorsFromAssembly` remain available for non-AOT convenience, but they are not trim-safe by themselves. For WebAssembly AOT and iOS, prefer manual registration or add your own linker preservation for scanned validator types.
- Nested path resolution uses reflection over public readable properties. If trimming removes model properties that are only used for validation path mapping, nested field messages may fall back to less precise paths or fail to map to the expected Blazor field.
- Avoid validation rules that rely on unpreserved reflection metadata or runtime code generation in AOT targets. Blazilla avoids expression compilation, but custom validators and third-party rules must also be AOT-friendly.
- For Blazor WebAssembly AOT, use `RunAOTCompilation=true`; `PublishAot=true` is for Native AOT scenarios and is not the correct property for Blazor WebAssembly.

### Custom Validator Instance

Pass a validator instance directly instead of using dependency injection:

```razor
@code {
    private PersonValidator validator = new();
}

<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator Validator="@validator" />
    <!-- form fields -->
</EditForm>
```

### Custom Validator Selector

Implement fine-grained control over which validation rules to execute:

```razor
<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator Selector="@customSelector" />
    <!-- form fields -->
</EditForm>

@code {
    private IValidatorSelector customSelector = new CustomValidatorSelector();
    
    public class CustomValidatorSelector : IValidatorSelector
    {
        public bool CanExecute(IValidationRule rule, string propertyPath, IValidationContext context)
        {
            // Custom logic to determine if a rule should execute
            return true;
        }
    }
}
```

## Component Parameters

The `FluentValidator` component supports the following parameters:

| Parameter   | Type                   | Default | Description                                                                                                                                                                                                                         |
| ----------- | ---------------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Validator` | `IValidator?`          | `null`  | The FluentValidation validator instance to use. If not provided, the component will attempt to resolve a validator from the dependency injection container based on the EditForm's model type.                                      |
| `RuleSets`  | `IEnumerable<string>?` | `null`  | Collection of rule set names to execute. Only validation rules within the specified rule sets will be executed. Use this to run targeted validation scenarios (e.g., "Create", "Update", "Profile").                                |
| `AllRules`  | `bool`                 | `false` | When true, executes all validation rules including those defined within rule sets. When false (default), only executes rules not in any rule set plus any specified in the RuleSets parameter.                                      |
| `AsyncMode` | `bool`                 | `false` | **Obsolete since 3.0 and ignored.** Async rules are supported automatically. Remove this parameter. See the [Migration Guide](MIGRATION.md).                                                                                       |
| `Selector`  | `IValidatorSelector?`  | `null`  | Custom validator selector implementation for advanced scenarios requiring fine-grained control over which validation rules execute. Allows conditional rule execution based on custom logic, property paths, or validation context. |

## Validation Behavior

- **Field-level validation**: Triggered when individual form fields change. Runs asynchronously, so both synchronous and async rules are supported. The field's messages are cleared when validation starts and updated when it completes. On .NET 11, editing a field again cancels its pending validation.
- **Form-level validation**: Triggered on form submission or by calling `ValidateAsync`. Runs asynchronously when requested through `ValidateAsync` (including `EditForm` submission on .NET 11), and synchronously when requested through `EditContext.Validate()` (including `EditForm` submission on .NET 8–10).
- **Real-time feedback**: Validation messages appear as users interact with the form
- **Integration with EditContext**: Works seamlessly with Blazor's built-in validation system, including native async validation on .NET 11

## Performance Considerations

- Validators should be registered as singletons in the DI container since they are stateless and can be safely shared across requests/components
- Validators are cached and reused when resolved from dependency injection
- Validation contexts are created without runtime expression compilation so they can run in AOT environments
- Async rules run through FluentValidation's `ValidateAsync`, so slow checks don't block the UI
- Forward the `CancellationToken` to async rules so canceled validations stop work early

## Integration Details

### Blazor FieldIdentifier to FluentValidation Path Conversion

The library automatically converts Blazor's `FieldIdentifier` objects to FluentValidation property paths to ensure validation messages appear in the correct location. This conversion handles:

**Simple Properties**
```csharp
// Blazor FieldIdentifier: { Model = person, FieldName = "FirstName" }
// FluentValidation path: "FirstName"
```

**Nested Properties**
```csharp
// Blazor FieldIdentifier: { Model = address, FieldName = "Street" }
// FluentValidation path: "Address.Street" (when address is a property of person)
```

**Collection Items**
```csharp
// Blazor FieldIdentifier: { Model = phoneNumber, FieldName = "Number" }
// FluentValidation path: "PhoneNumbers[0].Number" (when phoneNumber is item 0 in a collection)
```

The path conversion is performed internally using object tree analysis to match Blazor's field identification system with FluentValidation's property path conventions. This ensures that validation messages from FluentValidation rules are correctly associated with the corresponding Blazor input components.

### Customizing Path Resolution Type Traversal

The `PathResolver` automatically traverses your object graph to find the correct property path for validation messages. By default, it skips certain types that shouldn't be traversed:

- Primitive types (`int`, `bool`, `double`, etc.)
- Value types and enums
- Common immutable types (`string`, `Uri`, `Type`)
- Blazor components (any type implementing `IComponent`)

#### Why Skip Types During Traversal?

Skipping certain types during path resolution is important for several reasons:

1. **Performance**: Prevents unnecessary traversal of large component trees or complex framework objects that don't contain validation targets
2. **Correctness**: Avoids traversing into framework types that contain circular references or shouldn't be part of validation paths
3. **Stability**: Prevents potential errors from attempting to traverse types with non-standard property access patterns or properties that throw exceptions

#### Adding Custom Ignored Types

For custom scenarios where you need to skip additional types during path resolution, use the `PathResolver.AddIgnoredType` method. This is useful when you have:

- Custom component base classes that should be treated like `IComponent`
- Third-party UI framework types that shouldn't be traversed
- Types with complex internal structures or circular references
- Types with properties that throw exceptions when accessed

**Example Usage:**

```csharp
// In Program.cs during application startup
using Blazilla;

// Add a single type to ignore
PathResolver.AddIgnoredType(typeof(MyCustomComponent));

// Add multiple types at once (more efficient - rebuilds the internal collection only once)
PathResolver.AddIgnoredType(
    typeof(MyCustomComponent),
    typeof(MyFrameworkType),
    typeof(IMyCustomInterface));  // Interface types - any implementing type will be ignored
```

**Real-World Scenario:**

```csharp
// Custom component base class used throughout your application
public abstract class CustomComponentBase : ComponentBase
{
    // Component infrastructure
    public RenderFragment? ChildContent { get; set; }
    // ... other component properties
}

// During startup, register it to be ignored during path resolution
PathResolver.AddIgnoredType(typeof(CustomComponentBase));

// Now any type inheriting from CustomComponentBase will be skipped during traversal,
// preventing unnecessary performance overhead and potential traversal into component internals
```

**Performance Tip**: When adding multiple types, pass them all in a single call to `AddIgnoredType` rather than calling it multiple times. This rebuilds the internal frozen set only once instead of once per type.

**Thread Safety**: The `AddIgnoredType` method is thread-safe and uses a high-performance frozen set internally. However, it's recommended to call this method only during application startup (in `Program.cs`) before any validation occurs to ensure consistent behavior across your application.

### Blazor Forms and Async Validation Limitations

**.NET 11 and later**: Blazor supports asynchronous form validation natively. `EditForm` awaits `EditContext.ValidateAsync()` before it invokes `OnValidSubmit` or `OnInvalidSubmit`, and Blazilla registers its validation with the `EditContext`. No workaround is needed. The synchronous `EditContext.Validate()` method is obsolete in .NET 11.

**.NET 8, .NET 9, and .NET 10**: Blazor's built-in form validation is synchronous:

1. **OnValidSubmit Event Timing**: `EditForm` calls the synchronous `EditContext.Validate()` before it invokes `OnValidSubmit`, so it can't wait for async rules.

2. **EditContext.Validate() Behavior**: `EditContext.Validate()` runs validation synchronously. FluentValidation throws `AsyncValidatorInvokedSynchronouslyException` if a validator with async rules runs synchronously.

**Workaround for .NET 8–10**: When validators contain async rules (`MustAsync`, custom async validators):

- Use `OnSubmit` instead of `OnValidSubmit`
- Call `await editContext.ValidateAsync()` to ensure async validation completes before you process the form

This limitation is part of Blazor's validation architecture before .NET 11 and affects all validation libraries, not just FluentValidation integrations.

## Migration Guide from Blazored.FluentValidation

If you're currently using [Blazored.FluentValidation](https://github.com/Blazored/FluentValidation), migrating to Blazilla is straightforward. The libraries share similar APIs and functionality, with some key improvements in Blazilla.

### Key Differences

| Feature                   | Blazored.FluentValidation                         | Blazilla                                           |
| ------------------------- | ------------------------------------------------- | -------------------------------------------------- |
| Component Name            | `<FluentValidationValidator />`                   | `<FluentValidator />`                              |
| Validator Discovery       | Automatic assembly scanning, DI, or explicit pass | Requires DI registration or explicit pass          |
| Async Validation Support  | Limited, requires workarounds                     | Built-in; native `OnValidSubmit` on .NET 11        |
| Nested Object Validation  | Limited (doesn't support all scenarios)           | Fully supported with improved path resolution      |
| Rule Sets                 | Supported via `RuleSet` parameter                 | Supported via `RuleSets` and `AllRules` parameters |
| Custom Validator Selector | Not supported                                     | Supported via `Selector` parameter                 |
| Performance               | Poor, degrades with nested objects                | High, optimized with compiled expression trees     |
| DI Registration Required  | No (automatic assembly scanning fallback)         | Yes (or pass via `Validator` parameter)            |

### Migration Steps

#### 1. Update Package References

Remove the Blazored.FluentValidation package:

```bash
dotnet remove package Blazored.FluentValidation
```

Add Blazilla:

```bash
dotnet add package Blazilla
```

#### 2. Update Using Statements

Replace:

```csharp
using Blazored.FluentValidation;
```

With:

```csharp
using Blazilla;
```

#### 3. Update Component Names

Replace the component name in your Razor files:

**Before (Blazored.FluentValidation):**

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidationValidator />
    <ValidationSummary />
    <!-- form fields -->
</EditForm>
```

**After (Blazilla):**

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    <ValidationSummary />
    <!-- form fields -->
</EditForm>
```

#### 4. Update Rule Set Parameters

If you're using rule sets, update the parameter name:

**Before:**

```razor
<FluentValidationValidator RuleSet="Create,Update" />
```

**After:**

```razor
<FluentValidator RuleSets="@(new[] { "Create", "Update" })" />
```

Or use the newer pattern for executing all rules:

```razor
<FluentValidator AllRules="true" />
```

#### 5. Update Async Validation

Blazilla has improved async validation support. Update your components:

**Before (Blazored.FluentValidation - required manual handling):**

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidationValidator @ref="validator" />
    <!-- form fields -->
    <button type="submit" disabled="@isValidating">Submit</button>
</EditForm>

@code {
    private FluentValidationValidator? validator;
    private bool isValidating;

    private async Task HandleValidSubmit()
    {
        // Manual async validation handling
        if (validator != null)
        {
            isValidating = true;
            await Task.Delay(100); // Wait for async validation
            isValidating = false;
        }
        // Process form
    }
}
```

**After (Blazilla on .NET 11 - native support):**

```razor
<EditForm Model="@model" OnValidSubmit="@ProcessForm">
    <FluentValidator />
    <!-- form fields -->
    <button type="submit">Submit</button>
</EditForm>
```

**After (Blazilla on .NET 8–10 - built-in support):**

```razor
<EditForm Model="@model" OnSubmit="@HandleSubmit">
    <FluentValidator />
    <!-- form fields -->
    <button type="submit" disabled="@isSubmitting">Submit</button>
</EditForm>

@code {
    private bool isSubmitting;

    private async Task HandleSubmit(EditContext editContext)
    {
        isSubmitting = true;
        
        try
        {
            // Built-in async validation handling
            var isValid = await editContext.ValidateAsync();
            
            if (isValid)
            {
                // Process form
                await ProcessForm();
            }
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

#### 6. Register Validators in Dependency Injection

**Important Change**: Blazored.FluentValidation automatically scans assemblies to find validators - no setup required. Blazilla requires validators to be either registered in DI or passed via the `Validator` parameter.

**Before (Blazored.FluentValidation - automatic assembly scanning):**

```csharp
// No registration needed - Blazored.FluentValidation automatically
// scans assemblies and finds validators at runtime
```

**After (Blazilla - explicit registration required):**

```csharp
// Manual registration (simple but tedious for many validators)
builder.Services.AddSingleton<IValidator<Person>, PersonValidator>();
builder.Services.AddSingleton<IValidator<Company>, CompanyValidator>();
builder.Services.AddSingleton<IValidator<Address>, AddressValidator>();
// ... register all validators used in your application
```

**Registration Options:**

To simplify registration of multiple validators, you can use one of these approaches:

**Option 1: FluentValidation's Automatic Registration**

```csharp
// Install: dotnet add package FluentValidation.DependencyInjectionExtensions
// Scans assembly and registers all validators
services.AddValidatorsFromAssemblyContaining<PersonValidator>();
```

**Option 2: Scrutor for Assembly Scanning**

```csharp
// Install: dotnet add package Scrutor
services.Scan(scan => scan
    .FromAssemblyOf<PersonValidator>()
    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
    .AsImplementedInterfaces()
    .WithSingletonLifetime());
```

**Option 3: Injectio for Attribute-Based Registration**

```csharp
// Install: dotnet add package Injectio
// Add attribute to each validator class
[RegisterSingleton<IValidator<Person>>]
public class PersonValidator : AbstractValidator<Person>
{
    // validator implementation
}
```

**Option 4: Pass Validators Directly**

```razor
// No DI registration needed - pass validator instance to component
<FluentValidator Validator="@(new PersonValidator())" />
```

> **Note**: Use singleton lifetime since validators are stateless and thread-safe. Options 1-3 simplify registration when you have many validators.

#### 7. Handle Breaking Changes

**No Automatic Assembly Scanning**: This is the biggest breaking change. Blazored.FluentValidation automatically scans assemblies at runtime to discover and use validators without any configuration. Blazilla does not support automatic assembly scanning - you must explicitly provide validators.

You have two options with Blazilla:

- **Option 1 (Recommended)**: Register each validator in the DI container
- **Option 2**: Pass validators directly via the `Validator` parameter on each component

If you have many validators, you'll need to add DI registrations for each one in your `Program.cs`.

## Troubleshooting

### Common Issues

**No validator found error**

No validator found for model type MyModel. To use FluentValidator, register a validator for this model type or pass one directly to the Validator parameter.

**Solution**: Ensure your validator is registered in the DI container:

```csharp
builder.Services.AddSingleton<IValidator<MyModel>, MyModelValidator>();
```

**EditContext parameter missing**

FluentValidator requires a cascading parameter of type EditContext.

**Solution**: Ensure the component is placed inside an `EditForm`:

```razor
<EditForm Model="@model">
    <FluentValidator />
    <!-- form content -->
</EditForm>
```

**AsyncValidatorInvokedSynchronouslyException on submit (.NET 8–10)**

**Problem**: On .NET 8–10, `OnValidSubmit` runs synchronous validation, which can't run async rules such as `MustAsync`.

**Solution**: Use `OnSubmit` with `EditContextExtensions.ValidateAsync()`, or target .NET 11, where `OnValidSubmit` supports async rules:

```razor
<!-- ❌ Incorrect on .NET 8-10 - OnValidSubmit can't run async rules -->
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    <!-- form fields -->
</EditForm>

<!-- ✅ Correct on all frameworks - Use OnSubmit with ValidateAsync -->
<EditForm Model="@model" OnSubmit="@HandleSubmit">
    <FluentValidator />
    <!-- form fields -->
</EditForm>

@code {
    private async Task HandleSubmit(EditContext editContext)
    {
        // Wait for all async validation to complete
        var isValid = await editContext.ValidateAsync();
        
        if (isValid)
        {
            // Only proceed if validation passed
            await ProcessForm();
        }
    }
}
```

**CS0618 warning: `AsyncMode` or `Validate` is obsolete**

**Solution**: Remove the `AsyncMode` parameter from `FluentValidator`; async rules run automatically. On .NET 11, replace `editContext.Validate(...)` with `await editContext.ValidateAsync(...)`. See the [Migration Guide from 2.x to 3.0](MIGRATION.md).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
