namespace BlazorShared.Models;

public record Department
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? ManagerId { get; set; }

    public decimal Budget { get; set; }
}
