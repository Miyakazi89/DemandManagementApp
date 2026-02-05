using DemandManagement2.Domain.Entities;

namespace DemandManagement2.Api.Dtos;

public sealed record DemandListItemDto(
    Guid Id,
    string Title,
    DemandType Type,
    DemandStatus Status,
    string BusinessUnit,
    string RequestedBy,
    int Urgency,
    decimal EstimatedEffort,
    decimal? WeightedScore,
    DateTime CreatedAtUtc
);