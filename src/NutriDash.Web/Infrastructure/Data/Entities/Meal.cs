namespace NutriDash.Infrastructure.Data.Entities;

public class Meal
{
    public int Id { get; set; }
    public int MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;
    public MealType MealType { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalCalories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public string? Notes { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
