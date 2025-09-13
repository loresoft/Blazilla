namespace BlazorShared.Models;

public record Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? Age { get; set; }

    public string? EmailAddress { get; set; }

    public Address? Address { get; set; }
}
