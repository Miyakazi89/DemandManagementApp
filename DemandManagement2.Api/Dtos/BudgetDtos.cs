using System.ComponentModel.DataAnnotations;

namespace DemandManagement2.Api.Dtos;

public record BudgetSummaryDto(
    decimal TotalPlanned,
    decimal TotalActual,
    decimal Variance,
    decimal TotalCapEx,
    decimal TotalOpEx,
    decimal TotalNPV,
    List<MonthlyBudgetDto> MonthlyBreakdown,
    List<DemandBudgetDto> ByDemand
);

public record MonthlyBudgetDto(int Month, int Year, string Label, decimal Planned, decimal Actual, decimal CumulativeBenefit);

public record DemandBudgetDto(
    Guid DemandId, string Title, string Status,
    decimal InitialCost, decimal AnnualBenefit, decimal CalculatedNPV,
    decimal CapEx, decimal OpEx,
    decimal PlannedTotal, decimal ActualTotal
);

public class CreateBudgetEntryDto
{
    [Required] public Guid DemandRequestId { get; set; }
    [Range(1, 12)] public int Month { get; set; }
    [Range(2020, 2100)] public int Year { get; set; }
    [Range(0, double.MaxValue)] public decimal PlannedAmount { get; set; }
    [Range(0, double.MaxValue)] public decimal ActualAmount { get; set; }
    public string Category { get; set; } = "CapEx";
    public string Notes { get; set; } = "";
}

public record BudgetEntryDto(
    Guid Id, Guid DemandRequestId, string DemandTitle,
    int Month, int Year,
    decimal PlannedAmount, decimal ActualAmount,
    string Category, string Notes, DateTime CreatedAtUtc
);
