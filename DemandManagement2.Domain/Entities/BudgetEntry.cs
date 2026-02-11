namespace DemandManagement2.Domain.Entities;

public class BudgetEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    public int Month { get; set; }
    public int Year { get; set; }

    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }

    public string Category { get; set; } = "";  // "CapEx" or "OpEx"
    public string Notes { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
