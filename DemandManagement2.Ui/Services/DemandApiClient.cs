using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemandManagement2.Ui.Services;

public class DemandApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DemandApiClient(IHttpClientFactory factory, TokenStorageService tokenStorage)
    {
        _http = factory.CreateClient("DemandApi");
        _tokenStorage = tokenStorage;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // -----------------------------
    // Demands
    // -----------------------------
    public async Task<List<DemandListItem>> GetDemands()
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<DemandListItem>>("/api/demands", JsonOptions) ?? new();
    }

    public async Task<DemandDetails?> GetDemand(Guid id)
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<DemandDetails>($"/api/demands/{id}", JsonOptions);
    }

    public async Task<Guid> CreateDemand(CreateDemandRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("/api/demands", request, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"POST /api/demands failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<CreateDemandResponse>(JsonOptions);
        return result?.Id ?? Guid.Empty;
    }

    // -----------------------------
    // Attachments
    // -----------------------------
    public async Task UploadAttachment(Guid demandId, Stream fileStream, string fileName, string contentType)
    {
        await AttachTokenAsync();
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync($"/api/demands/{demandId}/attachments", content);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Upload failed: {(int)response.StatusCode}: {body}");
        }
    }

    public async Task DeleteAttachment(Guid demandId, Guid attachmentId)
    {
        await AttachTokenAsync();
        var response = await _http.DeleteAsync($"/api/demands/{demandId}/attachments/{attachmentId}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    public async Task UpdateDemand(Guid id, CreateDemandRequest request)
    {
        await AttachTokenAsync();
        var response = await _http.PutAsJsonAsync($"/api/demands/{id}", request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    public async Task RequestInfo(Guid id, string message = "")
    {
        await AttachTokenAsync();
        var payload = new { Message = message };
        var response = await _http.PatchAsJsonAsync($"/api/demands/{id}/request-info", payload, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    public async Task DeleteDemand(Guid id)
    {
        await AttachTokenAsync();
        var response = await _http.DeleteAsync($"/api/demands/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    // -----------------------------
    // Assessment + Approval
    // -----------------------------
    public async Task SaveAssessment(Guid demandId, CreateOrUpdateAssessment request)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync($"/api/demands/{demandId}/assessment", request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    public async Task SaveApproval(Guid demandId, CreateApproval request)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync($"/api/demands/{demandId}/approval", request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"{(int)response.StatusCode}: {body}");
        }
    }

    // -----------------------------
    // Dashboard + Prioritization
    // -----------------------------
    public async Task<object?> DashboardSummary()
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<object>("/api/dashboard/summary", JsonOptions);
    }

    public async Task<List<DemandListItem>> Prioritization()
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<DemandListItem>>("/api/prioritization", JsonOptions) ?? new();
    }

    // -----------------------------
    // Resource & Capacity
    // -----------------------------
    public async Task<List<ResourceItem>> GetResources()
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<ResourceItem>>("/api/resources", JsonOptions) ?? new();
    }

    public async Task CreateResource(CreateResourceRequest request)
    {
        await AttachTokenAsync();
        (await _http.PostAsJsonAsync("/api/resources", request, JsonOptions)).EnsureSuccessStatusCode();
    }

    public async Task<CapacitySummary?> GetCapacitySummary(int? month = null, int? year = null)
    {
        await AttachTokenAsync();
        var url = "/api/capacity/summary";
        if (month.HasValue && year.HasValue)
            url += $"?month={month}&year={year}";

        return await _http.GetFromJsonAsync<CapacitySummary>(url, JsonOptions);
    }

    public async Task<List<ForecastItem>> GetCapacityForecast()
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<ForecastItem>>("/api/capacity/forecast", JsonOptions) ?? new();
    }

    public async Task<List<AllocationItem>> GetAllocations(int? month = null, int? year = null)
    {
        await AttachTokenAsync();
        var url = "/api/capacity/allocations";
        if (month.HasValue && year.HasValue)
            url += $"?month={month}&year={year}";

        return await _http.GetFromJsonAsync<List<AllocationItem>>(url, JsonOptions) ?? new();
    }

    public async Task CreateAllocation(CreateAllocationRequest request)
    {
        await AttachTokenAsync();
        (await _http.PostAsJsonAsync("/api/capacity/allocations", request, JsonOptions)).EnsureSuccessStatusCode();
    }

    // -----------------------------
    // Decision Notes
    // -----------------------------
    public async Task<List<DecisionNoteItem>> GetDecisionNotes(Guid demandId)
    {
        await AttachTokenAsync();
        return await _http.GetFromJsonAsync<List<DecisionNoteItem>>($"/api/demands/{demandId}/notes", JsonOptions) ?? new();
    }

    public async Task CreateDecisionNote(Guid demandId, CreateDecisionNoteRequest request)
    {
        await AttachTokenAsync();
        (await _http.PostAsJsonAsync($"/api/demands/{demandId}/notes", request, JsonOptions)).EnsureSuccessStatusCode();
    }
}

// =====================================================================
// UI MODELS (strongly typed so you can show NPV + assessment fields)
// =====================================================================

public record DemandListItem(
    Guid Id,
    string Title,
    string Type,
    string Status,
    string BusinessUnit,
    string RequestedBy,
    int Urgency,
    decimal EstimatedEffort,
    decimal? WeightedScore,
    DateTime CreatedAtUtc,
    DateTime? TargetDate
);

public record DemandDetails(
    Guid Id,
    string Title,
    string ProblemStatement,
    string Type,
    string Status,
    string BusinessUnit,
    string RequestedBy,
    int Urgency,
    decimal EstimatedEffort,
    decimal? WeightedScore,
    AssessmentDetails? Assessment,
    ApprovalDetails? Approval,
    DateTime CreatedAtUtc,
    DateTime? TargetDate,
    List<DemandEventItem>? Events,
    List<DemandAttachmentItem>? Attachments
);

// Matches API AssessmentDto (including NPV)
public record AssessmentDetails(
    Guid Id,
    Guid DemandRequestId,
    int BusinessValue,
    int CostImpact,
    int Risk,
    int ResourceNeed,
    int StrategicAlignment,
    decimal WeightedScore,

    decimal InitialCost,
    decimal AnnualBenefit,
    int ProjectYears,
    decimal DiscountRate,
    decimal CalculatedNPV,

    string AssessedBy,
    DateTime AssessedAtUtc
);

// Matches API ApprovalDto
public record ApprovalDetails(
    Guid Id,
    Guid DemandRequestId,
    ApprovalStatus Status,
    string DecisionBy,
    string Comments,
    DateTime DecidedAtUtc
);

public class CreateDemandRequest
{
    public string Title { get; set; } = "";
    public string ProblemStatement { get; set; } = "";
    public DemandType Type { get; set; } = DemandType.Project;
    public string BusinessUnit { get; set; } = "";
    public string RequestedBy { get; set; } = "";
    public int Urgency { get; set; } = 3;
    public int EstimatedEffort { get; set; } = 3;
    public DateTime? TargetDate { get; set; }
}

public record CreateDemandResponse(Guid Id);

public record DemandEventItem(
    Guid Id,
    string EventType,
    string Description,
    string PerformedBy,
    DateTime OccurredAtUtc
);

public record DemandAttachmentItem(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string UploadedBy,
    DateTime UploadedAtUtc,
    string DownloadUrl
);

public enum DemandType
{
    Project,
    Enhancement,
    Service,
    ResourceRequest
}

// ✅ UPDATED: matches API CreateOrUpdateAssessmentDto (includes NPV inputs)
public class CreateOrUpdateAssessment
{
    public int BusinessValue { get; set; } = 3;
    public int CostImpact { get; set; } = 3;
    public int Risk { get; set; } = 3;
    public int ResourceNeed { get; set; } = 3;
    public int StrategicAlignment { get; set; } = 3;

    // NPV inputs (API expects DiscountRate in %)
    public decimal InitialCost { get; set; } = 0m;
    public decimal AnnualBenefit { get; set; } = 0m;
    public int ProjectYears { get; set; } = 3;
    public decimal DiscountRate { get; set; } = 10m;

    public string AssessedBy { get; set; } = "";
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    OnHold,
    Rejected
}

public class CreateApproval
{
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Approved;
    public string DecisionBy { get; set; } = "";
    public string Comments { get; set; } = "";
}

// -----------------------------
// Resource & Capacity Models
// -----------------------------
public record ResourceItem(
    Guid Id,
    string Name,
    string Role,
    string Department,
    int CapacityHoursPerMonth,
    bool IsActive,
    DateTime CreatedAtUtc
);

public class CreateResourceRequest
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Department { get; set; } = "";
    public int CapacityHoursPerMonth { get; set; } = 160;
}

public record CapacitySummary(
    int Month,
    int Year,
    int TotalResources,
    int TotalCapacityHours,
    int AllocatedHours,
    int AvailableHours,
    decimal UtilizationPercent,
    List<DepartmentCapacity> ByDepartment
);

public record DepartmentCapacity(
    string Department,
    int CapacityHours,
    int AllocatedHours
);

public record ForecastItem(
    int Month,
    int Year,
    string Label,
    int CapacityHours,
    int AllocatedHours,
    int AvailableHours
);

public record AllocationItem(
    Guid Id,
    Guid ResourceId,
    string ResourceName,
    Guid DemandRequestId,
    string DemandTitle,
    int AllocatedHours,
    int Month,
    int Year,
    string Notes
);

public class CreateAllocationRequest
{
    public Guid ResourceId { get; set; }
    public Guid DemandRequestId { get; set; }
    public int AllocatedHours { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

// -----------------------------
// Decision Notes Models
// -----------------------------
public record DecisionNoteItem(
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

public class CreateDecisionNoteRequest
{
    public string MeetingDate { get; set; } = "";
    public string Attendees { get; set; } = "";
    public string Discussion { get; set; } = "";
    public string Decision { get; set; } = "";
    public string ActionItems { get; set; } = "";
    public string RecordedBy { get; set; } = "";
}