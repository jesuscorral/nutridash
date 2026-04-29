using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using Entities = NutriDash.Infrastructure.Data.Entities;
using NutriDash.Infrastructure.Services;

namespace NutriDash.Features.Dashboard;

public record DashboardSummary(
    decimal CurrentWeight,
    decimal GoalWeight,
    decimal WeightTrend7d,
    double AvgSystolic,
    double AvgDiastolic,
    double AvgEnergy,
    double AvgAdherence,
    NutriDash.Infrastructure.Services.HealthScoreBreakdown Score,
    List<decimal> WeightHistory,
    List<int> BpSystolicHistory,
    List<double> EnergyHistory,
    Entities.DailyTracking? TodayEntry,
    bool HasTodayEntry
);

public class DashboardService
{
    private readonly AppDbContext _db;
    private readonly HealthScoringService _scorer;

    public DashboardService(AppDbContext db, HealthScoringService scorer)
    {
        _db = db;
        _scorer = scorer;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var recent = await _db.DailyTrackings
            .OrderByDescending(t => t.Date)
            .Take(14).ToListAsync();

        var week1 = recent.Take(7).ToList();
        var week2 = recent.Skip(7).Take(7).ToList();

        var user = await _db.UserMetrics.FirstOrDefaultAsync()
            ?? new Entities.UserMetric();

        var todayEntry = recent.FirstOrDefault(t => t.Date == today);
        var currentWeight = recent.FirstOrDefault()?.Weight ?? user.GoalWeight;

        decimal weightTrend = 0;
        if (week1.Any() && week2.Any())
            weightTrend = week1.Average(d => d.Weight) - week2.Average(d => d.Weight);

        var score = _scorer.Calculate(week1, week2, user);

        return new DashboardSummary(
            CurrentWeight: currentWeight,
            GoalWeight: user.GoalWeight,
            WeightTrend7d: weightTrend,
            AvgSystolic: week1.Any() ? week1.Average(t => t.SystolicBP) : 0,
            AvgDiastolic: week1.Any() ? week1.Average(t => t.DiastolicBP) : 0,
            AvgEnergy: week1.Any() ? week1.Average(t => t.Energy) : 0,
            AvgAdherence: week1.Any() ? week1.Average(t => t.Adherence) : 0,
            Score: score,
            WeightHistory: recent.OrderBy(t => t.Date).Select(t => t.Weight).ToList(),
            BpSystolicHistory: recent.OrderBy(t => t.Date).Select(t => t.SystolicBP).ToList(),
            EnergyHistory: recent.OrderBy(t => t.Date).Select(t => (double)t.Energy).ToList(),
            TodayEntry: todayEntry,
            HasTodayEntry: todayEntry != null
        );
    }
}
