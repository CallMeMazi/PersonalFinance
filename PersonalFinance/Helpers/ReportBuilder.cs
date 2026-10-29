using Microsoft.EntityFrameworkCore;
using PersonalFinance.Data;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Helpers;

public static class ReportBuilder
{
    public static async Task<PeriodReportViewModel> BuildAsync(
        ApplicationDbContext db,
        DateTime fromInclusive,
        DateTime toExclusive,
        string title,
        string rangeLabel,
        decimal? targetHours,
        CancellationToken cancellationToken)
    {
        var currentRate = (await db.FinanceSettings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken))?.HourlyRate ?? 0;
        var logs = await db.WorkLogs.AsNoTracking()
            .Where(x => x.WorkDate >= fromInclusive && x.WorkDate < toExclusive)
            .ToListAsync(cancellationToken);
        var expenses = await db.Expenses.AsNoTracking()
            .Where(x => x.OccurredOn >= fromInclusive && x.OccurredOn < toExclusive)
            .ToListAsync(cancellationToken);
        var categories = await db.ExpenseCategories.AsNoTracking()
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        return Build(fromInclusive, toExclusive, title, rangeLabel, targetHours, currentRate, logs, expenses, categories);
    }

    public static PeriodReportViewModel Build(
        DateTime fromInclusive,
        DateTime toExclusive,
        string title,
        string rangeLabel,
        decimal? targetHours,
        decimal currentRate,
        IReadOnlyList<WorkLog> logs,
        IReadOnlyList<Expense> expenses,
        IReadOnlyList<ExpenseCategory> categories)
    {
        var actualHours = FinanceMath.TotalHours(logs);
        var actualIncome = FinanceMath.ActualIncome(logs);
        var effectiveRate = FinanceMath.EffectiveRate(logs, currentRate);
        var expensesTotal = expenses.Sum(x => x.Amount);
        var spentByCategory = expenses
            .GroupBy(x => x.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var rows = categories
            .Select(category =>
            {
                var spent = spentByCategory.GetValueOrDefault(category.Id);
                var budget = FinanceMath.Budget(actualIncome, category.AllocationPercent);
                return new BudgetRowViewModel
                {
                    CategoryId = category.Id,
                    Title = category.Title,
                    Color = string.IsNullOrWhiteSpace(category.Color) ? "#7c6cff" : category.Color,
                    Icon = category.Icon,
                    AllocationPercent = category.AllocationPercent,
                    AllocatedBudget = budget,
                    ActualExpense = spent,
                    UsagePercent = FinanceMath.UsagePercent(spent, budget)
                };
            })
            .OrderByDescending(x => x.ActualExpense)
            .ThenBy(x => x.Title)
            .ToList();

        var allocatedPercent = categories.Sum(x => x.AllocationPercent);
        var unallocatedPercent = Math.Max(0, 100 - allocatedPercent);
        if (unallocatedPercent > 0)
        {
            var unallocatedBudget = FinanceMath.Budget(actualIncome, unallocatedPercent);
            rows.Add(new BudgetRowViewModel
            {
                Title = "تخصیص‌نیافته",
                Color = "#64748b",
                AllocationPercent = unallocatedPercent,
                AllocatedBudget = unallocatedBudget,
                ActualExpense = 0,
                UsagePercent = 0,
                IsUnallocated = true
            });
        }

        var spendingRows = rows.Where(x => !x.IsUnallocated).ToList();
        var top = spendingRows.FirstOrDefault(x => x.ActualExpense > 0);

        return new PeriodReportViewModel
        {
            From = fromInclusive,
            ToExclusive = toExclusive,
            Title = title,
            RangeLabel = rangeLabel,
            TargetHours = targetHours,
            ActualHours = actualHours,
            ActualIncome = actualIncome,
            EffectiveRate = effectiveRate,
            CurrentRate = currentRate,
            ExpensesTotal = expensesTotal,
            UnallocatedPercent = unallocatedPercent,
            RemainingBudgetTotal = spendingRows.Sum(x => Math.Max(0, x.Remaining)),
            OverspentTotal = spendingRows.Sum(x => x.Overspent),
            ConsumedBudgetTotal = expensesTotal,
            TopCategoryName = top?.Title,
            TopCategoryAmount = top?.ActualExpense ?? 0,
            BudgetRows = rows
        };
    }
}
