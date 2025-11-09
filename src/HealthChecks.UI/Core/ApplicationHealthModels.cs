namespace HealthChecks.UI.Core;

public class ApplicationHealthReport
{
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int HealthyCount { get; set; }
    public int TotalCount { get; set; }
    public double AverageDurationMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public List<MemberHealthReport> Members { get; set; } = new List<MemberHealthReport>();
}

public class MemberHealthReport
{
    public string Name { get; set; } = null!;
    public string Uri { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long DurationMs { get; set; }
    public string? Payload { get; set; }
}
