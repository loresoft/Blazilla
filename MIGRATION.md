# Migrating from Blazilla 2.x to 3.0

Blazilla 3.0 adds support for .NET 11 and its native asynchronous form validation. It also simplifies asynchronous validation on every supported framework. This guide covers what changed, what might break, and how to update your app.

> Migrating from **Blazored.FluentValidation**? See [Migration Guide from Blazored.FluentValidation](README.md#migration-guide-from-blazoredfluentvalidation) in the README.

## Contents

- [What's new in 3.0](#whats-new-in-30)
- [Supported frameworks](#supported-frameworks)
- [Upgrade checklist](#upgrade-checklist)
- [Breaking changes](#breaking-changes)
- [Step-by-step migration](#step-by-step-migration)
- [API changes](#api-changes)
- [Behavior changes](#behavior-changes)
- [Troubleshooting](#troubleshooting)

## What's new in 3.0

- **.NET 11 support.** Blazilla adds a `net11.0` target. It registers its validation with the native asynchronous validation support in `EditContext`.
- **`OnValidSubmit` works with async rules on .NET 11.** On .NET 11, `EditForm` awaits `EditContext.ValidateAsync()` before invoking `OnValidSubmit` or `OnInvalidSubmit`, so rules such as `MustAsync` finish before your submit handler runs.
- **No more `AsyncMode`.** Field validation always runs through FluentValidation's asynchronous pipeline, which supports both synchronous and asynchronous rules. Form validation runs asynchronously when requested through `ValidateAsync`. The `AsyncMode` parameter is obsolete and ignored.
- **Cancellation.** `ValidateAsync` accepts a `CancellationToken`. On .NET 11, Blazilla forwards the token to your FluentValidation rules. Editing a field again cancels its earlier field validation, and submitting the form cancels pending field validations.
- **Pending and faulted state on .NET 11.** Use the native `EditContext.IsValidationPending()` and `EditContext.IsValidationFaulted()` APIs to show progress and error states.
- **Safer rule set overrides.** Rule sets passed to `Validate` or `ValidateAsync` are removed when validation completes, even if validation throws or is canceled.

## Supported frameworks

| Target framework | Blazilla 2.x | Blazilla 3.0 | Async rules with `OnValidSubmit` |
| ---------------- | :----------: | :----------: | -------------------------------- |
| .NET 8           |      ✅      |      ✅      | ❌ Use `OnSubmit` + `ValidateAsync` |
| .NET 9           |      ✅      |      ✅      | ❌ Use `OnSubmit` + `ValidateAsync` |
| .NET 10          |      ✅      |      ✅      | ❌ Use `OnSubmit` + `ValidateAsync` |
| .NET 11          |      ❌      |      ✅      | ✅ Native support                  |

You don't have to upgrade to .NET 11 to use Blazilla 3.0.

## Upgrade checklist

1. Update the `Blazilla` package to 3.0.
2. Remove `AsyncMode` from every `<FluentValidator />`.
3. Choose a submit pattern for forms with async rules:
   - **.NET 11:** use `OnValidSubmit`/`OnInvalidSubmit` and remove manual `ValidateAsync` calls from submit handlers.
   - **.NET 8–10, or libraries that multi-target:** keep `OnSubmit` and `await editContext.ValidateAsync()`.
4. On .NET 11, replace calls to `editContext.Validate(...)` with `await editContext.ValidateAsync(...)`.
5. Pass the `CancellationToken` through your `MustAsync` rules and the I/O they perform.
6. Remove any code that reads or writes `FluentValidator.PendingTask`.
7. Rebuild every project that calls Blazilla APIs. Don't rely on binaries compiled against 2.x.
8. Test form submission, field validation, rule sets, and nested object validation for each target framework your app uses.

## Breaking changes

| Change | Who is affected | What to do |
| ------ | --------------- | ---------- |
| `AsyncMode` is obsolete and ignored. | Markup or code that sets `AsyncMode`. Builds that treat warnings as errors fail with `CS0618`. | Remove the parameter. Async rules run automatically. |
| On .NET 8–10, form validation triggered by `EditForm.OnValidSubmit` or `EditContext.Validate()` always runs synchronously. A validator with async rules throws `AsyncValidatorInvokedSynchronouslyException`. | Forms on .NET 8–10 that combine `AsyncMode="true"` and async rules with `OnValidSubmit`. In 2.x, these forms submitted without waiting for async rules. | Use `OnSubmit` and `await editContext.ValidateAsync()`, or target .NET 11 and use `OnValidSubmit`. |
| On .NET 11, `EditContextExtensions.Validate(ruleSets)` is obsolete, matching the native `EditContext.Validate()`. | Code compiled for .NET 11 that calls `Validate` or `Validate(ruleSets)`. | Call `await editContext.ValidateAsync(...)` instead. Synchronous validation still works only when validators have no async rules. |
| The parameterless `EditContextExtensions.ValidateAsync(EditContext)` method was replaced by `ValidateAsync(EditContext, IEnumerable<string>? ruleSets = null, CancellationToken cancellationToken = default)`. | Assemblies compiled against Blazilla 2.x that are not recompiled. These fail at run time with `MissingMethodException`. | Recompile. `editContext.ValidateAsync()` remains source compatible. |
| On .NET 11, `editContext.ValidateAsync()` and `editContext.ValidateAsync(cancellationToken)` call the native `EditContext.ValidateAsync` instance method instead of the Blazilla extension. | Code that depended on Blazilla-specific behavior of the extension, such as its null `EditContext` check or the `PendingTask` protocol. | Use native .NET 11 semantics. Faulted validators return `false` and set `IsValidationFaulted()` instead of throwing. Canceling the token throws `OperationCanceledException`. |
| `FluentValidator.PendingTask` isn't used on .NET 11. On .NET 8–10, it's set only for validation requested through `ValidateAsync`. | Custom code or tests that inject, inspect, or await the `PendingTask` property. | Await `ValidateAsync` instead. The constant remains for compatibility only. |
| Field-level validation always runs asynchronously. The field's messages are cleared when validation starts and replaced when it completes. | UI or tests that expect field messages to update synchronously after `NotifyFieldChanged`. | Await validation, or wait for `OnValidationStateChanged`, before you check messages. |

The following behavior is **unchanged**:

- .NET 8, 9, and 10 are still supported.
- Validator configuration (`Validator`, `RuleSets`, `AllRules`, `Selector`), DI-based validator resolution, nested object validation, and path mapping work as before.
- On .NET 8–10, `OnSubmit` with `await editContext.ValidateAsync()` still works.
- Forms with only synchronous rules keep working with `OnValidSubmit` on every framework.

## Step-by-step migration

### 1. Update the package

```bash
dotnet add package Blazilla --version 3.0.0
```

### 2. Remove `AsyncMode`

**Before (2.x):**

```razor
<FluentValidator AsyncMode="true" AllRules="true" />
```

**After (3.0):**

```razor
<FluentValidator AllRules="true" />
```

Both `AsyncMode="true"` and `AsyncMode="false"` are ignored in 3.0. The parameter will be removed in a future major version.

### 3. Update form submission

#### .NET 11: use `OnValidSubmit`

On .NET 11, `EditForm` waits for all validation, including async rules, before invoking `OnValidSubmit` or `OnInvalidSubmit`. You no longer need a manual submit handler.

**Before (2.x):**

```razor
<EditForm Model="@person" OnSubmit="@HandleSubmit">
    <FluentValidator AsyncMode="true" />
    <ValidationSummary />
    @* inputs *@
    <button type="submit">Save</button>
</EditForm>

@code {
    private async Task HandleSubmit(EditContext editContext)
    {
        if (await editContext.ValidateAsync())
            await SaveAsync();
    }
}
```

**After (3.0 on .NET 11):**

```razor
<EditForm Model="@person" OnValidSubmit="@SaveAsync">
    <FluentValidator />
    <ValidationSummary />
    @* inputs *@
    <button type="submit">Save</button>
</EditForm>
```

> **Note:** Don't call `ValidateAsync` inside `OnValidSubmit` or `OnInvalidSubmit`. `EditForm` has already validated the form, so calling it again runs every rule twice.

#### .NET 8–10, or code that multi-targets: keep `OnSubmit` + `ValidateAsync`

On .NET 8–10, Blazor's built-in `EditForm` validates synchronously before invoking `OnValidSubmit`. To wait for async rules, handle `OnSubmit` and await `ValidateAsync`. This pattern also works on .NET 11, so use it for code that multi-targets.

**After (3.0 on any framework):**

```razor
<EditForm Model="@person" OnSubmit="@HandleSubmit">
    <FluentValidator />
    <ValidationSummary />
    @* inputs *@
    <button type="submit">Save</button>
</EditForm>

@code {
    private async Task HandleSubmit(EditContext editContext)
    {
        if (await editContext.ValidateAsync())
            await SaveAsync();
    }
}
```

The only change from 2.x is removing `AsyncMode`.

### 4. Replace synchronous validation calls (.NET 11)

On .NET 11, `EditContextExtensions.Validate(ruleSets)` is obsolete because it can't wait for async rules.

**Before:**

```csharp
var isValid = editContext.Validate("Create");
```

**After:**

```csharp
var isValid = await editContext.ValidateAsync("Create");

// or, with cancellation
var isValid = await editContext.ValidateAsync(["Create"], cancellationToken);
```

### 5. Forward cancellation tokens

On .NET 11, Blazilla cancels superseded field validation and passes cancellation to your rules. Use the token that FluentValidation passes to `MustAsync`:

**Before:**

```csharp
RuleFor(p => p.EmailAddress)
    .MustAsync(async (email, _) => await IsUniqueAsync(email));
```

**After:**

```csharp
RuleFor(p => p.EmailAddress)
    .MustAsync(async (email, cancellationToken) => await IsUniqueAsync(email, cancellationToken));
```

On .NET 8–10, `ValidateAsync(cancellationToken)` accepts a token for source compatibility, but Blazilla doesn't observe it there.

### 6. Stop using `PendingTask`

If you read `FluentValidator.PendingTask` from `EditContext.Properties`, await the validation API instead:

**Before:**

```csharp
editContext.Validate();
if (editContext.Properties.TryGetValue(FluentValidator.PendingTask, out var value) && value is Task task)
    await task;
```

**After:**

```csharp
var isValid = await editContext.ValidateAsync();
```

### 7. Optional: show pending and faulted state (.NET 11)

.NET 11 tracks async validation on the `EditContext`. Use it to show progress and failures:

```razor
<EditForm EditContext="@editContext" OnValidSubmit="@SaveAsync">
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

    <button type="submit" disabled="@editContext.IsValidationPending()">Save</button>
</EditForm>

@code {
    private readonly Person person = new();
    private EditContext editContext = default!;

    protected override void OnInitialized()
    {
        editContext = new EditContext(person);

        // Re-render when async validation starts and completes
        editContext.OnValidationStateChanged += (_, _) => InvokeAsync(StateHasChanged);
    }
}
```

`EditContext` raises `OnValidationStateChanged` when async validation starts and when it completes. Subscribe to it so the component re-renders and shows the current pending and faulted state.

## API changes

### `FluentValidator`

| Member | 2.x | 3.0 |
| ------ | --- | --- |
| `AsyncMode` parameter | Chose synchronous or asynchronous validation. | `[Obsolete]` and ignored on all frameworks. |
| `PendingTask` constant | Key for the pending form validation task. | Kept for compatibility. Used only on .NET 8–10 for validation requested through `ValidateAsync`. |
| `RuleSetProperty` constant | Key for per-call rule set overrides. | Unchanged. |
| `AsyncRequestProperty` constant | — | **New.** Internal flag used on .NET 8–10 to mark a validation request as asynchronous. Don't set it yourself. |

### `EditContextExtensions`

| 2.x signature | 3.0 signature | Notes |
| ------------- | ------------- | ----- |
| `bool Validate(this EditContext, params IEnumerable<string> ruleSets)` | Unchanged | `[Obsolete]` on .NET 11. Throws if a validator has async rules. |
| `Task<bool> ValidateAsync(this EditContext)` | Removed | Replaced by the optional-parameter overload below. `editContext.ValidateAsync()` still compiles; on .NET 11, it calls the native method. |
| `Task<bool> ValidateAsync(this EditContext, params IEnumerable<string> ruleSets)` | Unchanged | Convenience overload for inline rule sets. |
| — | `Task<bool> ValidateAsync(this EditContext, IEnumerable<string>? ruleSets = null, CancellationToken cancellationToken = default)` | **New.** Rule sets plus cancellation. |
| — | `Task<bool> ValidateAsync(this EditContext, CancellationToken cancellationToken)` | **New, .NET 8–10 only.** Matches the native .NET 11 `EditContext.ValidateAsync(CancellationToken)` call shape. |

## Behavior changes

### How validation runs

| Scenario | .NET 8–10 | .NET 11 |
| -------- | --------- | ------- |
| Field changed | Runs the validator asynchronously for the changed field. | Registers the field validation with `EditContext.RegisterAsyncFieldValidator`. Editing the same field again cancels the earlier validation. |
| `EditForm` with `OnValidSubmit`/`OnInvalidSubmit` | Synchronous form validation. Async rules throw. | Awaits `EditContext.ValidateAsync()`, including async rules, before invoking the callback. |
| `await editContext.ValidateAsync()` | Runs form validation asynchronously and waits for it to complete. | Native `EditContext.ValidateAsync()`. Cancels pending field validations, then awaits form validation. |
| `editContext.Validate()` | Synchronous form validation. Async rules throw. | Obsolete. Synchronous form validation. Async rules throw. |

### Errors and cancellation

- **.NET 11:** When an async rule throws, `ValidateAsync` returns `false` and `EditContext.IsValidationFaulted()` returns `true`. The exception isn't rethrown. A canceled `cancellationToken` throws `OperationCanceledException` after registered validators settle. When a rule observes cancellation and stops, Blazilla doesn't update validation messages for that validation.
- **.NET 8–10:** Exceptions from async rules propagate from `ValidateAsync`. The cancellation token isn't observed.

### Field messages while validating

When a field changes, Blazilla clears that field's messages before validation starts. It adds the new messages when validation completes. For fields with slow async rules, the field briefly shows no message. On .NET 11, use `IsValidationPending(...)` to show a progress indicator.

## Troubleshooting

### `CS0618: 'FluentValidator.AsyncMode' is obsolete`

Remove `AsyncMode` from the component. Async rules run automatically.

### `CS0618: 'EditContextExtensions.Validate(EditContext, params IEnumerable<string>)' is obsolete` (.NET 11)

Replace `editContext.Validate(...)` with `await editContext.ValidateAsync(...)`.

### `AsyncValidatorInvokedSynchronouslyException` on submit

A validator with async rules (for example, `MustAsync`) was run synchronously. This usually happens when a form on .NET 8–10 uses `OnValidSubmit`, or when code calls `editContext.Validate()`.

- On .NET 8–10, use `OnSubmit` and `await editContext.ValidateAsync()`.
- On .NET 11, use `OnValidSubmit` or `await editContext.ValidateAsync()` instead of `Validate()`.

### `MissingMethodException: ... EditContextExtensions.ValidateAsync(EditContext)`

An assembly compiled against Blazilla 2.x is calling a method that was removed in 3.0. Recompile that assembly against Blazilla 3.0.

### The form submits on .NET 11, but I validate twice

Remove manual `ValidateAsync` calls from `OnValidSubmit`/`OnInvalidSubmit` handlers. On .NET 11, `EditForm` has already validated the form when these callbacks run.

### `ValidateAsync` returns `false`, but there are no validation messages (.NET 11)

A validator threw an exception, so the form is faulted. Check `editContext.IsValidationFaulted()`, show an error, and let the user try again.
