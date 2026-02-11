using DemandManagement2.Api.Dtos;
using DemandManagement2.Api.Services;
using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize(Roles = "Admin,Assessor")]
[ApiController]
[Route("api/demands/{demandId:guid}/assessment")]
public class AssessmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    public AssessmentsController(AppDbContext db, IEmailService email) { _db = db; _email = email; }

    [HttpGet]
    public async Task<ActionResult<AssessmentDto>> Get(Guid demandId)
    {
        var a = await _db.Assessments.FirstOrDefaultAsync(x => x.DemandRequestId == demandId);
        if (a is null) return NotFound();

        return Ok(ToDto(a));
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrUpdate(Guid demandId, CreateOrUpdateAssessmentDto dto)
    {
        var demand = await _db.DemandRequests
            .Include(d => d.Assessment)
            .FirstOrDefaultAsync(d => d.Id == demandId);

        if (demand is null) return NotFound("Demand not found.");

        var a = demand.Assessment ?? new Assessment { DemandRequestId = demandId };

        // scoring fields
        a.BusinessValue = dto.BusinessValue;
        a.CostImpact = dto.CostImpact;
        a.Risk = dto.Risk;
        a.ResourceNeed = dto.ResourceNeed;
        a.StrategicAlignment = dto.StrategicAlignment;

        // NPV inputs
        a.InitialCost = dto.InitialCost;
        a.AnnualBenefit = dto.AnnualBenefit;
        a.ProjectYears = dto.ProjectYears;
        a.DiscountRate = dto.DiscountRate;

        // Budget breakdown
        a.CapExAmount = dto.CapExAmount;
        a.OpExAmount = dto.OpExAmount;

        // calculated NPV
        a.CalculatedNPV = CalculateNpv(dto.InitialCost, dto.AnnualBenefit, dto.ProjectYears, dto.DiscountRate);

        // assessor
        a.AssessedBy = dto.AssessedBy;
        a.AssessedAtUtc = DateTime.UtcNow;

        // Score (0..100)
        a.WeightedScore = ScoringService.CalculateScore(demand, a);

        if (demand.Assessment is null)
            _db.Assessments.Add(a);

        // Business rule: once assessed, move to UnderReview (if still in Intake)
        if (demand.Status == DemandStatus.Intake)
            demand.Status = DemandStatus.UnderReview;

        // Record timeline event
        var userName = User.Identity?.Name ?? dto.AssessedBy;
        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demandId,
            EventType = "Assessed",
            Description = $"Assessment completed by {userName} (Score: {a.WeightedScore:F1})",
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();

        // Notify requester about assessment
        var requester = await _db.Users.FirstOrDefaultAsync(u => u.FullName == demand.RequestedBy);
        await _email.SendDemandNotificationAsync("Assessed", demand.Id, demand.Title, requester?.Email);

        return NoContent();
    }

    private static AssessmentDto ToDto(Assessment a) => new(
        a.Id,
        a.DemandRequestId,
        a.BusinessValue,
        a.CostImpact,
        a.Risk,
        a.ResourceNeed,
        a.StrategicAlignment,
        a.WeightedScore,
        a.InitialCost,
        a.AnnualBenefit,
        a.ProjectYears,
        a.DiscountRate,
        a.CalculatedNPV,
        a.CapExAmount,
        a.OpExAmount,
        a.AssessedBy,
        a.AssessedAtUtc
    );

    /// <summary>
    /// NPV = -InitialCost + Σ(AnnualBenefit / (1+r)^t) for t=1..years
    /// dto.DiscountRate is stored as percent (e.g. 10 = 10%)
    /// </summary>
    private static decimal CalculateNpv(decimal initialCost, decimal annualBenefit, int years, decimal discountRatePercent)
    {
        if (years <= 0) return Math.Round(-initialCost, 2);

        var r = discountRatePercent / 100m; // convert percent to fraction
        decimal npv = -initialCost;

        for (var t = 1; t <= years; t++)
        {
            var denom = (decimal)Math.Pow((double)(1m + r), t);
            npv += annualBenefit / denom;
        }

        return Math.Round(npv, 2);
    }
}