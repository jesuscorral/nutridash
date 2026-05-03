using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using Entities = NutriDash.Infrastructure.Data.Entities;

namespace NutriDash.Features.DailyTracking;

public class TrackingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TrackingService> _logger;

    public TrackingService(AppDbContext db, ILogger<TrackingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Entities.DailyTracking>> GetRecentAsync(int days = 14)
        => await _db.DailyTrackings
            .OrderByDescending(t => t.Date)
            .Take(days).ToListAsync();

    public async Task<Entities.DailyTracking?> GetTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.DailyTrackings.FirstOrDefaultAsync(t => t.Date == today);
    }

    public async Task<(bool Success, string? Error)> SaveAsync(Entities.DailyTracking entry)
    {
        try
        {
            var existing = await _db.DailyTrackings.FirstOrDefaultAsync(t => t.Date == entry.Date);
            if (existing != null)
            {
                existing.Weight = entry.Weight;
                existing.SystolicBP = entry.SystolicBP;
                existing.DiastolicBP = entry.DiastolicBP;
                existing.Steps = entry.Steps;
                existing.WorkoutType = entry.WorkoutType;
                existing.Sleep = entry.Sleep;
                existing.Energy = entry.Energy;
                existing.Adherence = entry.Adherence;
                existing.Notes = entry.Notes;
                _logger.LogInformation("Tracking entry updated for {Date}", entry.Date);
            }
            else
            {
                _db.DailyTrackings.Add(entry);
                _logger.LogInformation("Tracking entry created for {Date}", entry.Date);
            }
            await _db.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save tracking entry for {Date}", entry.Date);
            return (false, ex.Message);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _db.DailyTrackings.FindAsync(id);
        if (entry == null) return false;
        _db.DailyTrackings.Remove(entry);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Tracking entry {Id} ({Date}) deleted", id, entry.Date);
        return true;
    }
}
