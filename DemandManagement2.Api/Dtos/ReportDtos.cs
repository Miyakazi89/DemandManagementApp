namespace DemandManagement2.Api.Dtos;

public class ReportFilterDto
{
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? BusinessUnit { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public record ReportRowDto(
    Guid Id,
    string Title,
    string Type,
    string Status,
    string BusinessUnit,
    string RequestedBy,
    int Urgency,
    int EstimatedEffort,
    decimal? WeightedScore,
    decimal? InitialCost,
    decimal? AnnualBenefit,
    decimal? CalculatedNPV,
    string? ApprovalStatus,
    DateTime CreatedAtUtc,
    DateTime? TargetDate
);
