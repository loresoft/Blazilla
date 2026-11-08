using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla;

/// <summary>
/// Temporarily sets a value in <see cref="EditContext.Properties"/> and removes it when disposed.
/// </summary>
/// <remarks>
/// Intended for use with a <see langword="using"/> statement. The <see langword="default"/> instance
/// does nothing when disposed, so it can be used when the property is set conditionally.
/// </remarks>
internal readonly struct EditContextScope : IDisposable
{
    private readonly EditContext? _editContext;
    private readonly object? _key;

    private EditContextScope(EditContext editContext, object key)
    {
        _editContext = editContext;
        _key = key;
    }

    /// <summary>
    /// Sets <paramref name="value"/> in the <see cref="EditContext.Properties"/> of <paramref name="editContext"/>
    /// under <paramref name="key"/> until the returned scope is disposed.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> whose properties are updated.</param>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    /// <returns>A scope that removes the property when disposed.</returns>
    public static EditContextScope Set(EditContext editContext, object key, object value)
    {
        ArgumentNullException.ThrowIfNull(editContext);
        ArgumentNullException.ThrowIfNull(key);

        editContext.Properties[key] = value;
        return new EditContextScope(editContext, key);
    }

    /// <summary>
    /// Sets the rule sets in the <see cref="EditContext.Properties"/> of <paramref name="editContext"/>
    /// under <see cref="FluentValidator.RuleSetProperty"/> until the returned scope is disposed.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> whose properties are updated.</param>
    /// <param name="ruleSets">The rule sets to apply.</param>
    /// <returns>
    /// A scope that removes the rule sets when disposed, or a no-op scope when <paramref name="ruleSets"/>
    /// is <see langword="null"/> or empty.
    /// </returns>
    public static EditContextScope SetRuleSets(EditContext editContext, IEnumerable<string>? ruleSets)
    {
        if (ruleSets?.Any() != true)
            return default;

        return Set(editContext, FluentValidator.RuleSetProperty, ruleSets);
    }

    /// <summary>
    /// Removes the property from the <see cref="EditContext.Properties"/>.
    /// </summary>
    public void Dispose()
    {
        if (_editContext != null && _key != null)
            _editContext.Properties.Remove(_key);
    }
}
