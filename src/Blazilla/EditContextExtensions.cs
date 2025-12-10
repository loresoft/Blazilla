using FluentValidation.Results;

using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla;

/// <summary>
/// Provides extension methods for the <see cref="EditContext"/> class to facilitate validation operations.
/// </summary>
public static class EditContextExtensions
{
    /// <summary>
    /// Asynchronously validates the <see cref="EditContext"/> by first performing synchronous validation,
    /// then waiting for any pending asynchronous validation tasks to complete.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to validate.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous validation operation.
    /// The task result is <see langword="true"/> if validation succeeded (no validation messages);
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method performs validation in two phases:
    /// <list type="number">
    /// <item>
    /// <description>Calls <see cref="EditContext.Validate()"/> to trigger synchronous validation, which may also initiate asynchronous validation tasks.</description>
    /// </item>
    /// <item>
    /// <description>Checks for any pending asynchronous validation task stored in the <see cref="EditContext.Properties"/> and awaits its completion.</description>
    /// </item>
    /// </list>
    /// After validation completes, any pending validation task is removed from the <see cref="EditContext.Properties"/>.
    /// The validation result is determined by checking if there are any validation messages using <see cref="EditContext.GetValidationMessages()"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="editContext"/> is <see langword="null"/>.</exception>
    public static async Task<bool> ValidateAsync(this EditContext editContext)
    {
        ArgumentNullException.ThrowIfNull(editContext);

        // start with synchronous validation, might trigger async validations

#pragma warning disable MA0042 // Do not use blocking calls in an async method
        editContext.Validate();
#pragma warning restore MA0042 // Do not use blocking calls in an async method

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
}
