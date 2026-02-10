using DemandManagement2.Domain.Entities;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandManagement2.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/demands/{demandId:guid}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly string _uploadPath;
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".txt", ".csv", ".zip" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public AttachmentsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _uploadPath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    [HttpPost]
    public async Task<ActionResult> Upload(Guid demandId, IFormFile file)
    {
        var demand = await _db.DemandRequests.FindAsync(demandId);
        if (demand is null) return NotFound("Demand not found.");

        if (file.Length == 0) return BadRequest("File is empty.");
        if (file.Length > MaxFileSize) return BadRequest("File exceeds 10 MB limit.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest($"File type '{ext}' is not allowed.");

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(_uploadPath, storedName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var userName = User.Identity?.Name ?? "System";

        var attachment = new DemandAttachment
        {
            DemandRequestId = demandId,
            FileName = file.FileName,
            StoredFileName = storedName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            UploadedBy = userName
        };

        _db.DemandAttachments.Add(attachment);

        _db.DemandEvents.Add(new DemandEvent
        {
            DemandRequestId = demandId,
            EventType = "FileUploaded",
            Description = $"File '{file.FileName}' uploaded by {userName}",
            PerformedBy = userName
        });

        await _db.SaveChangesAsync();

        return Ok(new { attachment.Id, attachment.FileName });
    }

    [AllowAnonymous]
    [HttpGet("{attachmentId:guid}")]
    public async Task<ActionResult> Download(Guid demandId, Guid attachmentId)
    {
        var attachment = await _db.DemandAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.DemandRequestId == demandId);

        if (attachment is null) return NotFound();

        var filePath = Path.Combine(_uploadPath, attachment.StoredFileName);
        if (!System.IO.File.Exists(filePath)) return NotFound("File not found on server.");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, attachment.ContentType, attachment.FileName);
    }

    [Authorize(Roles = "Admin,Assessor")]
    [HttpDelete("{attachmentId:guid}")]
    public async Task<ActionResult> Delete(Guid demandId, Guid attachmentId)
    {
        var attachment = await _db.DemandAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.DemandRequestId == demandId);

        if (attachment is null) return NotFound();

        var filePath = Path.Combine(_uploadPath, attachment.StoredFileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _db.DemandAttachments.Remove(attachment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
