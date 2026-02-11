namespace DemandManagement2.Domain.Entities;

public class Assessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    // Scoring inputs (1-5)
    public int BusinessValue { get; set; }
    public int CostImpact { get; set; }
    public int Risk { get; set; }
    public int ResourceNeed { get; set; }
    public int StrategicAlignment { get; set; }

    // Weighted score (0-100 typically)
    public decimal WeightedScore { get; set; }

    // NPV Financial Analysis
    public decimal InitialCost { get; set; }
    public decimal AnnualBenefit { get; set; }
    public int ProjectYears { get; set; }
    public decimal DiscountRate { get; set; }      // stored as percent (e.g. 10 means 10%)
    public decimal CalculatedNPV { get; set; }

    // Budget breakdown
    public decimal CapExAmount { get; set; }
    public decimal OpExAmount { get; set; }

    public string AssessedBy { get; set; } = string.Empty;
    public DateTime AssessedAtUtc { get; set; } = DateTime.UtcNow;
}