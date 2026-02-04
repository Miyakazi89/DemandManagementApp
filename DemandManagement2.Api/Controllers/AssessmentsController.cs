using DemandManagement2.Api.Dtos;
using DemandManagement2.Api.Services;
using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[ApiController]
[Route("api/demands/{demandId:guid}/assessment")]
public class AssessmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AssessmentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<AssessmentDto>> Get(Guid demandId)
    {
        var assessment = await _db.Assessments.FirstOrDefaultAsync(a => a.DemandRequestId == demandId);
        if (assessment is null) return NotFound();

        return Ok(new AssessmentDto(
            assessment.Id,
            assessment.DemandRequestId,
            assessment.BusinessValue,
            assessment.CostImpact,
            assessment.Risk,
            assessment.ResourceNeed,
            assessment.StrategicAlignment,
            assessment.WeightedScore,
            assessment.AssessedBy,
            assessment.AssessedAtUtc
        ));
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrUpdate(Guid demandId, CreateOrUpdateAssessmentDto dto)
    {
        var demand = await _db.DemandRequests
            .Include(d => d.Assessment)
            .FirstOrDefaultAsync(d => d.Id == demandId);

        if (demand is null) return NotFound("Demand not found.");

        // Create or update assessment
        var assessment = demand.Assessment ?? new Assessment { DemandRequestId = demandId };

        assessment.BusinessValue = dto.BusinessValue;
        assessment.CostImpact = dto.CostImpact;
        assessment.Risk = dto.Risk;
        assessment.ResourceNeed = dto.ResourceNeed;
        assessment.StrategicAlignment = dto.StrategicAlignment;
        assessment.AssessedBy = dto.AssessedBy;
        assessment.AssessedAtUtc = DateTime.UtcNow;

        // Score (0..100)
        assessment.WeightedScore = ScoringService.CalculateScore(demand, assessment);

        if (demand.Assessment is null)
            _db.Assessments.Add(assessment);

        // Business rule: once assessed, move to UnderReview (if still in Intake)
        if (demand.Status == DemandStatus.Intake)
            demand.Status = DemandStatus.UnderReview;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}