namespace BlazorShared.Models;

public record ProjectMilestone
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsCompleted { get; set; }

    public List<string> Deliverables { get; set; } = [];
}
