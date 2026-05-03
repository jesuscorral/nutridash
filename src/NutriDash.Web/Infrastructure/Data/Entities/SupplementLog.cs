namespace NutriDash.Infrastructure.Data.Entities;

public class SupplementLog
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool Taken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
