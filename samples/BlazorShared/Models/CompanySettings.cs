namespace BlazorShared.Models;

public record CompanySettings
{
    public bool AllowRemoteWork { get; set; }

    public int MaxVacationDays { get; set; }

    public List<string> AllowedEmailDomains { get; set; } = [];

    public Dictionary<string, string> CustomSettings { get; set; } = [];
}
