using DemandManagement2.Api.Dtos;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace DemandManagement2.Api.Controllers;

[ApiController]
[Route("api/prioritization")]
public class PrioritizationController : ControllerBase
{
    private readonly AppDbContext _db;
    public PrioritizationController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DemandListItemDto>>> Get()
    {
        var items = await _db.DemandRequests
            .Include(d => d.Assessment)
            .OrderByDescending(d => d.Assessment != null ? d.Assessment.WeightedScore : 0m)
            .ThenByDescending(d => d.Urgency)
            .ThenByDescending(d => d.CreatedAtUtc)
            .Select(d => new DemandListItemDto(
                d.Id,
                d.Title,
                d.Type,
                d.Status,
                d.BusinessUnit,
                d.RequestedBy,
                d.Urgency,
                d.EstimatedEffort,
                d.Assessment != null ? d.Assessment.WeightedScore : null,
                d.CreatedAtUtc
            ))
            .ToListAsync();

        return Ok(items);
    }
}