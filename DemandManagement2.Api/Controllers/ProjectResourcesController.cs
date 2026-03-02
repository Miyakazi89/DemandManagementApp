using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/project-resources")]
public class ProjectResourcesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProjectResourcesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ProjectResourceDto>>> GetAll([FromQuery] Guid? demandId)
    {
        var query = _db.ProjectResources
            .Include(r => r.DemandRequest)
            .AsQueryable();

        if (demandId.HasValue)
            query = query.Where(r => r.DemandRequestId == demandId.Value);

        var items = await query
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.CreatedAtUtc)
            .Select(r => new ProjectResourceDto(
                r.Id,
                r.DemandRequestId,
                r.DemandRequest.Title,
                r.ResourceType.ToString(),
                r.Name,
                r.Description,
                r.EstimatedCost,
                r.Currency,
                r.Quantity,
                r.Supplier,
                r.Owner,
                r.Notes,
                r.Status,
                r.CreatedAtUtc,
                r.SortOrder
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResourceDto>> GetById(Guid id)
    {
        var r = await _db.ProjectResources
            .Include(r => r.DemandRequest)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (r is null) return NotFound();

        return Ok(new ProjectResourceDto(
            r.Id, r.DemandRequestId, r.DemandRequest.Title,
            r.ResourceType.ToString(), r.Name, r.Description,
            r.EstimatedCost, r.Currency, r.Quantity,
            r.Supplier, r.Owner, r.Notes, r.Status, r.CreatedAtUtc, r.SortOrder
        ));
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpPost]
    public async Task<ActionResult> Create(CreateProjectResourceDto dto)
    {
        var demand = await _db.DemandRequests.FindAsync(dto.DemandRequestId);
        if (demand is null) return BadRequest("Demand not found.");

        // Place new resource at the end
        var maxOrder = await _db.ProjectResources.MaxAsync(r => (int?)r.SortOrder) ?? -1;

        var resource = new ProjectResource
        {
            DemandRequestId = dto.DemandRequestId,
            ResourceType = dto.ResourceType,
            Name = dto.Name,
            Description = dto.Description,
            EstimatedCost = dto.EstimatedCost,
            Currency = dto.Currency,
            Quantity = dto.Quantity,
            Supplier = dto.Supplier,
            Owner = dto.Owner,
            Notes = dto.Notes,
            Status = dto.Status,
            SortOrder = maxOrder + 1
        };

        _db.ProjectResources.Add(resource);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, new { resource.Id });
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, CreateProjectResourceDto dto)
    {
        var resource = await _db.ProjectResources.FindAsync(id);
        if (resource is null) return NotFound();

        resource.ResourceType = dto.ResourceType;
        resource.Name = dto.Name;
        resource.Description = dto.Description;
        resource.EstimatedCost = dto.EstimatedCost;
        resource.Currency = dto.Currency;
        resource.Quantity = dto.Quantity;
        resource.Supplier = dto.Supplier;
        resource.Owner = dto.Owner;
        resource.Notes = dto.Notes;
        resource.Status = dto.Status;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var resource = await _db.ProjectResources.FindAsync(id);
        if (resource is null) return NotFound();

        _db.ProjectResources.Remove(resource);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpPost("reorder")]
    public async Task<ActionResult> Reorder(List<ReorderItemDto> items)
    {
        var ids = items.Select(i => i.Id).ToList();
        var resources = await _db.ProjectResources
            .Where(r => ids.Contains(r.Id))
            .ToListAsync();

        foreach (var item in items)
        {
            var r = resources.FirstOrDefault(x => x.Id == item.Id);
            if (r is not null) r.SortOrder = item.SortOrder;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record ProjectResourceDto(
    Guid Id,
    Guid DemandRequestId,
    string DemandTitle,
    string ResourceType,
    string Name,
    string? Description,
    decimal? EstimatedCost,
    string? Currency,
    int? Quantity,
    string? Supplier,
    string? Owner,
    string? Notes,
    string Status,
    DateTime CreatedAtUtc,
    int SortOrder
);

public record ReorderItemDto(Guid Id, int SortOrder);

public class CreateProjectResourceDto
{
    public Guid DemandRequestId { get; set; }
    public ProjectResourceType ResourceType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Currency { get; set; }
    public int? Quantity { get; set; }
    public string? Supplier { get; set; }
    public string? Owner { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
}
