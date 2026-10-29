using PersonalFinance.Helpers;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Tests;

public class FinanceMathTests
{
    [Fact]
    public void DailyIncome_UsesHoursTimesStoredRate()
    {
        var logs = new[]
        {
            new WorkLog { Hours = 6, HourlyRateUsed = 200_000 }
        };

        Assert.Equal(1_200_000m, FinanceMath.ActualIncome(logs));
        Assert.Equal(6m, FinanceMath.TotalHours(logs));
    }

    [Fact]
    public void HistoricalIncome_DoesNotChangeWhenCurrentRateChanges()
    {
        var logs = new[]
        {
            new WorkLog { Hours = 4, HourlyRateUsed = 200_000 },
            new WorkLog { Hours = 2, HourlyRateUsed = 250_000 }
        };

        Assert.Equal(1_300_000m, FinanceMath.ActualIncome(logs));
        Assert.Equal(1_300_000m / 6m, FinanceMath.EffectiveRate(logs, 999_999));
    }

    [Fact]
    public void ExpectedIncome_IsTargetTimesEffectiveRate()
    {
        Assert.Equal(8_000_000m, FinanceMath.ExpectedIncome(40, 200_000));
    }

    [Fact]
    public void ProgressPercent_AvoidsDivideByZero_AndCanExceedHundred()
    {
        Assert.Equal(0m, FinanceMath.ProgressPercent(8, 0));
        Assert.Equal(125m, FinanceMath.ProgressPercent(10, 8));
    }

    [Fact]
    public void CategoryBudget_UsesActualIncomeTimesPercent()
    {
        Assert.Equal(2_000_000m, FinanceMath.Budget(10_000_000m, 20));
        Assert.Equal(150m, FinanceMath.UsagePercent(3_000_000m, 2_000_000m));
        Assert.Equal(100m, FinanceMath.UsagePercent(50_000m, 0));
        Assert.Equal(0m, FinanceMath.UsagePercent(0, 0));
    }

    [Fact]
    public void ReportBuilder_SeparatesWorkShortfallFromRealFinancialLoss()
    {
        var report = ReportBuilder.Build(
            new DateTime(2025, 3, 15),
            new DateTime(2025, 3, 22),
            "هفته",
            "بازه",
            40,
            200_000,
            [new WorkLog { Hours = 10, HourlyRateUsed = 200_000 }],
            [new Expense { Amount = 100_000, CategoryId = 1 }],
            [new ExpenseCategory { Id = 1, Title = "خوراک", AllocationPercent = 20, Color = "#f59e0b" }]);

        Assert.Equal(2_000_000m, report.ActualIncome);
        Assert.Equal(8_000_000m, report.ExpectedIncome);
        Assert.Equal(30m, report.RemainingHours);
        Assert.Equal(6_000_000m, report.PotentialMissedIncome);
        Assert.Equal(1_900_000m, report.NetIncome);
        Assert.Contains("زیان مالی واقعی نیست", report.WorkStatus);
        Assert.Equal(400_000m, report.BudgetRows.First(x => x.Title == "خوراک").AllocatedBudget);
        Assert.True(report.UnallocatedPercent > 0);
    }
}
