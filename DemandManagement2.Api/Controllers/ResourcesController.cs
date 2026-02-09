using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ResourcesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ResourceDto>>> GetAll()
    {
        var resources = await _db.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Department)
            .ThenBy(r => r.Name)
            .Select(r => new ResourceDto(
                r.Id,
                r.Name,
                r.Role,
                r.Department,
                r.CapacityHoursPerMonth,
                r.IsActive
            ))
            .ToListAsync();

        return Ok(resources);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceDto>> GetById(Guid id)
    {
        var r = await _db.Resources.FindAsync(id);
        if (r is null) return NotFound();

        return Ok(new ResourceDto(r.Id, r.Name, r.Role, r.Department, r.CapacityHoursPerMonth, r.IsActive));
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateResourceDto dto)
    {
        var resource = new Resource
        {
            Name = dto.Name,
            Role = dto.Role,
            Department = dto.Department,
            CapacityHoursPerMonth = dto.CapacityHoursPerMonth
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, new { resource.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, CreateResourceDto dto)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource is null) return NotFound();

        resource.Name = dto.Name;
        resource.Role = dto.Role;
        resource.Department = dto.Department;
        resource.CapacityHoursPerMonth = dto.CapacityHoursPerMonth;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource is null) return NotFound();

        resource.IsActive = false; // Soft delete
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// DTOs
public record ResourceDto(
    Guid Id,
    string Name,
    string Role,
    string Department,
    int CapacityHoursPerMonth,
    bool IsActive
);

public class CreateResourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int CapacityHoursPerMonth { get; set; } = 160;
}
