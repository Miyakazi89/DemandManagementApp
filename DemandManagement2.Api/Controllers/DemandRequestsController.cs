using DemandManagement2.Api.Dtos;
using DemandManagement2.Api.Services;
using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/demands")]
public class DemandsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    public DemandsController(AppDbContext db, IEmailService email) { _db = db; _email = email; }

    [HttpGet]
    public async Task<ActionResult<List<DemandListItemDto>>> GetAll(
        [FromQuery] DemandStatus? status = null,
        [FromQuery] DemandType? type = null)
    {
        var query = _db.DemandRequests
            .Include(d => d.Assessment)
            .AsQueryable();

        // Requesters can only see their own demands
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "Requester")
        {
            var userName = User.Identity?.Name ?? "";
            query = query.Where(d => d.RequestedBy.ToLower() == userName.ToLower());
        }

        if (status is not null) query = query.Where(d => d.Status == status);
        if (type is not null) query = query.Where(d => d.Type == type);

        var items = await query
            .OrderByDescending(d => d.CreatedAtUtc)
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DemandDetailsDto>> GetById(Guid id)
    {
        var d = await _db.DemandRequests
            .Include(x => x.Assessment)
            .Include(x => x.Approval)
            .Include(x => x.Events)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d is null) return NotFound();

        // Requesters can only view their own demands
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "Requester")
        {
            var userName = User.Identity?.Name ?? "";
            if (!string.Equals(d.RequestedBy, userName, StringComparison.OrdinalIgnoreCase))
                return Forbid();
        }

        var a = d.Assessment;

        var events = d.Events
            .OrderByDescending(e => e.OccurredAtUtc)
            .Select(e => new DemandEventDto(e.Id, e.EventType, e.Description, e.PerformedBy, e.OccurredAtUtc))
            .ToList();

        var attachments = d.Attachments
            .OrderByDescending(att => att.UploadedAtUtc)
            .Select(att => new DemandAttachmentDto(
                att.Id, att.FileName, att.ContentType, att.FileSizeBytes,
                att.UploadedBy, att.UploadedAtUtc,
                $"/api/demands/{d.Id}/attachments/{att.Id}"))
            .ToList();

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
            a?.WeightedScore,
            a is null ? null : new AssessmentDto(
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
            ),
            d.Approval is null ? null : new ApprovalDto(
                d.Approval.Id,
                d.Approval.DemandRequestId,
                d.Approval.Status,
                d.Approval.DecisionBy,
                d.Approval.Comments,
                d.Approval.DecidedAtUtc
            ),
            d.CreatedAtUtc,
            d.TargetDate,
            events,
            attachments
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateDemandRequestDto dto)
    {
        var userName = User.Identity?.Name ?? "System";

        var demand = new DemandRequest
        {
            Title = dto.Title,
            ProblemStatement = dto.ProblemStatement,
            Type = dto.Type,
            BusinessUnit = dto.BusinessUnit,
            RequestedBy = dto.RequestedBy,
            Urgency = dto.Urgency,
            EstimatedEffort = dto.EstimatedEffort,
            TargetDate = dto.TargetDate,
            Status = DemandStatus.Intake
        };

        _db.DemandRequests.Add(demand);

        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demand.Id,
            EventType = "Created",
            Description = $"Demand request '{demand.Title}' was created",
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();

        // Notify assessors/admins about new demand
        var assessors = await _db.Users
            .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Assessor)
            .Select(u => u.Email)
            .ToListAsync();
        foreach (var email in assessors)
            await _email.SendDemandNotificationAsync("Created", demand.Id, demand.Title, email);

        return CreatedAtAction(nameof(GetById), new { id = demand.Id }, new { demand.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, CreateDemandRequestDto dto)
    {
        var demand = await _db.DemandRequests.FindAsync(id);
        if (demand is null) return NotFound();

        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userName = User.Identity?.Name ?? "System";

        // Requesters can only update their own demands when status is Intake or NeedsInfo
        if (role == "Requester")
        {
            if (demand.Status != DemandStatus.Intake && demand.Status != DemandStatus.NeedsInfo)
                return Forbid("You can only edit demands that are in Intake or awaiting additional info.");
        }

        demand.Title = dto.Title;
        demand.ProblemStatement = dto.ProblemStatement;
        demand.Type = dto.Type;
        demand.BusinessUnit = dto.BusinessUnit;
        demand.RequestedBy = dto.RequestedBy;
        demand.Urgency = dto.Urgency;
        demand.EstimatedEffort = dto.EstimatedEffort;
        demand.TargetDate = dto.TargetDate;

        // When requester responds, move back to UnderReview
        if (role == "Requester" && demand.Status == DemandStatus.NeedsInfo)
            demand.Status = DemandStatus.UnderReview;

        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demand.Id,
            EventType = "Updated",
            Description = $"Demand request was updated by {userName}",
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpPatch("{id:guid}/request-info")]
    public async Task<ActionResult> RequestInfo(Guid id, [FromBody] RequestInfoDto? dto = null)
    {
        var demand = await _db.DemandRequests.FindAsync(id);
        if (demand is null) return NotFound();

        var userName = User.Identity?.Name ?? "System";
        var message = dto?.Message ?? "";

        demand.Status = DemandStatus.NeedsInfo;

        var description = string.IsNullOrWhiteSpace(message)
            ? $"Additional information requested by {userName}"
            : $"{userName}: {message}";

        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demand.Id,
            EventType = "InfoRequested",
            Description = description,
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();

        // Notify requester about info request
        var requester = await _db.Users.FirstOrDefaultAsync(u => u.FullName == demand.RequestedBy);
        await _email.SendDemandNotificationAsync("InfoRequested", demand.Id, demand.Title, requester?.Email);

        return NoContent();
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var demand = await _db.DemandRequests
            .Include(d => d.Assessment)
            .Include(d => d.Approval)
            .Include(d => d.Events)
            .Include(d => d.Attachments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (demand is null) return NotFound();

        // Remove related records
        if (demand.Approval is not null) _db.ApprovalDecisions.Remove(demand.Approval);
        if (demand.Assessment is not null) _db.Assessments.Remove(demand.Assessment);

        // Remove decision notes
        var notes = await _db.DecisionNotes.Where(n => n.DemandRequestId == id).ToListAsync();
        _db.DecisionNotes.RemoveRange(notes);

        // Remove events and attachments
        _db.DemandEvents.RemoveRange(demand.Events);

        // Delete attachment files from disk
        foreach (var att in demand.Attachments)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", att.StoredFileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
        _db.DemandAttachments.RemoveRange(demand.Attachments);

        _db.DemandRequests.Remove(demand);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
