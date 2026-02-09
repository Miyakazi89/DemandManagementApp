namespace DemandManagement2.Domain.Entities;

public class DemandAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DemandRequestId { get; set; }
    public DemandRequest DemandRequest { get; set; } = default!;

    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
