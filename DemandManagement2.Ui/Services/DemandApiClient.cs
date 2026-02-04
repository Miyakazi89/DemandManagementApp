using System.Net.Http.Json;

namespace DemandManagement2.Ui.Services;

public class DemandApiClient
{
    private readonly HttpClient _http;

    public DemandApiClient(IHttpClientFactory factory)
        => _http = factory.CreateClient("DemandApi");

    public async Task<List<DemandListItem>> GetDemands()
        => await _http.GetFromJsonAsync<List<DemandListItem>>("http://localhost:5182/api/demands") ?? new();

    public async Task<DemandDetails?> GetDemand(Guid id)
        => await _http.GetFromJsonAsync<DemandDetails>($"/api/demands/{id}");

    public async Task CreateDemand(CreateDemandRequest request)
        => (await _http.PostAsJsonAsync("/api/demands", request)).EnsureSuccessStatusCode();

    public async Task SaveAssessment(Guid demandId, CreateOrUpdateAssessment request)
        => (await _http.PostAsJsonAsync($"/api/demands/{demandId}/assessment", request)).EnsureSuccessStatusCode();

    public async Task SaveApproval(Guid demandId, CreateApproval request)
        => (await _http.PostAsJsonAsync($"/api/demands/{demandId}/approval", request)).EnsureSuccessStatusCode();

    public async Task<object?> DashboardSummary()
        => await _http.GetFromJsonAsync<object>("/api/dashboard/summary");

   
public async Task<List<DemandListItem>> Prioritization()
    => await _http.GetFromJsonAsync<List<DemandListItem>>("api/prioritization") ?? new();

}

// Simple UI models (keep lightweight)
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
    DateTime CreatedAtUtc
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
    object? Assessment,
    object? Approval,
    DateTime CreatedAtUtc
);

public class CreateDemandRequest
{
    public string Title { get; set; } = "";
    public string ProblemStatement { get; set; } = "";
    public string Type { get; set; } = "Project";
    public string BusinessUnit { get; set; } = "";
    public string RequestedBy { get; set; } = "";
    public int Urgency { get; set; } = 3;
    public decimal EstimatedEffort { get; set; } = 3;
}

public class CreateOrUpdateAssessment
{
    public int BusinessValue { get; set; } = 3;
    public int CostImpact { get; set; } = 3;
    public int Risk { get; set; } = 3;
    public int ResourceNeed { get; set; } = 3;
    public int StrategicAlignment { get; set; } = 3;
    public string AssessedBy { get; set; } = "";
}

public class CreateApproval
{
    public string Status { get; set; } = "Approved";
    public string DecisionBy { get; set; } = "";
    public string Comments { get; set; } = "";
}
