using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DemandManagement2.Ui.Services;

public class TokenStorageService
{
    private readonly ProtectedSessionStorage _storage;
    private const string TokenKey = "authToken";
    private const string UserKey = "authUser";

    public TokenStorageService(ProtectedSessionStorage storage)
        => _storage = storage;

    public async Task SetTokenAsync(string token)
        => await _storage.SetAsync(TokenKey, token);

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _storage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            // Called during prerendering — no JS interop available
            return null;
        }
    }

    public async Task SetUserAsync(AuthResponse user)
        => await _storage.SetAsync(UserKey, user);

    public async Task<AuthResponse?> GetUserAsync()
    {
        try
        {
            var result = await _storage.GetAsync<AuthResponse>(UserKey);
            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _storage.DeleteAsync(TokenKey);
            await _storage.DeleteAsync(UserKey);
        }
        catch (InvalidOperationException)
        {
            // Prerendering — ignore
        }
    }
}
