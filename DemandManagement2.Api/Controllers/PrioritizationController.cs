using DemandManagement2.Api.Dtos;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/prioritization")]
public class PrioritizationController : ControllerBase
{
    private readonly AppDbContext _db;
    public PrioritizationController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DemandListItemDto>>> Get()
    {
        var query = _db.DemandRequests
            .Include(d => d.Assessment)
            .AsQueryable();

        // Requesters only see their own demands
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "Requester")
        {
            var userName = User.Identity?.Name ?? "";
            query = query.Where(d => d.RequestedBy.ToLower() == userName.ToLower());
        }

        var items = await query
            .OrderByDescending(d => d.Assessment != null ? d.Assessment.WeightedScore : 0m)
            .ThenByDescending(d => d.Urgency)
            .ThenByDescending(d => d.CreatedAtUtc)
            .Select(d => new DemandListItemDto(
                d.Id,
                d.Title,
                d.Type.ToString(),
                d.Status.ToString(),
                d.BusinessUnit,
                d.RequestedBy,
                d.Urgency,
                d.EstimatedEffort,
                d.Assessment != null ? d.Assessment.WeightedScore : null,
                d.CreatedAtUtc,
                d.TargetDate
            ))
            .ToListAsync();

        return Ok(items);
    }
}