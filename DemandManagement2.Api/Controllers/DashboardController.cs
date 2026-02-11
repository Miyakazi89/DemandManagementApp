using DemandManagement2.Infrastructure.Data;
using DemandManagement2.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult> Summary()
    {
        var query = _db.DemandRequests.AsQueryable();

        // Requesters only see their own demands
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "Requester")
        {
            var userName = User.Identity?.Name ?? "";
            query = query.Where(d => d.RequestedBy.ToLower() == userName.ToLower());
        }

        var total = await query.CountAsync();

        var byStatus = await query
            .GroupBy(d => d.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return Ok(new { Total = total, ByStatus = byStatus });
    }

    [HttpGet("aging")]
    public async Task<ActionResult> Aging([FromQuery] int warnDays = 14)
    {
        var now = DateTime.UtcNow;
        var query = _db.DemandRequests.AsQueryable();

        // Requesters only see their own demands
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "Requester")
        {
            var userName = User.Identity?.Name ?? "";
            query = query.Where(d => d.RequestedBy.ToLower() == userName.ToLower());
        }

        var items = await query
            .Select(d => new
            {
                d.Id,
                d.Title,
                Status = d.Status.ToString(),
                DaysOpen = EF.Functions.DateDiffDay(d.CreatedAtUtc, now),
                IsOverdue = EF.Functions.DateDiffDay(d.CreatedAtUtc, now) >= warnDays
            })
            .OrderByDescending(x => x.DaysOpen)
            .ToListAsync();

        return Ok(items);
    }
}
