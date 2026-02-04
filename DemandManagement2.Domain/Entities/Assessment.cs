namespace DemandManagement2.Domain.Entities;

public class Assessment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    // scoring 1..5
    public int BusinessValue { get; set; }
    public int CostImpact { get; set; }
    public int Risk { get; set; }
    public int ResourceNeed { get; set; }
    public int StrategicAlignment { get; set; }

    public decimal WeightedScore { get; set; } // calculated

    public string AssessedBy { get; set; } = string.Empty;
    public DateTime AssessedAtUtc { get; set; } = DateTime.UtcNow;
}