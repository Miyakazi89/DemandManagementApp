namespace DemandManagement2.Domain.Entities;

public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    // Capacity in hours per month
    public int CapacityHoursPerMonth { get; set; } = 160;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ResourceAllocation> Allocations { get; set; } = new List<ResourceAllocation>();
}

public class ResourceAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = default!;

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    // Allocated hours for this demand
    public int AllocatedHours { get; set; }

    // Month/Year for the allocation
    public int Month { get; set; }
    public int Year { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
