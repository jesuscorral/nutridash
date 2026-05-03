namespace NutriDash.Infrastructure.Data.Entities;

public class UserMetric
{
    public int Id { get; set; }
    public string Name { get; set; } = "Usuario";
    public int Age { get; set; } = 38;
    public decimal Height { get; set; } = 178;
    public decimal GoalWeight { get; set; } = 79;
    public int TargetSystolicBP { get; set; } = 120;
    public int TargetDiastolicBP { get; set; } = 80;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
