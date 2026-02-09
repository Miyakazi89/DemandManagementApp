using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/capacity")]
public class CapacityController : ControllerBase
{
    private readonly AppDbContext _db;
    public CapacityController(AppDbContext db) => _db = db;

    // Get capacity summary for a given month/year
    [HttpGet("summary")]
    public async Task<ActionResult<CapacitySummaryDto>> GetSummary([FromQuery] int? month = null, [FromQuery] int? year = null)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        // Get all active resources
        var resources = await _db.Resources.Where(r => r.IsActive).ToListAsync();
        var totalCapacity = resources.Sum(r => r.CapacityHoursPerMonth);

        // Get allocations for the month
        var allocations = await _db.ResourceAllocations
            .Where(a => a.Month == targetMonth && a.Year == targetYear)
            .ToListAsync();

        var totalAllocated = allocations.Sum(a => a.AllocatedHours);
        var availableCapacity = totalCapacity - totalAllocated;
        var utilizationPercent = totalCapacity > 0 ? (decimal)totalAllocated / totalCapacity * 100 : 0;

        // Get capacity by department
        var byDepartment = resources
            .GroupBy(r => r.Department)
            .Select(g => new DepartmentCapacityDto(
                g.Key,
                g.Sum(r => r.CapacityHoursPerMonth),
                allocations.Where(a => g.Any(r => r.Id == a.ResourceId)).Sum(a => a.AllocatedHours)
            ))
            .ToList();

        return Ok(new CapacitySummaryDto(
            targetMonth,
            targetYear,
            resources.Count,
            totalCapacity,
            totalAllocated,
            availableCapacity,
            Math.Round(utilizationPercent, 1),
            byDepartment
        ));
    }

    // Get resource allocations
    [HttpGet("allocations")]
    public async Task<ActionResult<List<AllocationDto>>> GetAllocations([FromQuery] int? month = null, [FromQuery] int? year = null)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var allocations = await _db.ResourceAllocations
            .Include(a => a.Resource)
            .Include(a => a.DemandRequest)
            .Where(a => a.Month == targetMonth && a.Year == targetYear)
            .Select(a => new AllocationDto(
                a.Id,
                a.ResourceId,
                a.Resource.Name,
                a.DemandRequestId,
                a.DemandRequest.Title,
                a.AllocatedHours,
                a.Month,
                a.Year,
                a.Notes
            ))
            .ToListAsync();

        return Ok(allocations);
    }

    // Create allocation
    [HttpPost("allocations")]
    public async Task<ActionResult> CreateAllocation(CreateAllocationDto dto)
    {
        var resource = await _db.Resources.FindAsync(dto.ResourceId);
        if (resource is null) return BadRequest("Resource not found.");

        var demand = await _db.DemandRequests.FindAsync(dto.DemandRequestId);
        if (demand is null) return BadRequest("Demand not found.");

        var allocation = new ResourceAllocation
        {
            ResourceId = dto.ResourceId,
            DemandRequestId = dto.DemandRequestId,
            AllocatedHours = dto.AllocatedHours,
            Month = dto.Month,
            Year = dto.Year,
            Notes = dto.Notes ?? string.Empty
        };

        _db.ResourceAllocations.Add(allocation);
        await _db.SaveChangesAsync();

        return Ok(new { allocation.Id });
    }

    // Delete allocation
    [HttpDelete("allocations/{id:guid}")]
    public async Task<ActionResult> DeleteAllocation(Guid id)
    {
        var allocation = await _db.ResourceAllocations.FindAsync(id);
        if (allocation is null) return NotFound();

        _db.ResourceAllocations.Remove(allocation);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Get demand vs capacity forecast (next 6 months)
    [HttpGet("forecast")]
    public async Task<ActionResult<List<ForecastDto>>> GetForecast()
    {
        var resources = await _db.Resources.Where(r => r.IsActive).ToListAsync();
        var totalCapacity = resources.Sum(r => r.CapacityHoursPerMonth);

        var forecast = new List<ForecastDto>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < 6; i++)
        {
            var date = now.AddMonths(i);
            var month = date.Month;
            var year = date.Year;

            var allocated = await _db.ResourceAllocations
                .Where(a => a.Month == month && a.Year == year)
                .SumAsync(a => a.AllocatedHours);

            forecast.Add(new ForecastDto(
                month,
                year,
                $"{date:MMM yyyy}",
                totalCapacity,
                allocated,
                totalCapacity - allocated
            ));
        }

        return Ok(forecast);
    }
}

// DTOs
public record CapacitySummaryDto(
    int Month,
    int Year,
    int TotalResources,
    int TotalCapacityHours,
    int AllocatedHours,
    int AvailableHours,
    decimal UtilizationPercent,
    List<DepartmentCapacityDto> ByDepartment
);

public record DepartmentCapacityDto(
    string Department,
    int CapacityHours,
    int AllocatedHours
);

public record AllocationDto(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid DemandRequestId,
    string DemandTitle,
    int AllocatedHours,
    int Month,
    int Year,
    string Notes
);

public record ForecastDto(
    int Month,
    int Year,
    string Label,
    int CapacityHours,
    int AllocatedHours,
    int AvailableHours
);

public class CreateAllocationDto
{
    public Guid ResourceId { get; set; }
    public Guid DemandRequestId { get; set; }
    public int AllocatedHours { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string? Notes { get; set; }
}
