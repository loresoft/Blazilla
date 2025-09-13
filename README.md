# Blazor FluentValidation

A library for using FluentValidation with Blazor

[![Nuget version](https://img.shields.io/nuget/v/LoreSoft.Blazor.FluentValidation.svg?logo=nuget)](https://www.nuget.org/packages/LoreSoft.Blazor.FluentValidation/)
[![Build Status](https://github.com/loresoft/LoreSoft.Blazor.FluentValidation/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/LoreSoft.Blazor.FluentValidation/actions)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/LoreSoft.Blazor.FluentValidation/badge.svg?branch=main)](https://coveralls.io/github/loresoft/LoreSoft.Blazor.FluentValidation?branch=main)
[![License](https://img.shields.io/github/license/loresoft/LoreSoft.Blazor.FluentValidation.svg)](LICENSE)

## Overview

LoreSoft.Blazor.FluentValidation provides seamless integration between [FluentValidation](https://fluentvalidation.net/) and Blazor's `EditForm` component. This library enables you to use FluentValidation's powerful and flexible validation rules with Blazor forms, supporting both Blazor Server and Blazor WebAssembly applications.

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
dotnet add package LoreSoft.Blazor.FluentValidation
```

Or via Package Manager Console:

```bash
Install-Package LoreSoft.Blazor.FluentValidation
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

Enable asynchronous validation mode for validators that contain async rules:

```razor
<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <FluentValidationValidator AsyncMode="true" />
    <!-- form fields -->
</EditForm>
```

```csharp
public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(p => p.EmailAddress)
            .NotEmpty().WithMessage("Email is required")
            .MustAsync(async (email, cancellation) => 
            {
                // Simulate async validation (e.g., database check)
                await Task.Delay(100, cancellation);
                return !email?.Equals("admin@example.com", StringComparison.OrdinalIgnoreCase) ?? true;
            }).WithMessage("This email address is not available");
    }
}
```

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
- Asynchronous validation uses `Task.Run()` to prevent UI thread blocking

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

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
