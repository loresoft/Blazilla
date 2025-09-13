namespace BlazorShared.Models;

public record Address
{
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? AddressLine3 { get; set; }

    public string? City { get; set; }

    public string? StateProvince { get; set; }

    public string? PostalCode { get; set; }
}
