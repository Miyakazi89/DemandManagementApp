using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemandManagement2.Ui.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AuthService(
        IHttpClientFactory factory,
        TokenStorageService tokenStorage,
        JwtAuthenticationStateProvider authStateProvider)
    {
        _http = factory.CreateClient("DemandApi");
        _tokenStorage = tokenStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        await _tokenStorage.SetTokenAsync(auth!.Token);
        await _tokenStorage.SetUserAsync(auth);
        _authStateProvider.NotifyAuthStateChanged();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/register", new
        {
            request.FullName,
            request.Email,
            request.Password,
            request.Role
        }, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        await _tokenStorage.SetTokenAsync(auth!.Token);
        await _tokenStorage.SetUserAsync(auth);
        _authStateProvider.NotifyAuthStateChanged();
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await _tokenStorage.ClearAsync();
        _authStateProvider.NotifyAuthStateChanged();
    }
}
