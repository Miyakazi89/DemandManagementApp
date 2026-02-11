using DemandManagement2.Api.Dtos;
using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize(Roles = "Admin,Assessor")]
[ApiController]
[Route("api/budget")]
public class BudgetController : ControllerBase
{
    private readonly AppDbContext _db;
    public BudgetController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummaryDto>> GetSummary([FromQuery] int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        // Get all assessed demands (not rejected)
        var demands = await _db.DemandRequests
            .Include(d => d.Assessment)
            .Where(d => d.Assessment != null && d.Status != DemandStatus.Rejected)
            .ToListAsync();

        var budgetEntries = await _db.BudgetEntries
            .Where(b => b.Year == targetYear)
            .ToListAsync();

        var totalPlanned = budgetEntries.Sum(b => b.PlannedAmount);
        var totalActual = budgetEntries.Sum(b => b.ActualAmount);
        var totalCapEx = demands.Sum(d => d.Assessment!.CapExAmount);
        var totalOpEx = demands.Sum(d => d.Assessment!.OpExAmount);
        var totalNPV = demands.Sum(d => d.Assessment!.CalculatedNPV);

        // Monthly breakdown
        var months = Enumerable.Range(1, 12).Select(m =>
        {
            var monthEntries = budgetEntries.Where(b => b.Month == m).ToList();
            var cumulativeBenefit = budgetEntries.Where(b => b.Month <= m).Sum(b => b.ActualAmount);
            return new MonthlyBudgetDto(
                m, targetYear,
                new DateTime(targetYear, m, 1).ToString("MMM yyyy"),
                monthEntries.Sum(e => e.PlannedAmount),
                monthEntries.Sum(e => e.ActualAmount),
                cumulativeBenefit
            );
        }).ToList();

        // Per-demand breakdown
        var byDemand = demands.Select(d =>
        {
            var entries = budgetEntries.Where(b => b.DemandRequestId == d.Id).ToList();
            return new DemandBudgetDto(
                d.Id, d.Title, d.Status.ToString(),
                d.Assessment!.InitialCost, d.Assessment.AnnualBenefit, d.Assessment.CalculatedNPV,
                d.Assessment.CapExAmount, d.Assessment.OpExAmount,
                entries.Sum(e => e.PlannedAmount),
                entries.Sum(e => e.ActualAmount)
            );
        }).ToList();

        return Ok(new BudgetSummaryDto(totalPlanned, totalActual, totalPlanned - totalActual,
            totalCapEx, totalOpEx, totalNPV, months, byDemand));
    }

    [HttpGet("entries")]
    public async Task<ActionResult<List<BudgetEntryDto>>> GetEntries(
        [FromQuery] Guid? demandId = null, [FromQuery] int? year = null)
    {
        var query = _db.BudgetEntries
            .Include(b => b.DemandRequest)
            .AsQueryable();

        if (demandId.HasValue)
            query = query.Where(b => b.DemandRequestId == demandId.Value);
        if (year.HasValue)
            query = query.Where(b => b.Year == year.Value);

        var entries = await query.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
            .Select(b => new BudgetEntryDto(
                b.Id, b.DemandRequestId, b.DemandRequest.Title,
                b.Month, b.Year,
                b.PlannedAmount, b.ActualAmount,
                b.Category, b.Notes, b.CreatedAtUtc
            ))
            .ToListAsync();

        return Ok(entries);
    }

    [HttpPost("entries")]
    public async Task<ActionResult> CreateEntry(CreateBudgetEntryDto dto)
    {
        var demand = await _db.DemandRequests.FindAsync(dto.DemandRequestId);
        if (demand is null) return NotFound("Demand not found.");

        var entry = new BudgetEntry
        {
            DemandRequestId = dto.DemandRequestId,
            Month = dto.Month,
            Year = dto.Year,
            PlannedAmount = dto.PlannedAmount,
            ActualAmount = dto.ActualAmount,
            Category = dto.Category,
            Notes = dto.Notes
        };

        _db.BudgetEntries.Add(entry);
        await _db.SaveChangesAsync();

        return Ok(new { entry.Id });
    }
}
