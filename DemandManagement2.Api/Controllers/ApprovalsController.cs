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
[Route("api/demands/{demandId:guid}/approval")]
public class ApprovalsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    public ApprovalsController(AppDbContext db, IEmailService email) { _db = db; _email = email; }

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

        // Record timeline event
        var userName = User.Identity?.Name ?? dto.DecisionBy;
        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demandId,
            EventType = dto.Status.ToString(),
            Description = $"Demand {dto.Status.ToString().ToLower()} by {userName}",
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();

        // Notify requester about approval decision
        var requester = await _db.Users.FirstOrDefaultAsync(u => u.FullName == demand.RequestedBy);
        await _email.SendDemandNotificationAsync(dto.Status.ToString(), demand.Id, demand.Title, requester?.Email);

        return NoContent();
    }
}