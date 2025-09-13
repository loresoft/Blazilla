namespace BlazorShared.Models;

public record Project
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal Budget { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public List<ProjectMilestone> Milestones { get; set; } = [];
}
