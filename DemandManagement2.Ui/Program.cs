using System.Net.Http.Headers;
using DemandManagement2.Ui.Components;
using DemandManagement2.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ✅ Read from appsettings.json
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5182";

builder.Services.AddHttpClient("DemandApi", client =>
{
    client.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<DemandApiClient>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
