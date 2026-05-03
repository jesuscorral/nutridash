using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using NutriDash.Infrastructure.Services;
using NutriDash.Features.Dashboard;
using NutriDash.Features.DailyTracking;
using NutriDash.Features.Analytics;
using NutriDash.Features.MealPlan;
using NutriDash.Features.ShoppingList;
using NutriDash.Features.Supplements;
using NutriDash.Features.Settings;
using NutriDash.ServiceDefaults;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ── Service Defaults (Aspire) ─────────────────────────────────────────────────
builder.AddServiceDefaults();

// ── Data Protection (persist keys so antiforgery tokens survive restarts) ────
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dataprotection-keys"));

// ── Database ─────────────────────────────────────────────────────────────────
builder.AddNpgsqlDbContext<AppDbContext>("DefaultConnection");

// ── HTTP Client ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("Gemini", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// ── Infrastructure Services ───────────────────────────────────────────────────
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddSingleton<HealthScoringService>();
builder.Services.AddSingleton<SupplementRecommendationService>();
builder.Services.AddSingleton<AdaptiveNutritionService>();

// ── Feature Services (Scoped — per request/circuit) ───────────────────────────
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<TrackingService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<MealPlanService>();
builder.Services.AddScoped<ShoppingListService>();
builder.Services.AddScoped<SupplementsService>();
builder.Services.AddScoped<SettingsService>();

// ── Blazor ────────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ── Service Defaults Endpoints (Aspire) ───────────────────────────────────────
app.MapDefaultEndpoints();

// ── DB init + seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

// ── Pipeline ──────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<NutriDash.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
