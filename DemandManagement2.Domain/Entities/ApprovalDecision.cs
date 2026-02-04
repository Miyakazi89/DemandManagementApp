namespace DemandManagement2.Domain.Entities;

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    OnHold = 2,
    Rejected = 3
}

public class ApprovalDecision
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string DecisionBy { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public DateTime? DecidedAtUtc { get; set; }
}
