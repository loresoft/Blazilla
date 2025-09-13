namespace BlazorShared.Models;

public record Company
{
    public string? Name { get; set; }

    public string? RegistrationNumber { get; set; }

    public Address? HeadquartersAddress { get; set; } = new();

    public List<Department> Departments { get; set; } = [];

    public List<Project> Projects { get; set; } = [];

    public CompanySettings? Settings { get; set; } = new();

    public List<Employee> Employees { get; set; } = [];
}
