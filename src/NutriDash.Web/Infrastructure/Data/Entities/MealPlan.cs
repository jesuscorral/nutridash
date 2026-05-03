namespace NutriDash.Infrastructure.Data.Entities;

public class MealPlan
{
    public int Id { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public DayOfWeek Day { get; set; }
    public WorkoutType WorkoutType { get; set; }
    public int GenerationBatch { get; set; } // groups 7 days from same generation
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
}
