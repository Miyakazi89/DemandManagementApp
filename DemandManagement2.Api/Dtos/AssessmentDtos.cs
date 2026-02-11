using System.ComponentModel.DataAnnotations;

namespace DemandManagement2.Api.Dtos;

public class CreateOrUpdateAssessmentDto
{
    [Range(1, 5)] public int BusinessValue { get; set; }
    [Range(1, 5)] public int CostImpact { get; set; }         // higher = more expensive
    [Range(1, 5)] public int Risk { get; set; }               // higher = riskier
    [Range(1, 5)] public int ResourceNeed { get; set; }       // higher = more resources
    [Range(1, 5)] public int StrategicAlignment { get; set; } // higher = better alignment

    // NPV Financial Analysis
    [Range(0, double.MaxValue)] public decimal InitialCost { get; set; }      // Initial investment
    [Range(0, double.MaxValue)] public decimal AnnualBenefit { get; set; }    // Expected annual benefit
    [Range(0, 50)] public int ProjectYears { get; set; }                      // Project duration (years)
    [Range(0, 100)] public decimal DiscountRate { get; set; }                 // Discount rate (%)

    // Budget breakdown
    [Range(0, double.MaxValue)] public decimal CapExAmount { get; set; }
    [Range(0, double.MaxValue)] public decimal OpExAmount { get; set; }

    [Required, MinLength(2), MaxLength(200)]
    public string AssessedBy { get; set; } = string.Empty;
}

public record AssessmentDto(
    Guid Id,
    Guid DemandRequestId,
    int BusinessValue,
    int CostImpact,
    int Risk,
    int ResourceNeed,
    int StrategicAlignment,
    decimal WeightedScore,

    // NPV Financial Analysis
    decimal InitialCost,
    decimal AnnualBenefit,
    int ProjectYears,
    decimal DiscountRate,
    decimal CalculatedNPV,

    // Budget breakdown
    decimal CapExAmount,
    decimal OpExAmount,

    string AssessedBy,
    DateTime AssessedAtUtc
);