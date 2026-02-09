using System.Net.Http.Headers;
using DemandManagement2.Ui.Components;
using DemandManagement2.Ui.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Read from appsettings.json
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5182";

builder.Services.AddHttpClient("DemandApi", client =>
{
    client.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Auth services
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthorizationCore();

// API client
builder.Services.AddScoped<DemandApiClient>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
