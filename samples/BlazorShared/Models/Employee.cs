namespace BlazorShared.Models;

public record Employee
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public decimal Salary { get; set; }

    public DateTime HireDate { get; set; }

    public int DepartmentId { get; set; }

    public List<string> Skills { get; set; } = [];

    public List<int> AssignedProjects { get; set; } = [];

    public Address? HomeAddress { get; set; } = new();
}
