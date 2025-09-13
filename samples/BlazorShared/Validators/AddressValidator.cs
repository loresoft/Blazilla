using BlazorShared.Models;

using FluentValidation;

namespace BlazorShared.Validators;

public class AddressValidator : AbstractValidator<Address>
{
    public const string Line1Required = "You must enter Address Line 1";
    public const string Line2Required = "You must enter Address Line 2";
    public const string Line3Required = "You must enter Address Line 3";
    public const string CityRequired = "You must enter a city";
    public const string StateProvinceRequired = "You must enter a state or province";
    public const string PostalCodeRequired = "You must enter a postal code";

    public AddressValidator()
    {
        RuleFor(p => p.AddressLine1).NotEmpty().WithMessage(Line1Required);
        RuleFor(p => p.City).NotEmpty().WithMessage(CityRequired);
        RuleFor(p => p.StateProvince).NotEmpty().WithMessage(StateProvinceRequired);
        RuleFor(p => p.PostalCode).NotEmpty().WithMessage(PostalCodeRequired);
    }
}
