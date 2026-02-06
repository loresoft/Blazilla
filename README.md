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
- **Asynchronous validation** - Built-in support for async validation rules
- **Rule sets** - Execute specific groups of validation rules
- **Custom validator selectors** - Fine-grained control over which rules to execute
- **Dependency injection integration** - Automatic validator resolution from DI container
- **Performance optimized** - Compiled expression trees for fast validation context creation

## Installation

Install the package via NuGet:

```bash
dotnet add package Blazilla
```

Or via Package Manager Console:

```bash
Install-Package Blazilla
```

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

## Advanced Usage

### Asynchronous Validation

Blazor's built-in validation system doesn't natively support asynchronous validation. When using async validation rules with FluentValidation, you need to handle form submission manually to ensure async validation completes before the form is submitted.

Enable asynchronous validation mode and use `OnSubmit` instead of `OnValidSubmit` to properly handle async validation:

```razor
<EditForm Model="@person" OnSubmit="@HandleSubmit">
    <FluentValidator AsyncMode="true" />
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

Create a validator with async rules:

```csharp
public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(p => p.EmailAddress)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MustAsync(async (email, cancellation) => 
            {
                // Simulate async validation (e.g., database check)
                await Task.Delay(500, cancellation);
                return !email?.Equals("admin@example.com", StringComparison.OrdinalIgnoreCase) ?? true;
            }).WithMessage("This email address is not available");
    }
}
```

#### Why This Approach is Necessary

Blazor's `OnValidSubmit` event fires immediately after synchronous validation passes, without waiting for async validation to complete. This can result in forms being submitted with incomplete validation results. 

The `EditContextExtensions.ValidateAsync()` method:

1. Triggers synchronous validation first (which may initiate async validation tasks)
2. Waits for any pending async validation tasks to complete
3. Returns `true` only when all validation (sync and async) has passed

This ensures that form submission is properly prevented when async validation rules fail.

> **Important**: Always use `OnSubmit` instead of `OnValidSubmit` when working with async validation rules. The `OnValidSubmit` event doesn't wait for async validation to complete.

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
| `AsyncMode` | `bool`                 | `false` | Enables asynchronous validation mode for validators containing async rules (MustAsync, etc.). When enabled, validation will be performed asynchronously and may show a brief delay for async operations like database checks.       |
| `Selector`  | `IValidatorSelector?`  | `null`  | Custom validator selector implementation for advanced scenarios requiring fine-grained control over which validation rules execute. Allows conditional rule execution based on custom logic, property paths, or validation context. |

## Validation Behavior

- **Field-level validation**: Triggered when individual form fields change
- **Form-level validation**: Triggered when the form is submitted via `OnValidSubmit`
- **Real-time feedback**: Validation messages appear immediately as users interact with the form
- **Integration with EditContext**: Works seamlessly with Blazor's built-in validation system

## Performance Considerations

- Validators should be registered as singletons in the DI container since they are stateless and can be safely shared across requests/components
- Validators are cached and reused when resolved from dependency injection
- Validation contexts are created using compiled expression trees for optimal performance
- Asynchronous validation executes async validators directly, ensuring responsive UI performance

> **AsyncMode**: Only enable `AsyncMode="true"` when your validators contain actual async rules (like `MustAsync`). Using async mode with purely synchronous validators introduces unnecessary overhead from async state machine generation and task scheduling, even though the validation logic itself is synchronous

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

**Important**: Blazor's built-in form validation system has inherent limitations with asynchronous validation:

1. **OnValidSubmit Event Timing**: The `OnValidSubmit` event fires immediately after synchronous validation passes, without waiting for any asynchronous validation operations to complete.

2. **EditContext.Validate() Behavior**: The standard `EditContext.Validate()` method only performs synchronous validation and doesn't trigger or wait for async validation rules.

**Workaround for Async Validation**:

When using validators with async rules (`MustAsync`, custom async validators), you must:

- Enable `AsyncMode="true"` on the `FluentValidator` component
- Use `OnSubmit` instead of `OnValidSubmit`
- Call `EditContextExtensions.ValidateAsync()` to ensure async validation completes

This limitation is a fundamental aspect of Blazor's validation architecture and affects all validation libraries, not just FluentValidation integrations.

## Migration Guide from Blazored.FluentValidation

If you're currently using [Blazored.FluentValidation](https://github.com/Blazored/FluentValidation), migrating to Blazilla is straightforward. The libraries share similar APIs and functionality, with some key improvements in Blazilla.

### Key Differences

| Feature                   | Blazored.FluentValidation                         | Blazilla                                           |
| ------------------------- | ------------------------------------------------- | -------------------------------------------------- |
| Component Name            | `<FluentValidationValidator />`                   | `<FluentValidator />`                              |
| Validator Discovery       | Automatic assembly scanning, DI, or explicit pass | Requires DI registration or explicit pass          |
| Async Validation Support  | Limited, requires workarounds                     | Built-in with `AsyncMode` parameter                |
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

**After (Blazilla - built-in support):**

```razor
<EditForm Model="@model" OnSubmit="@HandleSubmit">
    <FluentValidator AsyncMode="true" />
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

**Invalid form submits when using AsyncMode**

When using `AsyncMode="true"`, forms may submit even when async validation fails if you're using `OnValidSubmit` instead of `OnSubmit`.

**Problem**: `OnValidSubmit` fires immediately after synchronous validation passes, without waiting for async validation to complete.

**Solution**: Use `OnSubmit` with `EditContextExtensions.ValidateAsync()`:

```razor
<!-- ❌ Incorrect - Don't use OnValidSubmit with async validation -->
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator AsyncMode="true" />
    <!-- form fields -->
</EditForm>

<!-- ✅ Correct - Use OnSubmit with ValidateAsync -->
<EditForm Model="@model" OnSubmit="@HandleSubmit">
    <FluentValidator AsyncMode="true" />
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

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
