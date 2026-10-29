using Microsoft.EntityFrameworkCore;
using PersonalFinance.Models;

namespace PersonalFinance.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.FinanceSettings.AnyAsync(cancellationToken))
        {
            db.FinanceSettings.Add(new FinanceSettings
            {
                HourlyRate = 0,
                UpdatedAt = DateTime.Now
            });
        }

        if (!await db.ExpenseCategories.AnyAsync(cancellationToken))
        {
            db.ExpenseCategories.AddRange(
                new ExpenseCategory { Title = "خوراک", Color = "#f59e0b", Icon = "🍽️", AllocationPercent = 20, Description = "مواد غذایی و رستوران" },
                new ExpenseCategory { Title = "حمل‌ونقل", Color = "#38bdf8", Icon = "🚌", AllocationPercent = 10, Description = "سفر، تاکسی و سوخت" },
                new ExpenseCategory { Title = "اینترنت و موبایل", Color = "#818cf8", Icon = "📱", AllocationPercent = 10, Description = "بسته اینترنت و قبض موبایل" },
                new ExpenseCategory { Title = "تفریح", Color = "#e879f9", Icon = "🎮", AllocationPercent = 10, Description = "سرگرمی و اوقات فراغت" },
                new ExpenseCategory { Title = "پس‌انداز", Color = "#34d399", Icon = "💰", AllocationPercent = 30, Description = "پس‌انداز و سرمایه‌گذاری" },
                new ExpenseCategory { Title = "هزینه‌های متفرقه", Color = "#94a3b8", Icon = "📦", AllocationPercent = 10, Description = "سایر هزینه‌ها" }
            );
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task<FinanceSettings> GetSettingsAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var settings = await db.FinanceSettings.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (settings != null)
        {
            return settings;
        }

        settings = new FinanceSettings
        {
            HourlyRate = 0,
            UpdatedAt = DateTime.Now
        };
        db.FinanceSettings.Add(settings);
        await db.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
