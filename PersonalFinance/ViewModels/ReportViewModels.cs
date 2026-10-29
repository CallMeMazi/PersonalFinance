namespace PersonalFinance.ViewModels;

public class BudgetRowViewModel
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Color { get; set; } = "#7c6cff";
    public string? Icon { get; set; }
    public decimal AllocationPercent { get; set; }
    public decimal AllocatedBudget { get; set; }
    public decimal ActualExpense { get; set; }
    public bool IsUnallocated { get; set; }
    public decimal Remaining => AllocatedBudget - ActualExpense;
    public decimal Overspent => ActualExpense > AllocatedBudget ? ActualExpense - AllocatedBudget : 0;
    public decimal UsagePercent { get; set; }
    public bool IsOverBudget => ActualExpense > AllocatedBudget;
}

public class TrendPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

public class PeriodReportViewModel
{
    public DateTime From { get; set; }
    public DateTime ToExclusive { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RangeLabel { get; set; } = string.Empty;
    public decimal? TargetHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal ActualIncome { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal CurrentRate { get; set; }
    public decimal ExpensesTotal { get; set; }
    public decimal UnallocatedPercent { get; set; }
    public decimal RemainingBudgetTotal { get; set; }
    public decimal OverspentTotal { get; set; }
    public decimal ConsumedBudgetTotal { get; set; }
    public string? TopCategoryName { get; set; }
    public decimal TopCategoryAmount { get; set; }
    public List<BudgetRowViewModel> BudgetRows { get; set; } = [];

    public bool HasGoal => TargetHours.HasValue;
    public decimal RemainingHours => TargetHours is null ? 0 : Math.Max(0, TargetHours.Value - ActualHours);
    public decimal OvertimeHours => TargetHours is null ? 0 : Math.Max(0, ActualHours - TargetHours.Value);
    public decimal HoursDifference => TargetHours is null ? 0 : ActualHours - TargetHours.Value;
    public decimal? ExpectedIncome => TargetHours is null ? null : TargetHours.Value * EffectiveRate;
    public decimal? IncomeDifference => ExpectedIncome is null ? null : ActualIncome - ExpectedIncome.Value;
    public decimal NetIncome => ActualIncome - ExpensesTotal;
    public decimal? ProgressPercent => TargetHours is null or <= 0 ? null : decimal.Round(ActualHours / TargetHours.Value * 100m, 1);
    public decimal PotentialMissedIncome => RemainingHours * EffectiveRate;
    public decimal ExtraIncome => OvertimeHours * EffectiveRate;

    public string WorkStatus
    {
        get
        {
            if (!HasGoal)
            {
                return "هدف کاری این بازه تنظیم نشده است.";
            }

            if (ActualHours < TargetHours)
            {
                return "عقب‌ماندگی نسبت به هدف کاری. این مقدار زیان مالی واقعی نیست؛ فقط نشان می‌دهد کمتر از برنامه کار کرده‌اید.";
            }

            if (ActualHours > TargetHours)
            {
                return "جلوتر از هدف کاری هستید و ساعات مازاد ثبت شده است.";
            }

            return "ساعات کاری دقیقاً مطابق هدف است.";
        }
    }

    public string FinanceStatus
    {
        get
        {
            if (NetIncome > 0)
            {
                return "خالص درآمد این بازه مثبت است.";
            }

            if (NetIncome < 0)
            {
                return "هزینه‌های ثبت‌شده از درآمد این بازه بیشتر است.";
            }

            return "درآمد و هزینه این بازه سر به سر هستند.";
        }
    }
}

public class StatCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string Tone { get; set; } = "primary";
}

public class DashboardViewModel
{
    public string TodayLabel { get; set; } = string.Empty;
    public decimal CurrentRate { get; set; }
    public PeriodReportViewModel Month { get; set; } = new();
    public PeriodReportViewModel Week { get; set; } = new();
    public List<TrendPointViewModel> Trends { get; set; } = [];
    public List<WorkLogRowViewModel> RecentWorkLogs { get; set; } = [];
    public List<ExpenseRowViewModel> RecentExpenses { get; set; } = [];
    public string TrendLabelsJson { get; set; } = "[]";
    public string TrendIncomeJson { get; set; } = "[]";
    public string TrendExpenseJson { get; set; } = "[]";
    public string CategoryLabelsJson { get; set; } = "[]";
    public string CategoryValuesJson { get; set; } = "[]";
    public string CategoryColorsJson { get; set; } = "[]";
}

public class WeeklyReportPageViewModel
{
    public DateTime WeekStart { get; set; }
    public List<(DateTime WeekStart, string Label)> WeekOptions { get; set; } = [];
    public PeriodReportViewModel Report { get; set; } = new();
}

public class MonthlyReportPageViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<(int Year, int Month, string Title)> MonthOptions { get; set; } = [];
    public PeriodReportViewModel Report { get; set; } = new();
}
