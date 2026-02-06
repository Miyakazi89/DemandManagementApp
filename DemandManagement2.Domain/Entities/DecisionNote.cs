namespace DemandManagement2.Domain.Entities;

public class DecisionNote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    // Meeting/Decision details
    public string MeetingDate { get; set; } = string.Empty;
    public string Attendees { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string ActionItems { get; set; } = string.Empty;

    public string RecordedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
