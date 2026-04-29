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

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=/app/data/nutridash.db";

// Ensure directory exists (for Docker volume)
var dbPath = connStr.Replace("Data Source=", "").Trim();
var dbDir = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
    Directory.CreateDirectory(dbDir);

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlite(connStr));

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

// ── DB init + seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
