using GearHawk.Web.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Identity.Abstractions;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Aspire ServiceDefaults for discovery/OTel/health/resilience
builder.AddServiceDefaults();

// Add services to the container.
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// Ensure the named HttpClient used by DownstreamApi has Aspire service discovery
builder.Services.AddHttpClient("GearHawkApi").AddServiceDiscovery();

builder.Services.AddDownstreamApi("GearHawkApi", options =>
{
    options.BaseUrl = "https://gearhawk-api";
    options.Scopes = builder.Configuration.GetSection("Api:Scopes").Get<string[]>() ?? Array.Empty<string>();
});

builder.Services.AddAuthorization();

builder.Services
    .AddControllersWithViews();

builder.Services
    .AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Radzen services
builder.Services.AddRadzenComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();
app.MapControllers();

// Aspire health endpoints in Development
app.MapDefaultEndpoints();

app.Run();
