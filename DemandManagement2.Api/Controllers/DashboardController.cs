using DemandManagement2.Infrastructure.Data;
using DemandManagement2.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult> Summary()
    {
        var total = await _db.DemandRequests.CountAsync();

        var byStatus = await _db.DemandRequests
            .GroupBy(d => d.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return Ok(new { Total = total, ByStatus = byStatus });
    }

    [HttpGet("aging")]
    public async Task<ActionResult> Aging([FromQuery] int warnDays = 14)
    {
        var now = DateTime.UtcNow;

        var items = await _db.DemandRequests
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
