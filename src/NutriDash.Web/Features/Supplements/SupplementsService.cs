using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using NutriDash.Infrastructure.Data.Entities;
using NutriDash.Infrastructure.Services;

namespace NutriDash.Features.Supplements;

public class SupplementsService
{
    private readonly AppDbContext _db;
    private readonly SupplementRecommendationService _recommender;

    public SupplementsService(AppDbContext db, SupplementRecommendationService recommender)
    {
        _db = db;
        _recommender = recommender;
    }

    public async Task<List<SupplementRecommendation>> GetRecommendationsAsync()
    {
        var user = await _db.UserMetrics.FirstOrDefaultAsync() ?? new UserMetric();
        var recent = await _db.DailyTrackings
            .OrderByDescending(t => t.Date).Take(7).ToListAsync();
        return _recommender.GetRecommendations(recent, user);
    }

    public async Task<List<SupplementLog>> GetLogsAsync(int days = 14)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
        return await _db.SupplementLogs
            .Where(s => s.Date >= cutoff)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task LogTakenAsync(string name, string dose)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var existing = await _db.SupplementLogs
            .FirstOrDefaultAsync(s => s.Date == today && s.Name == name);
        if (existing == null)
        {
            _db.SupplementLogs.Add(new SupplementLog
            {
                Date = today, Name = name, Dose = dose,
                Reason = "Log manual", Taken = true
            });
            await _db.SaveChangesAsync();
        }
    }
}
