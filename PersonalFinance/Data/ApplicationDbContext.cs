using Microsoft.EntityFrameworkCore;
using PersonalFinance.Models;

namespace PersonalFinance.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<FinanceSettings> FinanceSettings => Set<FinanceSettings>();
    public DbSet<WorkLog> WorkLogs => Set<WorkLog>();
    public DbSet<WeeklyPlan> WeeklyPlans => Set<WeeklyPlan>();
    public DbSet<MonthlyGoal> MonthlyGoals => Set<MonthlyGoal>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinanceSettings>(entity =>
        {
            entity.Property(x => x.HourlyRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<WorkLog>(entity =>
        {
            entity.HasIndex(x => x.WorkDate).IsUnique();
            entity.Property(x => x.WorkDate).HasColumnType("date");
            entity.Property(x => x.Hours).HasPrecision(6, 2);
            entity.Property(x => x.HourlyRateUsed).HasPrecision(18, 2);
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<WeeklyPlan>(entity =>
        {
            entity.HasIndex(x => x.WeekStart).IsUnique();
            entity.Property(x => x.WeekStart).HasColumnType("date");
            entity.Property(x => x.TargetHours).HasPrecision(6, 2);
        });

        modelBuilder.Entity<MonthlyGoal>(entity =>
        {
            entity.HasIndex(x => new { x.PersianYear, x.PersianMonth }).IsUnique();
            entity.Property(x => x.TargetHours).HasPrecision(6, 2);
        });

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasIndex(x => x.Title).IsUnique();
            entity.Property(x => x.Title).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Icon).HasMaxLength(16);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.AllocationPercent).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.OccurredOn).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Expenses)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
