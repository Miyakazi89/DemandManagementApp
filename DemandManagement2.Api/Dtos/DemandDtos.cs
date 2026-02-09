using System.ComponentModel.DataAnnotations;
using DemandManagement2.Domain.Entities;

namespace DemandManagement2.Api.Dtos;


public record DemandListItemDto(
    Guid Id,
    string Title,
    string Type,
    string Status,
    string BusinessUnit,
    string RequestedBy,
    int Urgency,
    int EstimatedEffort,
    decimal? WeightedScore,
    DateTime CreatedAtUtc,
    DateTime? TargetDate
);


public record DemandDetailsDto(
    Guid Id,
    string Title,
    string ProblemStatement,
    DemandType Type,
    DemandStatus Status,
    string BusinessUnit,
    string RequestedBy,
    int Urgency,
    int EstimatedEffort,
    decimal? WeightedScore,
    AssessmentDto? Assessment,
    ApprovalDto? Approval,
    DateTime CreatedAtUtc,
    DateTime? TargetDate,
    List<DemandEventDto> Events,
    List<DemandAttachmentDto> Attachments
);

public class CreateDemandRequestDto
{
    [Required, MinLength(3), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MinLength(5), MaxLength(4000)]
    public string ProblemStatement { get; set; } = string.Empty;

    [Required]
    public DemandType Type { get; set; }

    [Required, MinLength(2), MaxLength(200)]
    public string BusinessUnit { get; set; } = string.Empty;

    [Required, MinLength(2), MaxLength(200)]
    public string RequestedBy { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Urgency { get; set; }

    [Range(1, 5)]
    public int EstimatedEffort { get; set; }

    public DateTime? TargetDate { get; set; }
}

public record DemandEventDto(
    Guid Id,
    string EventType,
    string Description,
    string PerformedBy,
    DateTime OccurredAtUtc
);

public record DemandAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string UploadedBy,
    DateTime UploadedAtUtc,
    string DownloadUrl
);

public class RequestInfoDto
{
    public string Message { get; set; } = string.Empty;
}
