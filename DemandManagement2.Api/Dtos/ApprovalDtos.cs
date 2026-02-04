using System.ComponentModel.DataAnnotations;
using DemandManagement2.Domain.Entities;

namespace DemandManagement2.Api.Dtos;

public class CreateApprovalDto
{
    [Required]
    public ApprovalStatus Status { get; set; } // Approved / Rejected / OnHold

    [Required, MinLength(2), MaxLength(200)]
    public string DecisionBy { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Comments { get; set; } = string.Empty;
}

public record ApprovalDto(
    Guid Id,
    Guid DemandRequestId,
    ApprovalStatus Status,
    string DecisionBy,
    string Comments,
    DateTime? DecidedAtUtc
);