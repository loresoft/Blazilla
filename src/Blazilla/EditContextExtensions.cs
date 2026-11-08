using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla;

/// <summary>
/// Provides extension methods for the <see cref="EditContext"/> class to facilitate validation operations.
/// </summary>
public static class EditContextExtensions
{
    /// <summary>
    /// Validates the <see cref="EditContext"/> synchronously.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to validate.</param>
    /// <param name="ruleSets">Optional validation rule sets to apply. When provided, overrides any rule sets configured on the validator component.</param>
    /// <returns>
    /// <see langword="true"/> if validation succeeded (no validation messages); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method temporarily sets the rule sets in the <see cref="EditContext.Properties"/> before triggering validation,
    /// then removes them after validation completes. The validation result is determined by checking if there are any
    /// validation messages using <see cref="EditContext.GetValidationMessages()"/>.
    /// </para>
    /// <para>
    /// This method does not wait for asynchronous validation to complete, so the result may not reflect all validation
    /// rules. Asynchronous rules are applied to the validation message store when they complete. On .NET 11 and later,
    /// this method is obsolete; use <see cref="ValidateAsync(EditContext, IEnumerable{string}, CancellationToken)"/> instead.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editContext"/> is <see langword="null"/>.</exception>
#if NET11_0_OR_GREATER
    [Obsolete("Validate is obsolete. Use ValidateAsync instead.")]
#endif
    public static bool Validate(this EditContext editContext, params IEnumerable<string> ruleSets)
    {
        ArgumentNullException.ThrowIfNull(editContext);

        // set rule sets in properties if provided, removed when the scope is disposed
        using (EditContextScope.SetRuleSets(editContext, ruleSets))
            editContext.Validate();

        // the validation will update the message store, check if there are any messages
        return !editContext.GetValidationMessages().Any();
    }

    /// <summary>
    /// Asynchronously validates the <see cref="EditContext"/> using the specified rule sets.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to validate.</param>
    /// <param name="ruleSets">Validation rule sets to apply. When provided, overrides any rule sets configured on the validator component.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous validation operation.
    /// The task result is <see langword="true"/> if validation succeeded (no validation messages); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This overload is a convenience for passing rule sets inline. It is equivalent to calling
    /// <see cref="ValidateAsync(EditContext, IEnumerable{string}, CancellationToken)"/> without a cancellation token.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editContext"/> is <see langword="null"/>.</exception>
    public static Task<bool> ValidateAsync(this EditContext editContext, params IEnumerable<string> ruleSets)
        => ValidateAsync(editContext, ruleSets, CancellationToken.None);

    /// <summary>
    /// Asynchronously validates the <see cref="EditContext"/> using the specified rule sets,
    /// waiting for all asynchronous validation to complete.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to validate.</param>
    /// <param name="ruleSets">Optional validation rule sets to apply. When provided, overrides any rule sets configured on the validator component.</param>
    /// <param name="cancellationToken">A token to observe while waiting for validation to complete.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous validation operation.
    /// The task result is <see langword="true"/> if validation succeeded (no validation messages); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method temporarily sets the rule sets in the <see cref="EditContext.Properties"/>, then delegates to
    /// <c>EditContext.ValidateAsync(CancellationToken)</c>. The rule sets are removed after validation completes.
    /// </para>
    /// <para>
    /// On .NET 11 and later, this uses the native <c>EditContext.ValidateAsync(CancellationToken)</c> method, which awaits
    /// all asynchronous validators registered with the <see cref="EditContext"/>. On earlier frameworks, the
    /// <c>ValidateAsync(EditContext, CancellationToken)</c> extension method in this class is used instead.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editContext"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown on .NET 11 and later when <paramref name="cancellationToken"/> is canceled.</exception>
    public static async Task<bool> ValidateAsync(this EditContext editContext, IEnumerable<string>? ruleSets = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editContext);

        // set rule sets in properties if provided, removed when the scope is disposed
        using var ruleSetScope = EditContextScope.SetRuleSets(editContext, ruleSets);

        return await editContext.ValidateAsync(cancellationToken).ConfigureAwait(false);
    }

#if !NET11_0_OR_GREATER
    /// <summary>
    /// Asynchronously validates the <see cref="EditContext"/> by first performing synchronous validation,
    /// then waiting for any pending asynchronous validation task to complete.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to validate.</param>
    /// <param name="cancellationToken">
    /// A cancellation token. It is not observed on frameworks earlier than .NET 11 and is provided for source
    /// compatibility with the native <c>EditContext.ValidateAsync(CancellationToken)</c> method.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous validation operation.
    /// The task result is <see langword="true"/> if validation succeeded (no validation messages); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is only available on frameworks earlier than .NET 11. On .NET 11 and later, the native
    /// <c>EditContext.ValidateAsync(CancellationToken)</c> instance method is used instead.
    /// </para>
    /// <para>
    /// This method performs validation in two phases:
    /// <list type="number">
    /// <item>
    /// <description>Flags the request as asynchronous in the <see cref="EditContext.Properties"/> and calls <see cref="EditContext.Validate()"/>, which starts the <see cref="FluentValidator"/> asynchronous validation task.</description>
    /// </item>
    /// <item>
    /// <description>Checks for a pending validation task stored in the <see cref="EditContext.Properties"/> under <see cref="FluentValidator.PendingTask"/> and awaits its completion.</description>
    /// </item>
    /// </list>
    /// After validation completes, the pending validation task is removed from the <see cref="EditContext.Properties"/>.
    /// The validation result is determined by checking if there are any validation messages using <see cref="EditContext.GetValidationMessages()"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editContext"/> is <see langword="null"/>.</exception>
    public static async Task<bool> ValidateAsync(this EditContext editContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(editContext);

        // flag the request as async so FluentValidator runs async validation and stores a pending task
        using (EditContextScope.Set(editContext, FluentValidator.AsyncRequestProperty, value: true))
        {
#pragma warning disable MA0042 // Do not use blocking calls in an async method
            editContext.Validate();
#pragma warning restore MA0042 // Do not use blocking calls in an async method
        }

        // check for any pending async validation task
        if (editContext.Properties.TryGetValue(FluentValidator.PendingTask, out var pendingTask)
            && pendingTask is Task task)
        {
            // await the async validation task to complete, pending task will update the message store when done
            await task.ConfigureAwait(false);

            // remove the completed task from properties
            editContext.Properties.Remove(FluentValidator.PendingTask);
        }

        // the validation will update the message store, check if there are any messages
        return !editContext.GetValidationMessages().Any();
    }
#endif
}
