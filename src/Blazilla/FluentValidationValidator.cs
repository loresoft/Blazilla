using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection.Metadata;

using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla;

/// <summary>
/// A component that integrates FluentValidation with form <see cref="Microsoft.AspNetCore.Components.Forms.EditContext"/>
/// validation system. This component provides real-time validation feedback and integrates with EditContext
/// to display validation messages in forms.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="FluentValidationValidator"/> component must be placed inside an EditForm
/// and requires a cascading EditContext parameter. It can automatically resolve validators
/// from the dependency injection container or accept a validator directly via the
/// <see cref="Validator"/> parameter.
/// </para>
/// <para>
/// The component supports various validation modes including field-level validation on change,
/// form-level validation on submit, rule set filtering, and both synchronous and asynchronous
/// validation modes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;EditForm Model="@model"&gt;
///     &lt;FluentValidationValidator /&gt;
///     &lt;ValidationSummary /&gt;
///
///     &lt;InputText @bind-Value="model.Name" /&gt;
///     &lt;ValidationMessage For="@(() =&gt; model.Name)" /&gt;
/// &lt;/EditForm&gt;
/// </code>
/// </example>
/// <seealso cref="Microsoft.AspNetCore.Components.Forms.EditContext"/>
/// <seealso cref="IValidator"/>
public class FluentValidationValidator : ComponentBase, IDisposable
{
    /// <summary>
    /// The key used to store the pending validation task in the EditContext properties.
    /// </summary>
    public const string PendingTask = "__FluentValidation_Task";

    private static readonly ConcurrentDictionary<Type, Func<object, PropertyChain?, IValidatorSelector, IValidationContext>> _contextFactoryCache = new();
    private static readonly ConcurrentDictionary<Type, Type> _validatorTypeCache = new();

    private readonly PathResolver _pathResolver = new();

    private EditContext? _currentContext;
    private ValidationMessageStore? _messages;
    private IValidator? _currentValidator;
    private Type _modelType = null!;

    /// <summary>
    /// Gets or sets the service provider used to resolve validators from the dependency injection container.
    /// This property is automatically injected by the Blazor framework.
    /// </summary>
    [Inject]
    protected IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the EditContext that provides the validation context for the form.
    /// This parameter is automatically provided by the parent EditForm component.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown during initialization if no EditContext is available.
    /// </exception>
    [CascadingParameter]
    protected EditContext? EditContext { get; set; }

    /// <summary>
    /// Gets or sets the FluentValidation validator to use for validation.
    /// If not specified, the component will attempt to resolve a validator
    /// from the service provider based on the model type.
    /// </summary>
    /// <remarks>
    /// When provided, this validator takes precedence over any validator
    /// that might be registered in the dependency injection container.
    /// </remarks>
    [Parameter]
    public IValidator? Validator { get; set; }

    /// <summary>
    /// Gets or sets the collection of rule sets to execute during validation.
    /// Only validation rules that belong to the specified rule sets will be executed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rule sets allow you to group validation rules and execute only specific
    /// groups of rules in different scenarios. This is useful when you have
    /// different validation requirements for create vs. update operations.
    /// </para>
    /// <para>
    /// If both <see cref="RuleSets"/> and <see cref="AllRules"/> are specified,
    /// <see cref="AllRules"/> takes precedence.
    /// </para>
    /// </remarks>
    [Parameter]
    public IEnumerable<string>? RuleSets { get; set; }

    /// <summary>
    /// Gets or sets a custom validator selector that determines which validation rules to execute.
    /// This provides fine-grained control over rule selection beyond simple rule sets.
    /// </summary>
    /// <remarks>
    /// When specified, this selector has the highest priority and will be combined
    /// with other selectors (field-specific and rule set selectors) using a composite selector.
    /// </remarks>
    [Parameter]
    public IValidatorSelector? Selector { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to execute all validation rules,
    /// including those in rule sets, during validation. Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, this parameter overrides any <see cref="RuleSets"/>
    /// specification and causes all rules to be executed regardless of their rule set membership.
    /// </remarks>
    [Parameter]
    public bool AllRules { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use asynchronous validation mode. Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>true</c>, asynchronous validators will be executed directly using ValidateAsync()
    /// for optimal performance. When <c>false</c>, synchronous validation
    /// will be performed using Validate().
    /// </para>
    /// <para>
    /// Async mode should be enabled when your validators contain async validation rules (MustAsync, etc.)
    /// to ensure proper asynchronous execution.
    /// </para>
    /// </remarks>
    [Parameter]
    public bool AsyncMode { get; set; }

    /// <summary>
    /// Initializes the component by setting up the validation context and subscribing to validation events.
    /// This method is called once when the component is first rendered.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no EditContext is available or when no suitable validator can be found for the model type.
    /// </exception>
    protected override void OnInitialized()
    {
        if (EditContext == null)
        {
            throw new InvalidOperationException(
                $"{nameof(FluentValidationValidator)} requires a cascading parameter of type {nameof(EditContext)}.");
        }

        _currentContext = EditContext;
        _messages = new ValidationMessageStore(_currentContext);

        _modelType = _currentContext.Model.GetType();

        _currentContext.OnFieldChanged += OnFieldChanged;
        _currentContext.OnValidationRequested += OnValidationRequested;

        _currentValidator = Validator;
        if (_currentValidator == null)
        {
            var validatorType = _validatorTypeCache.GetOrAdd(_modelType, t => typeof(IValidator<>).MakeGenericType(t));
            _currentValidator = ServiceProvider.GetService(validatorType) as IValidator;
        }

        if (_currentValidator == null)
        {
            throw new InvalidOperationException(
                $"No validator found for model type {_modelType.FullName}. " +
                $"To use {nameof(FluentValidationValidator)}, register a validator for this model type " +
                $"or pass one directly to the {nameof(Validator)} parameter.");
        }
    }

    /// <summary>
    /// Validates that the EditContext parameter has not changed between renders.
    /// This component does not support dynamic EditContext changes.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the EditContext parameter changes after the component has been initialized.
    /// </exception>
    protected override void OnParametersSet()
    {
        // EditContext change is not supported
        if (EditContext == _currentContext)
            return;

        throw new InvalidOperationException(
            $"{GetType()} does not support changing the {nameof(EditContext)} dynamically.");
    }

    /// <summary>
    /// Handles field changed events from the EditContext and performs validation for the changed field.
    /// This method is called whenever a form field value changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The event arguments containing the field identifier.</param>
    private async void OnFieldChanged(object? sender, FieldChangedEventArgs eventArgs)
    {
        if (_currentContext == null || _currentValidator == null)
            return;

        var fieldIdentifier = eventArgs.FieldIdentifier;

        // if the field belongs to a different model, we need to find the correct path
        var fieldName = ReferenceEquals(fieldIdentifier.Model, _currentContext.Model)
            ? fieldIdentifier.FieldName
            : _pathResolver.FindPath(_currentContext.Model, fieldIdentifier);

        // fallback to original field name if path resolution fails
        fieldName ??= fieldIdentifier.FieldName;

        // build context for the specific field
        var context = BuildContext(fieldName);
        if (context == null)
            return;

        var validationResults = AsyncMode
            ? await _currentValidator.ValidateAsync(context).ConfigureAwait(false)
            : _currentValidator.Validate(context);

        // update messages for the specific field
        ApplyValidationResults(validationResults, fieldIdentifier);

        // notify on UI thread
        _ = InvokeAsync(_currentContext.NotifyValidationStateChanged);
    }

    /// <summary>
    /// Handles validation requested events from the EditContext and performs full model validation.
    /// This method is called when the entire form is validated, typically on form submission.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The event arguments for the validation request.</param>
    private async void OnValidationRequested(object? sender, ValidationRequestedEventArgs eventArgs)
    {
        if (_currentContext == null || _currentValidator == null)
            return;

        // build context for the entire model
        var context = BuildContext();
        if (context == null)
            return;

        ValidationResult? validationResults;

        if (AsyncMode)
        {
            // store pending task in EditContext properties so it can be awaited if needed
            // used in EditContextExtensions.ValidateAsync() to prevent premature form submission
            var task = _currentValidator.ValidateAsync(context);
            _currentContext.Properties[PendingTask] = task;

            // continue validation so message store is updated when task completes
            validationResults = await task.ConfigureAwait(false);
        }
        else
        {
            validationResults = _currentValidator.Validate(context);
        }

        // update messages for all fields
        ApplyValidationResults(validationResults);

        // notify on UI thread
        _ = InvokeAsync(_currentContext.NotifyValidationStateChanged);
    }

    /// <summary>
    /// Builds a validation context for the specified field or the entire model.
    /// </summary>
    /// <param name="fieldName">The name of the specific field to validate, or null to validate the entire model.</param>
    /// <returns>A validation context configured with the appropriate validator selector, or null if the context cannot be created.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no current EditContext is available.</exception>
    private IValidationContext? BuildContext(string? fieldName = null)
    {
        if (_currentContext == null)
            return null;

        var selector = CreateSelector(fieldName);

        // Use cached factory for creating validation context instances
        var factory = _contextFactoryCache.GetOrAdd(_modelType, CreateContextFactory);

        return factory(_currentContext.Model, null, selector);
    }

    /// <summary>
    /// Creates a validator selector based on the component's configuration and the specified field name.
    /// The selector determines which validation rules will be executed.
    /// </summary>
    /// <param name="fieldName">The name of the specific field to create a selector for, or null for full model validation.</param>
    /// <returns>A validator selector that determines which validation rules to execute.</returns>
    /// <remarks>
    /// The selector prioritizes sources in the following order:
    /// 1. Custom <see cref="Selector"/> (highest priority)
    /// 2. Field-specific selector for the given <paramref name="fieldName"/>
    /// 3. Rule set selector based on <see cref="AllRules"/> or <see cref="RuleSets"/> (lowest priority)
    /// </remarks>
    private IValidatorSelector CreateSelector(string? fieldName)
    {
        // if nothing is specified, use the default selector
        if (Selector == null && fieldName == null && !AllRules && RuleSets?.Any() != true)
            return ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory();

        // use field selector only if fieldName is provided
        if (fieldName != null)
            return ValidatorOptions.Global.ValidatorSelectors.MemberNameValidatorSelectorFactory([fieldName]);

        var selectors = new List<IValidatorSelector>();

        // custom selector always has highest priority
        if (Selector != null)
            selectors.Add(Selector);

        // rule-set selector has lowest priority
        if (AllRules)
        {
            var allSelector = ValidatorOptions.Global.ValidatorSelectors.RulesetValidatorSelectorFactory([RulesetValidatorSelector.WildcardRuleSetName]);
            selectors.Add(allSelector);
        }
        else if (RuleSets?.Any() == true)
        {
            var rulesetSelector = ValidatorOptions.Global.ValidatorSelectors.RulesetValidatorSelectorFactory(RuleSets);
            selectors.Add(rulesetSelector);
        }

        // if no selectors, use default
        if (selectors.Count == 0)
            return ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory();

        // if only one selector, use it directly
        if (selectors.Count == 1)
            return selectors[0];

        // combine all selectors into one
        return ValidatorOptions.Global.ValidatorSelectors.CompositeValidatorSelectorFactory(selectors);
    }

    /// <summary>
    /// Applies validation results to the form's validation message store, clearing previous messages
    /// and adding new validation error messages for each field.
    /// </summary>
    /// <param name="validationResults">The validation results containing validation errors to display.</param>
    /// <param name="fieldIdentifier">The field identifier for the specific field being validated, or null for the entire model.</param>
    private void ApplyValidationResults(ValidationResult? validationResults, FieldIdentifier? fieldIdentifier = null)
    {
        validationResults ??= new ValidationResult();

        // clear previous messages for the field or all fields
        if (fieldIdentifier != null)
            _messages?.Clear(fieldIdentifier.Value);
        else
            _messages?.Clear();

        // if valid, nothing more to do
        if (validationResults.IsValid)
            return;

        var model = _currentContext?.Model;
        if (model == null)
            return;

        foreach (var validationFailure in validationResults.Errors)
        {
            // try to find the field by path, if that fails, create a new identifier
            var field = PathResolver.FindField(model, validationFailure.PropertyName)
                ?? new FieldIdentifier(model, validationFailure.PropertyName);

            // when validating a specific field, skip messages for other fields
            if (fieldIdentifier != null && !field.Equals(fieldIdentifier.Value))
                continue;

            // message store handles multiple messages per field
            _messages?.Add(field, validationFailure.ErrorMessage);
        }
    }

    /// <summary>
    /// Performs cleanup when the component is being disposed.
    /// This method can be overridden by derived classes to provide additional cleanup logic.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> if the method is being called from the <see cref="IDisposable.Dispose"/> method;
    /// <c>false</c> if it's being called from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// Releases all resources used by the <see cref="FluentValidationValidator"/> component.
    /// This includes clearing validation messages and unsubscribing from EditContext events.
    /// </summary>
    void IDisposable.Dispose()
    {
        _messages?.Clear();

        if (_currentContext != null)
        {
            _currentContext.OnFieldChanged -= OnFieldChanged;
            _currentContext.OnValidationRequested -= OnValidationRequested;
            _currentContext.NotifyValidationStateChanged();
        }

        Dispose(disposing: true);

        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Creates a compiled delegate factory for creating ValidationContext instances of the specified type.
    /// This avoids the performance overhead of using Activator.CreateInstance with reflection.
    /// </summary>
    /// <param name="modelType">The model type to create a factory for.</param>
    /// <returns>A compiled delegate that creates ValidationContext instances.</returns>
    private static Func<object, PropertyChain?, IValidatorSelector, IValidationContext> CreateContextFactory(Type modelType)
    {
        var contextType = typeof(ValidationContext<>).MakeGenericType(modelType);

        // Get the constructor that takes (T instance, PropertyChain propertyChain, IValidatorSelector validatorSelector)
        var constructor = contextType.GetConstructor([modelType, typeof(PropertyChain), typeof(IValidatorSelector)])
            ?? throw new InvalidOperationException($"Could not find appropriate constructor for {contextType}");

        // Create expression parameters
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var propertyChainParam = Expression.Parameter(typeof(PropertyChain), "propertyChain");
        var selectorParam = Expression.Parameter(typeof(IValidatorSelector), "selector");

        // Convert the instance parameter to the correct model type
        var typedInstance = Expression.Convert(instanceParam, modelType);

        // Create the constructor call expression
        var constructorCall = Expression.New(constructor, typedInstance, propertyChainParam, selectorParam);

        // Convert the result to IValidationContext
        var convertedResult = Expression.Convert(constructorCall, typeof(IValidationContext));

        // Compile the expression into a delegate
        var lambda = Expression.Lambda<Func<object, PropertyChain?, IValidatorSelector, IValidationContext>>(
            convertedResult,
            instanceParam,
            propertyChainParam,
            selectorParam);

        return lambda.Compile();
    }
}
