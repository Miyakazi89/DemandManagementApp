namespace DemandManagement2.Domain.Entities;

public enum DemandStatus
{
    Intake = 0,
    UnderReview = 1,
    Prioritized = 2,
    Approved = 3,
    Backlog = 4,
    Rejected = 5
}

public enum DemandType
{
    Project = 0,
    Enhancement = 1,
    Service = 2,
    ResourceRequest = 3
}

public class DemandRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string ProblemStatement { get; set; } = string.Empty;

    public DemandType Type { get; set; }
    public DemandStatus Status { get; set; } = DemandStatus.Intake;

    public string BusinessUnit { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;

    public int Urgency { get; set; }          // 1..5
    public int EstimatedEffort { get; set; }  // 1..5 (S,M,L mapped later)

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Assessment? Assessment { get; set; }
    public ApprovalDecision? Approval { get; set; }
}