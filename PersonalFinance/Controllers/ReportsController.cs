using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Weekly(string? week, CancellationToken cancellationToken)
    {
        ViewBag.Active = "weekly-report";
        var weekStart = ResolveWeek(week);
        var weekEnd = PersianDateHelper.GetWeekEndExclusive(weekStart);
        var plan = await _db.WeeklyPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WeekStart == weekStart, cancellationToken);
        var report = await ReportBuilder.BuildAsync(
            _db, weekStart, weekEnd, "گزارش هفتگی", PersianDateHelper.WeekRangeLabel(weekStart),
            plan?.TargetHours, cancellationToken);

        return View(new WeeklyReportPageViewModel
        {
            WeekStart = weekStart,
            WeekOptions = PersianDateHelper.GetWeekOptions(DateTime.Today, 20, 2),
            Report = report
        });
    }

    public async Task<IActionResult> Monthly(int? year, int? month, CancellationToken cancellationToken)
    {
        ViewBag.Active = "monthly-report";
        var (currentYear, currentMonth, _) = PersianDateHelper.ToPersian(DateTime.Today);
        if (year is < 1200 or > 1600 || month is < 1 or > 12)
        {
            year = currentYear;
            month = currentMonth;
        }

        year ??= currentYear;
        month ??= currentMonth;
        var from = PersianDateHelper.GetMonthStart(year.Value, month.Value);
        var to = PersianDateHelper.GetMonthEndExclusive(year.Value, month.Value);
        var goal = await _db.MonthlyGoals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PersianYear == year && x.PersianMonth == month, cancellationToken);
        var report = await ReportBuilder.BuildAsync(
            _db, from, to, PersianDateHelper.MonthTitle(year.Value, month.Value),
            $"{PersianDateHelper.ToShamsiDisplay(from)} تا {PersianDateHelper.ToShamsiDisplay(to.AddDays(-1))}",
            goal?.TargetHours, cancellationToken);

        return View(new MonthlyReportPageViewModel
        {
            Year = year.Value,
            Month = month.Value,
            MonthOptions = PersianDateHelper.GetMonthOptions(DateTime.Today, 24, 2),
            Report = report
        });
    }

    private static DateTime ResolveWeek(string? week)
    {
        if (!string.IsNullOrWhiteSpace(week) && DateTime.TryParse(week, out var parsed))
        {
            return PersianDateHelper.GetWeekStart(parsed);
        }

        if (PersianDateHelper.TryParse(week, out var shamsi))
        {
            return PersianDateHelper.GetWeekStart(shamsi);
        }

        return PersianDateHelper.GetWeekStart(DateTime.Today);
    }
}
