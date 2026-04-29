using NutriDash.Infrastructure.Data.Entities;

namespace NutriDash.Infrastructure.Services;

public record HealthScoreBreakdown(
    double WeightScore,
    double BpScore,
    double EnergyScore,
    double AdherenceScore,
    double Total,
    string Status // "Óptimo" | "Bueno" | "Mejorable" | "Crítico"
);

public class HealthScoringService
{
    // Weights must sum to 100
    private const double WeightWeight = 30;
    private const double BpWeight = 25;
    private const double EnergyWeight = 20;
    private const double AdherenceWeight = 25;

    public HealthScoreBreakdown Calculate(
        IEnumerable<DailyTracking> recentWeek,
        IEnumerable<DailyTracking> previousWeek,
        UserMetric targets)
    {
        var recent = recentWeek.ToList();
        var prev = previousWeek.ToList();

        // ── Weight trend score (30 pts) ──────────────────────────────────────────
        double weightScore = 20; // default neutral
        if (recent.Any() && prev.Any())
        {
            var recentAvg = (double)recent.Average(t => t.Weight);
            var prevAvg = (double)prev.Average(t => t.Weight);
            var delta = prevAvg - recentAvg; // positive = losing weight = good
            weightScore = delta switch
            {
                >= 0.5 => 30,
                >= 0.2 => 25,
                >= 0  => 20,
                >= -0.2 => 12,
                _ => 5
            };
        }

        // ── BP score (25 pts) ────────────────────────────────────────────────────
        double bpScore = 12;
        if (recent.Any())
        {
            var avgSys = recent.Average(t => t.SystolicBP);
            var avgDia = recent.Average(t => t.DiastolicBP);
            bpScore = (avgSys, avgDia) switch
            {
                var (s, d) when s < 120 && d < 80 => 25,
                var (s, d) when s < 130 && d < 85 => 20,
                var (s, d) when s < 140 && d < 90 => 14,
                var (s, _) when s < 160 => 7,
                _ => 3
            };
        }

        // ── Energy score (20 pts) ────────────────────────────────────────────────
        double energyScore = 8;
        if (recent.Any())
        {
            var avgEnergy = recent.Average(t => t.Energy);
            energyScore = avgEnergy / 5.0 * EnergyWeight;
        }

        // ── Adherence score (25 pts) ─────────────────────────────────────────────
        double adherenceScore = 10;
        if (recent.Any())
        {
            var avgAdh = recent.Average(t => t.Adherence);
            adherenceScore = avgAdh / 100.0 * AdherenceWeight;
        }

        var total = weightScore + bpScore + energyScore + adherenceScore;
        var status = total switch
        {
            >= 85 => "Óptimo",
            >= 65 => "Bueno",
            >= 45 => "Mejorable",
            _ => "Crítico"
        };

        return new HealthScoreBreakdown(weightScore, bpScore, energyScore, adherenceScore, total, status);
    }
}
