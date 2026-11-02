using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class RuleSetsValidator :
    AbstractValidator<RuleSetsModel>
{
    public const string RuleSetA = "Rule Set A";
    public const string RuleSetB = "Rule Set B";
    public const string RuleSetC = "Rule Set C";

    public RuleSetsValidator()
    {
        RuleFor(static e => e.FieldD)
            .NotEmpty();

        RuleSet(
            RuleSetA,
            () =>
            {
                RuleFor(static e => e.FieldA)
                    .NotEmpty();

                RuleFor(static e => e.FieldB)
                    .NotEmpty();
            });

        RuleSet(
            RuleSetB,
            () =>
            {
                RuleFor(static e => e.FieldB)
                    .NotEmpty();
            });

        RuleSet(
            RuleSetC,
            () =>
            {
                RuleFor(static e => e.FieldC)
                    .NotEmpty();

                RuleFor(static e => e.FieldD)
                    .NotEmpty();
            });
    }
}
