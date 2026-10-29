using PersonalFinance.Models;

namespace PersonalFinance.Helpers;

public static class FinanceMath
{
    public static decimal ActualIncome(IEnumerable<WorkLog> logs)
        => logs.Sum(x => x.Hours * x.HourlyRateUsed);

    public static decimal TotalHours(IEnumerable<WorkLog> logs)
        => logs.Sum(x => x.Hours);

    public static decimal EffectiveRate(IEnumerable<WorkLog> logs, decimal currentRate)
    {
        var items = logs as IList<WorkLog> ?? logs.ToList();
        var hours = items.Sum(x => x.Hours);
        if (hours <= 0)
        {
            return currentRate;
        }

        return items.Sum(x => x.Hours * x.HourlyRateUsed) / hours;
    }

    public static decimal ExpectedIncome(decimal targetHours, decimal rate) => targetHours * rate;

    public static decimal ProgressPercent(decimal actualHours, decimal targetHours)
        => targetHours <= 0 ? 0 : decimal.Round(actualHours / targetHours * 100m, 1);

    public static decimal Budget(decimal actualIncome, decimal percent)
        => decimal.Round(actualIncome * (percent / 100m), 0);

    public static decimal UsagePercent(decimal spent, decimal budget)
    {
        if (budget <= 0)
        {
            return spent > 0 ? 100m : 0m;
        }

        return decimal.Round(spent / budget * 100m, 1);
    }
}
