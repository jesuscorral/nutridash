namespace NutriDash.Infrastructure.Data.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public int MealId { get; set; }
    public Meal Meal { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal Grams { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public string Category { get; set; } = "Other"; // Proteínas, Verduras, Carbohidratos, Grasas, Frutas, Lácteos
}
