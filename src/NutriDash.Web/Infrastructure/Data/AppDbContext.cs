using Microsoft.EntityFrameworkCore;
using NutriDash.Infrastructure.Data.Entities;

namespace NutriDash.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DailyTracking> DailyTrackings => Set<DailyTracking>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<SupplementLog> SupplementLogs => Set<SupplementLog>();
    public DbSet<UserMetric> UserMetrics => Set<UserMetric>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meal>()
            .HasOne(m => m.MealPlan)
            .WithMany(mp => mp.Meals)
            .HasForeignKey(m => m.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ingredient>()
            .HasOne(i => i.Meal)
            .WithMany(m => m.Ingredients)
            .HasForeignKey(i => i.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyTracking>()
            .HasIndex(d => d.Date).IsUnique();
        modelBuilder.Entity<AppSetting>()
            .HasIndex(s => s.Key).IsUnique();
    }
}
