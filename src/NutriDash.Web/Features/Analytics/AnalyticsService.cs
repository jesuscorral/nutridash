using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using Entities = NutriDash.Infrastructure.Data.Entities;
using NutriDash.Infrastructure.Services;

namespace NutriDash.Features.Analytics;

public record WeeklyStats(
    DateOnly WeekStart,
    decimal AvgWeight,
    decimal WeightDelta,
    double AvgSystolic,
    double AvgDiastolic,
    double AvgEnergy,
    double AvgAdherence,
    int TotalSteps,
    NutriDash.Infrastructure.Services.HealthScoreBreakdown Score
);

public class AnalyticsService
{
    private readonly AppDbContext _db;
    private readonly HealthScoringService _scorer;

    public AnalyticsService(AppDbContext db, HealthScoringService scorer)
    {
        _db = db;
        _scorer = scorer;
    }

    public async Task<List<WeeklyStats>> GetWeeklyStatsAsync(int weeks = 4)
    {
        var allData = await _db.DailyTrackings
            .OrderByDescending(t => t.Date)
            .Take(weeks * 7)
            .ToListAsync();

        var user = await _db.UserMetrics.FirstOrDefaultAsync() ?? new Entities.UserMetric();

        var result = new List<WeeklyStats>();
        var groups = allData
            .GroupBy(t => {
                var d = t.Date.ToDateTime(TimeOnly.MinValue);
                int dow = (int)d.DayOfWeek;
                return DateOnly.FromDateTime(d.AddDays(dow == 0 ? -6 : -(dow - 1)));
            })
            .OrderByDescending(g => g.Key)
            .ToList();

        for (int i = 0; i < groups.Count; i++)
        {
            var current = groups[i].ToList();
            var previous = i + 1 < groups.Count ? groups[i + 1].ToList() : new List<Entities.DailyTracking>();

            decimal avgWeight = current.Any() ? current.Average(d => d.Weight) : 0;
            decimal prevAvg = previous.Any() ? previous.Average(d => d.Weight) : avgWeight;
            var score = _scorer.Calculate(current, previous, user);

            result.Add(new WeeklyStats(
                groups[i].Key,
                avgWeight,
                avgWeight - prevAvg,
                current.Any() ? current.Average(d => d.SystolicBP) : 0,
                current.Any() ? current.Average(d => d.DiastolicBP) : 0,
                current.Any() ? current.Average(d => d.Energy) : 0,
                current.Any() ? current.Average(d => d.Adherence) : 0,
                current.Sum(d => d.Steps),
                score
            ));
        }

        return result;
    }

    public async Task<List<Entities.DailyTracking>> GetAllTrackingAsync()
        => await _db.DailyTrackings.OrderByDescending(t => t.Date).ToListAsync();
}
