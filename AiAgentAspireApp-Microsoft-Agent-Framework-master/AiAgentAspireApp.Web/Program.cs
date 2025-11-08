using AiAgentAspireApp.Web;
using AiAgentAspireApp.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddOpenAIClient("ollama-chat-client", settings =>
{
    settings.EnableSensitiveTelemetryData = true;
})
   .AddChatClient();

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://gateway"));

builder.Services.AddHttpClient<AgentApiClient>(client => client.BaseAddress = new("http://gateway"));

builder.Services.AddHttpClient<ProductApiClient>(client => client.BaseAddress = new("http://gateway"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
