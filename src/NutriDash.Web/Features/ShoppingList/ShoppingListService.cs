using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data;
using NutriDash.Infrastructure.Data.Entities;
using NutriDash.Infrastructure.Services;

namespace NutriDash.Features.ShoppingList;

public record ShoppingCategory(string Name, string Icon, List<ShoppingItem> Items);
public record ShoppingItem(string Name, decimal TotalGrams, int Meals);

public class ShoppingListService
{
    private readonly AppDbContext _db;
    private readonly GeminiService _gemini;
    private readonly ILogger<ShoppingListService> _logger;

    public ShoppingListService(AppDbContext db, GeminiService gemini, ILogger<ShoppingListService> logger)
    {
        _db = db;
        _gemini = gemini;
        _logger = logger;
    }

    public async Task<List<ShoppingCategory>> GenerateFromCurrentWeekAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int dow = (int)today.DayOfWeek;
        var monday = today.AddDays(dow == 0 ? -6 : -(dow - 1));

        var ingredients = await _db.Ingredients
            .Include(i => i.Meal).ThenInclude(m => m.MealPlan)
            .Where(i => i.Meal.MealPlan.WeekStartDate == monday)
            .ToListAsync();

        if (!ingredients.Any())
        {
            _logger.LogDebug("GenerateFromCurrentWeekAsync: no ingredients found for week starting {Monday}", monday);
            return new List<ShoppingCategory>();
        }

        // Consolidate by name + category
        var consolidated = ingredients
            .GroupBy(i => new { Name = i.Name.Trim(), i.Category })
            .Select(g => new
            {
                g.Key.Name,
                g.Key.Category,
                TotalGrams = g.Sum(i => i.Grams),
                Count = g.Count()
            })
            .OrderBy(x => x.Name)
            .ToList();

        var categoryOrder = new Dictionary<string, int>
        {
            ["Proteínas"] = 0,
            ["Lácteos"] = 1,
            ["Verduras"] = 2,
            ["Carbohidratos"] = 3,
            ["Grasas saludables"] = 4,
            ["Frutas"] = 5,
            ["Otros"] = 6
        };
        var categoryIcons = new Dictionary<string, string>
        {
            ["Proteínas"] = "🥩",
            ["Lácteos"] = "🥛",
            ["Verduras"] = "🥦",
            ["Carbohidratos"] = "🌾",
            ["Grasas saludables"] = "🥑",
            ["Frutas"] = "🍎",
            ["Otros"] = "🧂"
        };

        return consolidated
            .GroupBy(x => x.Category)
            .OrderBy(g => categoryOrder.TryGetValue(g.Key, out var o) ? o : 99)
            .Select(g => new ShoppingCategory(
                g.Key,
                categoryIcons.TryGetValue(g.Key, out var icon) ? icon : "📦",
                g.Select(i => new ShoppingItem(i.Name, i.TotalGrams, i.Count)).ToList()
            ))
            .ToList();
    }
}
