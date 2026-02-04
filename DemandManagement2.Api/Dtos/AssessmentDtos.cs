using System.ComponentModel.DataAnnotations;

namespace DemandManagement2.Api.Dtos;

public class CreateOrUpdateAssessmentDto
{
    [Range(1, 5)] public int BusinessValue { get; set; }
    [Range(1, 5)] public int CostImpact { get; set; }         // higher = more expensive
    [Range(1, 5)] public int Risk { get; set; }               // higher = riskier
    [Range(1, 5)] public int ResourceNeed { get; set; }       // higher = more resources
    [Range(1, 5)] public int StrategicAlignment { get; set; } // higher = better alignment

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
    string AssessedBy,
    DateTime AssessedAtUtc
);