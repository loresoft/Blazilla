using FluentValidation;
using FluentValidation.Internal;

namespace Blazilla.ValidatorSelectors;

/// <summary>
/// Combines the wrapped selectors using Boolean AND semantics.
/// </summary>
/// <param name="selectors">
/// The <see cref="IValidatorSelector" /> instances to wrap.
/// </param>
public class BooleanAndCompositeSelector(
    params IEnumerable<IValidatorSelector> selectors) :
    IValidatorSelector
{
    /// <inheritdoc />
    public bool CanExecute(
        IValidationRule rule,
        string propertyPath,
        IValidationContext context) =>
        selectors.All(s => s.CanExecute(rule, propertyPath, context));
}
