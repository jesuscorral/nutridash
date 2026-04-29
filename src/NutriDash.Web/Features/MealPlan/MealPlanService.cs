using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using NutriDash.Infrastructure.Data.Entities;
using NutriDash.Infrastructure.Services;

namespace NutriDash.Features.MealPlan;

public class MealPlanService
{
    private readonly AppDbContext _db;
    private readonly GeminiService _gemini;
    private readonly AdaptiveNutritionService _adaptive;

    public MealPlanService(AppDbContext db, GeminiService gemini, AdaptiveNutritionService adaptive)
    {
        _db = db;
        _gemini = gemini;
        _adaptive = adaptive;
    }

    public async Task<List<NutriDash.Infrastructure.Data.Entities.MealPlan>> GetCurrentWeekAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int dow = (int)today.DayOfWeek;
        var monday = today.AddDays(dow == 0 ? -6 : -(dow - 1));

        return await _db.MealPlans
            .Include(mp => mp.Meals).ThenInclude(m => m.Ingredients)
            .Where(mp => mp.WeekStartDate == monday)
            .OrderBy(mp => mp.Day)
            .ToListAsync();
    }

    public async Task<List<(int Batch, DateOnly WeekStart, DateTime Generated)>> GetHistoryBatchesAsync()
    {
        var batches = await _db.MealPlans
            .GroupBy(mp => new { mp.GenerationBatch, mp.WeekStartDate, mp.GeneratedAt })
            .Select(g => new { g.Key.GenerationBatch, g.Key.WeekStartDate, g.Key.GeneratedAt })
            .OrderByDescending(x => x.GeneratedAt)
            .ToListAsync();

        return batches.Select(b => (b.GenerationBatch, b.WeekStartDate, b.GeneratedAt)).ToList();
    }

    public async Task<List<NutriDash.Infrastructure.Data.Entities.MealPlan>> GetBatchAsync(int batch)
    {
        return await _db.MealPlans
            .Include(mp => mp.Meals).ThenInclude(m => m.Ingredients)
            .Where(mp => mp.GenerationBatch == batch)
            .OrderBy(mp => mp.Day)
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> GenerateNewWeekAsync()
    {
        try
        {
            var user = await _db.UserMetrics.FirstOrDefaultAsync() ?? new UserMetric();
            var recent = await _db.DailyTrackings
                .OrderByDescending(t => t.Date).Take(14).ToListAsync();

            var ctx = _adaptive.BuildContext(recent, user);
            var prompt = _adaptive.BuildMealPlanPrompt(ctx, user);

            var jsonResponse = await _gemini.GenerateAsync(prompt);
            if (string.IsNullOrWhiteSpace(jsonResponse))
                return (false, "Gemini no devolvió contenido.");

            var plans = ParseGeminiResponse(jsonResponse);
            if (plans == null || !plans.Any())
                return (false, "No se pudo parsear la respuesta de Gemini.");

            // New batch ID
            var maxBatch = await _db.MealPlans.AnyAsync()
                ? await _db.MealPlans.MaxAsync(m => m.GenerationBatch)
                : 0;
            var newBatch = maxBatch + 1;

            var today = DateOnly.FromDateTime(DateTime.Today);
            int dow = (int)today.DayOfWeek;
            var monday = today.AddDays(dow == 0 ? -6 : -(dow - 1));

            // Remove only current week's plans (keep history)
            var currentWeek = await _db.MealPlans
                .Where(mp => mp.WeekStartDate == monday)
                .ToListAsync();
            _db.MealPlans.RemoveRange(currentWeek);

            foreach (var plan in plans)
            {
                plan.WeekStartDate = monday;
                plan.GenerationBatch = newBatch;
                plan.GeneratedAt = DateTime.UtcNow;
                _db.MealPlans.Add(plan);
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private List<NutriDash.Infrastructure.Data.Entities.MealPlan>? ParseGeminiResponse(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var weekPlan = doc.RootElement.GetProperty("weekPlan");
            var plans = new List<NutriDash.Infrastructure.Data.Entities.MealPlan>();
            var dayMap = new Dictionary<string, DayOfWeek>(StringComparer.OrdinalIgnoreCase)
            {
                ["Lunes"] = DayOfWeek.Monday, ["Monday"] = DayOfWeek.Monday,
                ["Martes"] = DayOfWeek.Tuesday, ["Tuesday"] = DayOfWeek.Tuesday,
                ["Miércoles"] = DayOfWeek.Wednesday, ["Wednesday"] = DayOfWeek.Wednesday,
                ["Jueves"] = DayOfWeek.Thursday, ["Thursday"] = DayOfWeek.Thursday,
                ["Viernes"] = DayOfWeek.Friday, ["Friday"] = DayOfWeek.Friday,
                ["Sábado"] = DayOfWeek.Saturday, ["Saturday"] = DayOfWeek.Saturday,
                ["Domingo"] = DayOfWeek.Sunday, ["Sunday"] = DayOfWeek.Sunday
            };

            foreach (var dayEl in weekPlan.EnumerateArray())
            {
                var dayName = dayEl.TryGetProperty("day", out var d) ? d.GetString() ?? "" : "";
                var dayOfWeek = dayMap.TryGetValue(dayName, out var dw) ? dw : DayOfWeek.Monday;
                var wtStr = dayEl.TryGetProperty("workoutType", out var wt) ? wt.GetString() ?? "" : "";
                var workoutType = wtStr.Contains("Fuerza", StringComparison.OrdinalIgnoreCase) ? WorkoutType.Strength
                    : wtStr.Contains("Cardio", StringComparison.OrdinalIgnoreCase) ? WorkoutType.Cardio
                    : WorkoutType.Rest;

                var meals = new List<Meal>();
                if (dayEl.TryGetProperty("meals", out var mealsEl))
                {
                    foreach (var (key, mealType) in new[] {
                        ("breakfast", MealType.Breakfast),
                        ("lunch", MealType.Lunch),
                        ("dinner", MealType.Dinner)
                    })
                    {
                        if (mealsEl.TryGetProperty(key, out var mealEl))
                            meals.Add(ParseMeal(mealEl, mealType));
                    }
                }

                plans.Add(new NutriDash.Infrastructure.Data.Entities.MealPlan
                {
                    Day = dayOfWeek,
                    WorkoutType = workoutType,
                    Meals = meals
                });
            }

            return plans;
        }
        catch { return null; }
    }

    private static Meal ParseMeal(JsonElement el, MealType type)
    {
        var meal = new Meal
        {
            MealType = type,
            Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            TotalCalories = el.TryGetProperty("calories", out var cal) ? cal.GetInt32() : 0,
            Protein = el.TryGetProperty("protein", out var p) ? (decimal)p.GetDouble() : 0,
            Carbs = el.TryGetProperty("carbs", out var c) ? (decimal)c.GetDouble() : 0,
            Fat = el.TryGetProperty("fat", out var f) ? (decimal)f.GetDouble() : 0,
        };

        if (el.TryGetProperty("ingredients", out var ings))
        {
            foreach (var ing in ings.EnumerateArray())
            {
                meal.Ingredients.Add(new Ingredient
                {
                    Name = ing.TryGetProperty("name", out var nm) ? nm.GetString() ?? "" : "",
                    Grams = ing.TryGetProperty("grams", out var g) ? (decimal)g.GetDouble() : 0,
                    Calories = ing.TryGetProperty("calories", out var ic) ? ic.GetInt32() : 0,
                    Protein = ing.TryGetProperty("protein", out var ip) ? (decimal)ip.GetDouble() : 0,
                    Carbs = ing.TryGetProperty("carbs", out var icarb) ? (decimal)icarb.GetDouble() : 0,
                    Fat = ing.TryGetProperty("fat", out var ifat) ? (decimal)ifat.GetDouble() : 0,
                    Category = ing.TryGetProperty("category", out var cat) ? cat.GetString() ?? "Otros" : "Otros"
                });
            }
        }

        return meal;
    }
}
