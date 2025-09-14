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
    <FluentValidationValidator />
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
    <FluentValidationValidator AsyncMode="true" />
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
<FluentValidationValidator RuleSets="@(new[] { "Create" })" />

<!-- Execute all rules including those in rule sets -->
<FluentValidationValidator AllRules="true" />
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
    <FluentValidationValidator />
    
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
    <FluentValidationValidator Validator="@validator" />
    <!-- form fields -->
</EditForm>
```

### Custom Validator Selector

Implement fine-grained control over which validation rules to execute:

```razor
<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidationValidator Selector="@customSelector" />
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

The `FluentValidationValidator` component supports the following parameters:

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

### Blazor Forms and Async Validation Limitations

**Important**: Blazor's built-in form validation system has inherent limitations with asynchronous validation:

1. **OnValidSubmit Event Timing**: The `OnValidSubmit` event fires immediately after synchronous validation passes, without waiting for any asynchronous validation operations to complete.

2. **EditContext.Validate() Behavior**: The standard `EditContext.Validate()` method only performs synchronous validation and doesn't trigger or wait for async validation rules.

**Workaround for Async Validation**:

When using validators with async rules (`MustAsync`, custom async validators), you must:

- Enable `AsyncMode="true"` on the `FluentValidationValidator` component
- Use `OnSubmit` instead of `OnValidSubmit`
- Call `EditContextExtensions.ValidateAsync()` to ensure async validation completes

This limitation is a fundamental aspect of Blazor's validation architecture and affects all validation libraries, not just FluentValidation integrations.

## Troubleshooting

### Common Issues

**No validator found error**

No validator found for model type MyModel. To use FluentValidationValidator, register a validator for this model type or pass one directly to the Validator parameter.

**Solution**: Ensure your validator is registered in the DI container:

```csharp
builder.Services.AddSingleton<IValidator<MyModel>, MyModelValidator>();
```

**EditContext parameter missing**

FluentValidationValidator requires a cascading parameter of type EditContext.

**Solution**: Ensure the component is placed inside an `EditForm`:

```razor
<EditForm Model="@model">
    <FluentValidationValidator />
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
    <FluentValidationValidator AsyncMode="true" />
    <!-- form fields -->
</EditForm>

<!-- ✅ Correct - Use OnSubmit with ValidateAsync -->
<EditForm Model="@model" OnSubmit="@HandleSubmit">
    <FluentValidationValidator AsyncMode="true" />
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
