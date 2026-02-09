using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/demands/{demandId:guid}/notes")]
public class DecisionNotesController : ControllerBase
{
    private readonly AppDbContext _db;
    public DecisionNotesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DecisionNoteDto>>> GetAll(Guid demandId)
    {
        var notes = await _db.DecisionNotes
            .Where(n => n.DemandRequestId == demandId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new DecisionNoteDto(
                n.Id,
                n.DemandRequestId,
                n.MeetingDate,
                n.Attendees,
                n.Discussion,
                n.Decision,
                n.ActionItems,
                n.RecordedBy,
                n.CreatedAtUtc
            ))
            .ToListAsync();

        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DecisionNoteDto>> GetById(Guid demandId, Guid id)
    {
        var n = await _db.DecisionNotes
            .FirstOrDefaultAsync(x => x.Id == id && x.DemandRequestId == demandId);

        if (n is null) return NotFound();

        return Ok(new DecisionNoteDto(
            n.Id, n.DemandRequestId, n.MeetingDate, n.Attendees,
            n.Discussion, n.Decision, n.ActionItems, n.RecordedBy, n.CreatedAtUtc
        ));
    }

    [HttpPost]
    public async Task<ActionResult> Create(Guid demandId, CreateDecisionNoteDto dto)
    {
        var demand = await _db.DemandRequests.FindAsync(demandId);
        if (demand is null) return NotFound("Demand not found.");

        var note = new DecisionNote
        {
            DemandRequestId = demandId,
            MeetingDate = dto.MeetingDate,
            Attendees = dto.Attendees,
            Discussion = dto.Discussion,
            Decision = dto.Decision,
            ActionItems = dto.ActionItems ?? string.Empty,
            RecordedBy = dto.RecordedBy
        };

        _db.DecisionNotes.Add(note);

        // Record timeline event
        var userName = User.Identity?.Name ?? dto.RecordedBy;
        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demandId,
            EventType = "NoteAdded",
            Description = $"Meeting note added by {userName}",
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();

        return Ok(new { note.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid demandId, Guid id, CreateDecisionNoteDto dto)
    {
        var note = await _db.DecisionNotes
            .FirstOrDefaultAsync(x => x.Id == id && x.DemandRequestId == demandId);

        if (note is null) return NotFound();

        note.MeetingDate = dto.MeetingDate;
        note.Attendees = dto.Attendees;
        note.Discussion = dto.Discussion;
        note.Decision = dto.Decision;
        note.ActionItems = dto.ActionItems ?? string.Empty;
        note.RecordedBy = dto.RecordedBy;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid demandId, Guid id)
    {
        var note = await _db.DecisionNotes
            .FirstOrDefaultAsync(x => x.Id == id && x.DemandRequestId == demandId);

        if (note is null) return NotFound();

        _db.DecisionNotes.Remove(note);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// DTOs
public record DecisionNoteDto(
    Guid Id,
    Guid DemandRequestId,
    string MeetingDate,
    string Attendees,
    string Discussion,
    string Decision,
    string ActionItems,
    string RecordedBy,
    DateTime CreatedAtUtc
);

public class CreateDecisionNoteDto
{
    public string MeetingDate { get; set; } = string.Empty;
    public string Attendees { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string? ActionItems { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}
