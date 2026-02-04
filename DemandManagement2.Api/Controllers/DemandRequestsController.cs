using DemandManagement2.Api.Dtos;
using DemandManagement2.Infrastructure.Data;
using DemandManagement2.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[ApiController]
[Route("api/demands")]
public class DemandsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DemandsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DemandListItemDto>>> GetAll(
        [FromQuery] DemandStatus? status = null,
        [FromQuery] DemandType? type = null)
    {
        var query = _db.DemandRequests
            .Include(d => d.Assessment)
            .AsQueryable();

        if (status is not null) query = query.Where(d => d.Status == status);
        if (type is not null) query = query.Where(d => d.Type == type);

        var items = await query
            .OrderByDescending(d => d.CreatedAtUtc)
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DemandDetailsDto>> GetById(Guid id)
    {
        var d = await _db.DemandRequests
            .Include(x => x.Assessment)
            .Include(x => x.Approval)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d is null) return NotFound();

        var dto = new DemandDetailsDto(
            d.Id,
            d.Title,
            d.ProblemStatement,
            d.Type,
            d.Status,
            d.BusinessUnit,
            d.RequestedBy,
            d.Urgency,
            d.EstimatedEffort,
            d.Assessment?.WeightedScore,
            d.Assessment is null ? null : new AssessmentDto(
                d.Assessment.Id,
                d.Assessment.DemandRequestId,
                d.Assessment.BusinessValue,
                d.Assessment.CostImpact,
                d.Assessment.Risk,
                d.Assessment.ResourceNeed,
                d.Assessment.StrategicAlignment,
                d.Assessment.WeightedScore,
                d.Assessment.AssessedBy,
                d.Assessment.AssessedAtUtc
            ),
            d.Approval is null ? null : new ApprovalDto(
                d.Approval.Id,
                d.Approval.DemandRequestId,
                d.Approval.Status,
                d.Approval.DecisionBy,
                d.Approval.Comments,
                d.Approval.DecidedAtUtc
            ),
            d.CreatedAtUtc
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateDemandRequestDto dto)
    {
        var demand = new DemandRequest
        {
            Title = dto.Title,
            ProblemStatement = dto.ProblemStatement,
            Type = dto.Type,
            BusinessUnit = dto.BusinessUnit,
            RequestedBy = dto.RequestedBy,
            Urgency = dto.Urgency,
            EstimatedEffort = dto.EstimatedEffort,
            Status = DemandStatus.Intake
        };

        _db.DemandRequests.Add(demand);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = demand.Id }, new { demand.Id });
    }
}
