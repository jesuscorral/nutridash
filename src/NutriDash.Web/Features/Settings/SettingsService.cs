using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using NutriDash.Infrastructure.Data.Entities;

namespace NutriDash.Features.Settings;

public class SettingsService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(AppDbContext db, ILogger<SettingsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<UserMetric> GetUserMetricAsync()
        => await _db.UserMetrics.FirstOrDefaultAsync() ?? new UserMetric();

    public async Task SaveUserMetricAsync(UserMetric metric)
    {
        var existing = await _db.UserMetrics.FirstOrDefaultAsync();
        if (existing == null)
        {
            metric.UpdatedAt = DateTime.UtcNow;
            _db.UserMetrics.Add(metric);
        }
        else
        {
            existing.Name = metric.Name;
            existing.Age = metric.Age;
            existing.Height = metric.Height;
            existing.GoalWeight = metric.GoalWeight;
            existing.TargetSystolicBP = metric.TargetSystolicBP;
            existing.TargetDiastolicBP = metric.TargetDiastolicBP;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        _logger.LogInformation("User metrics saved for '{Name}'", metric.Name);
    }

    public async Task<string> GetGeminiKeyAsync()
    {
        var s = await _db.AppSettings.FirstOrDefaultAsync(x => x.Key == "GeminiApiKey");
        return s?.Value ?? "";
    }

    public async Task SaveGeminiKeyAsync(string key)
    {
        var s = await _db.AppSettings.FirstOrDefaultAsync(x => x.Key == "GeminiApiKey");
        if (s == null)
            _db.AppSettings.Add(new AppSetting { Key = "GeminiApiKey", Value = key, UpdatedAt = DateTime.UtcNow });
        else
        {
            s.Value = key;
            s.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        _logger.LogInformation("Gemini API key updated");
    }
}
