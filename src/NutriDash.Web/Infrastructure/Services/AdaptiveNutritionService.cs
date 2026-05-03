using NutriDash.Infrastructure.Data.Entities;

namespace NutriDash.Infrastructure.Services;

public record AdaptiveContext(
    decimal CurrentWeight,
    decimal GoalWeight,
    decimal WeeklyWeightDelta,   // negative = losing
    double AvgSystolic,
    double AvgDiastolic,
    double AvgEnergy,
    double AvgAdherence,
    double AvgSleep,
    WorkoutType[] WeekSchedule,
    string[] ActiveFlags,        // e.g. "REDUCE_CARBS", "LOW_SODIUM"
    string Notes
);

public class AdaptiveNutritionService
{
    private readonly ILogger<AdaptiveNutritionService> _logger;

    public AdaptiveNutritionService(ILogger<AdaptiveNutritionService> logger)
    {
        _logger = logger;
    }

    public AdaptiveContext BuildContext(
        IEnumerable<DailyTracking> recentDays,
        UserMetric user)
    {
        var days = recentDays.OrderByDescending(d => d.Date).ToList();
        if (!days.Any())
        {
            _logger.LogWarning("BuildContext: no tracking data found, returning defaults");
            return new AdaptiveContext(user.GoalWeight + 5, user.GoalWeight, 0,
                120, 80, 3, 70, 7,
                Array.Empty<WorkoutType>(), Array.Empty<string>(), "No hay datos de tracking disponibles.");
        }

        var week1 = days.Take(7).ToList();
        var week2 = days.Skip(7).Take(7).ToList();

        var currentWeight = week1.Any() ? week1.First().Weight : days.First().Weight;
        decimal weekDelta = 0;
        if (week1.Any() && week2.Any())
            weekDelta = week1.Average(d => d.Weight) - week2.Average(d => d.Weight);

        var avgSys = week1.Any() ? week1.Average(d => d.SystolicBP) : 130;
        var avgDia = week1.Any() ? week1.Average(d => d.DiastolicBP) : 85;
        var avgEnergy = week1.Any() ? week1.Average(d => d.Energy) : 3.0;
        var avgAdh = week1.Any() ? week1.Average(d => d.Adherence) : 70.0;
        var avgSleep = week1.Any() ? (double)week1.Average(d => d.Sleep) : 7.0;

        var schedule = week1.OrderBy(d => d.Date).Select(d => d.WorkoutType).ToArray();

        // ── Adaptive flags ────────────────────────────────────────────────────
        var flags = new List<string>();
        var notes = new List<string>();

        if (weekDelta >= 0)
        {
            flags.Add("REDUCE_CARBS");
            notes.Add("Sin bajada de peso esta semana → reducir carbohidratos en reposo.");
        }
        if (avgSys > 135)
        {
            flags.Add("LOW_SODIUM");
            flags.Add("NO_ALCOHOL");
            notes.Add($"Tensión sistólica media {avgSys:F0} mmHg → dieta baja en sodio, sin alcohol.");
        }
        if (avgEnergy < 3.0)
        {
            flags.Add("INCREASE_CARBS_STRENGTH");
            notes.Add($"Energía media {avgEnergy:F1}/5 → aumentar carbohidratos en días de fuerza.");
        }
        if (avgAdh < 60)
        {
            flags.Add("SIMPLIFY_DIET");
            notes.Add($"Adherencia {avgAdh:F0}% → simplificar el plan, pocas recetas rotativas.");
        }

        var context = new AdaptiveContext(
            currentWeight, user.GoalWeight, weekDelta,
            avgSys, avgDia, avgEnergy, avgAdh, avgSleep,
            schedule, flags.ToArray(),
            string.Join(" ", notes)
        );
        _logger.LogInformation("AdaptiveContext built: Weight={W}kg Goal={G}kg Delta={D:+0.0;-0.0;0.0}kg Flags=[{F}]",
            currentWeight, user.GoalWeight, weekDelta, string.Join(", ", flags));
        return context;
    }

    public string BuildMealPlanPrompt(AdaptiveContext ctx, UserMetric user)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Eres un nutricionista especialista en recomposición corporal y salud cardiovascular.");
        sb.AppendLine("Genera un plan de comidas semanal COMPLETO para 7 días en formato JSON ESTRICTO.");
        sb.AppendLine();
        sb.AppendLine("=== DATOS DEL USUARIO ===");
        sb.AppendLine($"- Nombre: {user.Name}, Edad: {user.Age}, Altura: {user.Height}cm");
        sb.AppendLine($"- Peso actual: {ctx.CurrentWeight}kg | Objetivo: {ctx.GoalWeight}kg");
        sb.AppendLine($"- Delta semanal peso: {ctx.WeeklyWeightDelta:+0.0;-0.0;0.0}kg");
        sb.AppendLine($"- Tensión media: {ctx.AvgSystolic:F0}/{ctx.AvgDiastolic:F0} mmHg");
        sb.AppendLine($"- Energía media: {ctx.AvgEnergy:F1}/5 | Adherencia: {ctx.AvgAdherence:F0}%");
        sb.AppendLine($"- Sueño medio: {ctx.AvgSleep:F1}h");
        sb.AppendLine();
        sb.AppendLine("=== REGLAS ADAPTATIVAS ACTIVAS ===");
        foreach (var f in ctx.ActiveFlags) sb.AppendLine($"- {f}");
        sb.AppendLine(ctx.Notes);
        sb.AppendLine();
        sb.AppendLine("=== CALENDARIO SEMANAL ===");
        var dayNames = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
        var schedule = new[] { "Fuerza", "Cardio", "Descanso", "Fuerza", "Cardio", "Descanso", "Descanso" };
        for (int i = 0; i < 7; i++)
            sb.AppendLine($"- {dayNames[i]}: {schedule[i]}");
        sb.AppendLine();
        sb.AppendLine("=== DIRECTRICES NUTRICIONALES ===");
        sb.AppendLine("- Objetivo: déficit de 400-500 kcal/día para pérdida de grasa controlada");
        sb.AppendLine("- Proteína: mínimo 1.8g/kg peso corporal (prioridad en días de fuerza)");
        sb.AppendLine("- Días fuerza: más carbohidratos complejos (quinoa, arroz integral, boniato)");
        sb.AppendLine("- Días cardio: carbohidratos moderados");
        sb.AppendLine("- Días descanso: mayor proporción proteína/grasa, menos carbohidratos");
        sb.AppendLine("- Batch cooking friendly: preparaciones que duran 3-4 días");
        sb.AppendLine("- Ingredientes reales, disponibles en supermercado español");
        sb.AppendLine("- Sin comidas procesadas, sin azúcares añadidos");
        if (ctx.ActiveFlags.Contains("LOW_SODIUM")) sb.AppendLine("- CRÍTICO: bajo en sodio (máx 1500mg/día), sin embutidos, sin conservas saladas");
        sb.AppendLine();
        sb.AppendLine("=== FORMATO JSON REQUERIDO (exacto, sin cambios) ===");
        sb.AppendLine(@"{
  ""weekPlan"": [
    {
      ""day"": ""Lunes"",
      ""dayIndex"": 0,
      ""workoutType"": ""Fuerza"",
      ""totalCalories"": 1650,
      ""totalProtein"": 145,
      ""totalCarbs"": 165,
      ""totalFat"": 48,
      ""meals"": {
        ""breakfast"": {
          ""name"": ""Nombre de la comida"",
          ""calories"": 420,
          ""protein"": 35,
          ""carbs"": 42,
          ""fat"": 10,
          ""ingredients"": [
            {""name"": ""Avena"", ""grams"": 80, ""calories"": 296, ""protein"": 10, ""carbs"": 55, ""fat"": 5, ""category"": ""Carbohidratos""}
          ]
        },
        ""lunch"": { /* mismo esquema */ },
        ""dinner"": { /* mismo esquema */ }
      }
    }
  ]
}");
        sb.AppendLine();
        sb.AppendLine("IMPORTANTE: Devuelve ÚNICAMENTE el JSON. Sin texto adicional. Sin markdown. Sin explicaciones.");
        sb.AppendLine("Las categorías de ingredientes válidas son: Proteínas, Verduras, Carbohidratos, Grasas saludables, Frutas, Lácteos, Otros");
        return sb.ToString();
    }
}
