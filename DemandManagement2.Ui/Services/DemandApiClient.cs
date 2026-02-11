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
    // Reports & Export
    // -----------------------------
    public async Task<List<ReportRow>> GetReport(ReportFilter filter)
    {
        await AttachTokenAsync();
        var url = $"/api/reports?{filter.ToQueryString()}";
        return await _http.GetFromJsonAsync<List<ReportRow>>(url, JsonOptions) ?? new();
    }

    public async Task<byte[]> ExportReportCsv(ReportFilter filter)
    {
        await AttachTokenAsync();
        return await _http.GetByteArrayAsync($"/api/reports/export/csv?{filter.ToQueryString()}");
    }

    public async Task<byte[]> ExportReportExcel(ReportFilter filter)
    {
        await AttachTokenAsync();
        return await _http.GetByteArrayAsync($"/api/reports/export/excel?{filter.ToQueryString()}");
    }

    public async Task<byte[]> ExportReportPdf(ReportFilter filter)
    {
        await AttachTokenAsync();
        return await _http.GetByteArrayAsync($"/api/reports/export/pdf?{filter.ToQueryString()}");
    }

    // -----------------------------
    // Budget & Finance
    // -----------------------------
    public async Task<BudgetSummary?> GetBudgetSummary(int? year = null)
    {
        await AttachTokenAsync();
        var url = year.HasValue ? $"/api/budget/summary?year={year}" : "/api/budget/summary";
        return await _http.GetFromJsonAsync<BudgetSummary>(url, JsonOptions);
    }

    public async Task<List<BudgetEntryItem>> GetBudgetEntries(Guid? demandId = null, int? year = null)
    {
        await AttachTokenAsync();
        var parts = new List<string>();
        if (demandId.HasValue) parts.Add($"demandId={demandId}");
        if (year.HasValue) parts.Add($"year={year}");
        var url = "/api/budget/entries" + (parts.Count > 0 ? "?" + string.Join("&", parts) : "");
        return await _http.GetFromJsonAsync<List<BudgetEntryItem>>(url, JsonOptions) ?? new();
    }

    public async Task CreateBudgetEntry(CreateBudgetEntry entry)
    {
        await AttachTokenAsync();
        (await _http.PostAsJsonAsync("/api/budget/entries", entry, JsonOptions)).EnsureSuccessStatusCode();
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

// Matches API AssessmentDto (including NPV + CapEx/OpEx)
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

    decimal CapExAmount,
    decimal OpExAmount,

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

    // Budget breakdown
    public decimal CapExAmount { get; set; } = 0m;
    public decimal OpExAmount { get; set; } = 0m;

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

// -----------------------------
// Report Models
// -----------------------------
public class ReportFilter
{
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? BusinessUnit { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string ToQueryString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(Status)) parts.Add($"status={Uri.EscapeDataString(Status)}");
        if (!string.IsNullOrEmpty(Type)) parts.Add($"type={Uri.EscapeDataString(Type)}");
        if (!string.IsNullOrEmpty(BusinessUnit)) parts.Add($"businessUnit={Uri.EscapeDataString(BusinessUnit)}");
        if (FromDate.HasValue) parts.Add($"fromDate={FromDate.Value:yyyy-MM-dd}");
        if (ToDate.HasValue) parts.Add($"toDate={ToDate.Value:yyyy-MM-dd}");
        return string.Join("&", parts);
    }
}

public record ReportRow(
    Guid Id, string Title, string Type, string Status,
    string BusinessUnit, string RequestedBy, int Urgency,
    int EstimatedEffort, decimal? WeightedScore,
    decimal? InitialCost, decimal? AnnualBenefit, decimal? CalculatedNPV,
    string? ApprovalStatus, DateTime CreatedAtUtc, DateTime? TargetDate
);

// -----------------------------
// Budget Models
// -----------------------------
public record BudgetSummary(
    decimal TotalPlanned, decimal TotalActual, decimal Variance,
    decimal TotalCapEx, decimal TotalOpEx, decimal TotalNPV,
    List<MonthlyBudget> MonthlyBreakdown,
    List<DemandBudget> ByDemand
);

public record MonthlyBudget(int Month, int Year, string Label, decimal Planned, decimal Actual, decimal CumulativeBenefit);

public record DemandBudget(
    Guid DemandId, string Title, string Status,
    decimal InitialCost, decimal AnnualBenefit, decimal CalculatedNPV,
    decimal CapEx, decimal OpEx,
    decimal PlannedTotal, decimal ActualTotal
);

public record BudgetEntryItem(
    Guid Id, Guid DemandRequestId, string DemandTitle,
    int Month, int Year,
    decimal PlannedAmount, decimal ActualAmount,
    string Category, string Notes, DateTime CreatedAtUtc
);

public class CreateBudgetEntry
{
    public Guid DemandRequestId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public string Category { get; set; } = "CapEx";
    public string Notes { get; set; } = "";
}