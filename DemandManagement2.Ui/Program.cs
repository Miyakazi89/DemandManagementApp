using System.Net.Http.Headers;
using DemandManagement2.Ui.Components;
using DemandManagement2.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Read API Base URL from appsettings.json
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5182";

// Named HttpClient for your API
builder.Services.AddHttpClient("DemandApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5182/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Your wrapper service
builder.Services.AddScoped<DemandApiClient>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();