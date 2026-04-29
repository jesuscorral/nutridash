using NutriDash.Infrastructure.Data.Entities;

namespace NutriDash.Infrastructure.Services;

public record SupplementRecommendation(
    string Name,
    string Dose,
    string Timing,
    string Reason,
    string Priority // "Alta" | "Media" | "Baja"
);

public class SupplementRecommendationService
{
    public List<SupplementRecommendation> GetRecommendations(
        IEnumerable<DailyTracking> recent,
        UserMetric targets)
    {
        var list = recent.ToList();
        var recs = new List<SupplementRecommendation>();

        if (!list.Any()) return GetDefaultRecommendations();

        var avgSys = list.Average(t => t.SystolicBP);
        var avgSleep = (double)list.Average(t => t.Sleep);
        var avgEnergy = list.Average(t => t.Energy);
        var avgAdh = list.Average(t => t.Adherence);
        var hasStrength = list.Any(t => t.WorkoutType == WorkoutType.Strength);

        // Omega-3 — always recommended, priority up if BP elevated
        recs.Add(new SupplementRecommendation(
            "Omega-3 EPA+DHA",
            avgSys > 130 ? "3g / día" : "2g / día",
            "Con la comida",
            avgSys > 130
                ? $"Tensión sistólica media {avgSys:F0} mmHg. DHA/EPA reducen inflamación vascular."
                : "Perfil cardiovascular y antiinflamatorio general.",
            avgSys > 130 ? "Alta" : "Media"
        ));

        // Magnesio — priority up if sleep < 7h or energy < 3
        var needsMg = avgSleep < 7 || avgEnergy < 3;
        recs.Add(new SupplementRecommendation(
            "Magnesio Bisglicinato",
            "400mg",
            "30 min antes de dormir",
            needsMg
                ? $"Sueño medio {avgSleep:F1}h / Energía {avgEnergy:F1}/5. El magnesio mejora calidad de sueño y energía celular."
                : "Soporte muscular, nervioso y de recuperación.",
            needsMg ? "Alta" : "Media"
        ));

        // Creatina — if strength training
        if (hasStrength)
        {
            recs.Add(new SupplementRecommendation(
                "Creatina Monohidrato",
                "5g / día",
                "Post-entrenamiento (días de fuerza)",
                "Días de entrenamiento de fuerza detectados. Mejora rendimiento y retención de masa muscular en déficit calórico.",
                "Alta"
            ));
        }

        // Vitamina D3 + K2 — always
        recs.Add(new SupplementRecommendation(
            "Vitamina D3 + K2",
            "D3: 2000 UI · K2: 100mcg",
            "Con desayuno (con grasa)",
            "Fundamental para inmunidad, salud ósea y optimización hormonal. K2 dirige el calcio al hueso, no a arterias.",
            "Alta"
        ));

        // Extra: simplify note if adherence low
        if (avgAdh < 60)
        {
            recs.Add(new SupplementRecommendation(
                "⚠️ Nota de adherencia",
                "—",
                "—",
                $"Adherencia media {avgAdh:F0}%. Considera simplificar el plan de comidas antes de añadir suplementos.",
                "Alta"
            ));
        }

        return recs;
    }

    private static List<SupplementRecommendation> GetDefaultRecommendations() =>
        new()
        {
            new("Omega-3 EPA+DHA", "2g / día", "Con la comida", "Perfil cardiovascular y antiinflamatorio.", "Media"),
            new("Magnesio Bisglicinato", "400mg", "Antes de dormir", "Soporte muscular y de sueño.", "Media"),
            new("Creatina Monohidrato", "5g / día", "Post-entrenamiento", "Rendimiento y masa muscular.", "Alta"),
            new("Vitamina D3 + K2", "D3: 2000 UI · K2: 100mcg", "Con desayuno", "Inmunidad y salud ósea.", "Alta")
        };
}
