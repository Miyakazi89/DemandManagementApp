namespace DemandManagement2.Domain.Entities;

public enum ProjectResourceType
{
    Financial,
    Physical,
    InformationKnowledge,
    GovernanceSupport
}

public class ProjectResource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = null!;

    public ProjectResourceType ResourceType { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Financial
    public decimal? EstimatedCost { get; set; }
    public string? Currency { get; set; }

    // Physical
    public int? Quantity { get; set; }

    // Financial + Physical
    public string? Supplier { get; set; }

    // Information/Knowledge + Governance/Support
    public string? Owner { get; set; }

    public string? Notes { get; set; }

    // Pending | In Progress | Approved | Available | Acquired | In Place
    public string Status { get; set; } = "Pending";

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
