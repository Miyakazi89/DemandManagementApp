using DemandManagement2.Api.Dtos;
using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[ApiController]
[Route("api/demands/{demandId:guid}/approval")]
public class ApprovalsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ApprovalsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApprovalDto>> Get(Guid demandId)
    {
        var approval = await _db.ApprovalDecisions.FirstOrDefaultAsync(a => a.DemandRequestId == demandId);
        if (approval is null) return NotFound();

        return Ok(new ApprovalDto(
            approval.Id,
            approval.DemandRequestId,
            approval.Status,
            approval.DecisionBy,
            approval.Comments,
            approval.DecidedAtUtc
        ));
    }

    [HttpPost]
    public async Task<ActionResult> Decide(Guid demandId, CreateApprovalDto dto)
    {
        var demand = await _db.DemandRequests
            .Include(d => d.Assessment)
            .Include(d => d.Approval)
            .FirstOrDefaultAsync(d => d.Id == demandId);

        if (demand is null) return NotFound("Demand not found.");

        // Rule: don't approve without an assessment score
        if (demand.Assessment is null)
            return BadRequest("Cannot approve/reject without an assessment.");

        var approval = demand.Approval ?? new ApprovalDecision { DemandRequestId = demandId };

        approval.Status = dto.Status;
        approval.DecisionBy = dto.DecisionBy;
        approval.Comments = dto.Comments ?? string.Empty;
        approval.DecidedAtUtc = DateTime.UtcNow;

        if (demand.Approval is null)
            _db.ApprovalDecisions.Add(approval);

        // Sync demand status
        demand.Status = dto.Status switch
        {
            ApprovalStatus.Approved => DemandStatus.Approved,
            ApprovalStatus.Rejected => DemandStatus.Rejected,
            ApprovalStatus.OnHold => DemandStatus.Backlog,
            _ => demand.Status
        };

        await _db.SaveChangesAsync();
        return NoContent();
    }
}