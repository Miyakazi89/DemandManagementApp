namespace DemandManagement2.Domain.Entities;

public class DemandEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
