namespace NutriDash.Infrastructure.Data.Entities;

public class DailyTracking
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Weight { get; set; }
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int Steps { get; set; }
    public WorkoutType WorkoutType { get; set; }
    public decimal Sleep { get; set; }
    public int Energy { get; set; } // 1–5
    public int Adherence { get; set; } // 0–100
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
