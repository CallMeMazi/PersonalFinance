using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class HomeController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Active = "dashboard";
        var today = DateTime.Today;
        var (year, month, _) = PersianDateHelper.ToPersian(today);
        var monthStart = PersianDateHelper.GetMonthStart(year, month);
        var monthEnd = PersianDateHelper.GetMonthEndExclusive(year, month);
        var weekStart = PersianDateHelper.GetWeekStart(today);
        var weekEnd = PersianDateHelper.GetWeekEndExclusive(today);

        var monthGoal = await _db.MonthlyGoals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PersianYear == year && x.PersianMonth == month, cancellationToken);
        var weekPlan = await _db.WeeklyPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WeekStart == weekStart, cancellationToken);

        var monthReport = await ReportBuilder.BuildAsync(
            _db, monthStart, monthEnd, PersianDateHelper.MonthTitle(year, month),
            $"{PersianDateHelper.ToShamsiDisplay(monthStart)} تا {PersianDateHelper.ToShamsiDisplay(monthEnd.AddDays(-1))}",
            monthGoal?.TargetHours, cancellationToken);

        var weekReport = await ReportBuilder.BuildAsync(
            _db, weekStart, weekEnd, "هفته جاری", PersianDateHelper.WeekRangeLabel(weekStart),
            weekPlan?.TargetHours, cancellationToken);

        var trendStartYearMonth = PersianDateHelper.AddMonths(year, month, -5);
        var trendStart = PersianDateHelper.GetMonthStart(trendStartYearMonth.Year, trendStartYearMonth.Month);
        var trendLogs = await _db.WorkLogs.AsNoTracking()
            .Where(x => x.WorkDate >= trendStart && x.WorkDate < monthEnd)
            .ToListAsync(cancellationToken);
        var trendExpenses = await _db.Expenses.AsNoTracking()
            .Where(x => x.OccurredOn >= trendStart && x.OccurredOn < monthEnd)
            .ToListAsync(cancellationToken);

        var trends = new List<TrendPointViewModel>();
        for (var i = 5; i >= 0; i--)
        {
            var (y, m) = PersianDateHelper.AddMonths(year, month, -i);
            var from = PersianDateHelper.GetMonthStart(y, m);
            var to = PersianDateHelper.GetMonthEndExclusive(y, m);
            trends.Add(new TrendPointViewModel
            {
                Label = PersianDateHelper.MonthNames[m],
                Income = FinanceMath.ActualIncome(trendLogs.Where(x => x.WorkDate >= from && x.WorkDate < to)),
                Expense = trendExpenses.Where(x => x.OccurredOn >= from && x.OccurredOn < to).Sum(x => x.Amount)
            });
        }

        var recentLogs = await _db.WorkLogs.AsNoTracking()
            .OrderByDescending(x => x.WorkDate)
            .Take(6)
            .ToListAsync(cancellationToken);
        var recentExpenses = await _db.Expenses.AsNoTracking()
            .Include(x => x.Category)
            .OrderByDescending(x => x.OccurredOn)
            .ThenByDescending(x => x.Id)
            .Take(6)
            .ToListAsync(cancellationToken);

        var categorySlices = monthReport.BudgetRows.Where(x => !x.IsUnallocated && x.ActualExpense > 0).ToList();
        if (categorySlices.Count == 0)
        {
            categorySlices = monthReport.BudgetRows.Where(x => x.IsUnallocated).ToList();
        }

        var settings = await DbSeeder.GetSettingsAsync(_db, cancellationToken);

        var model = new DashboardViewModel
        {
            TodayLabel = PersianDateHelper.FullDateLabel(today),
            CurrentRate = settings.HourlyRate,
            Month = monthReport,
            Week = weekReport,
            Trends = trends,
            RecentWorkLogs = recentLogs.Select(MapWorkLog).ToList(),
            RecentExpenses = recentExpenses.Select(MapExpense).ToList(),
            TrendLabelsJson = JsonSerializer.Serialize(trends.Select(x => x.Label), JsonOptions),
            TrendIncomeJson = JsonSerializer.Serialize(trends.Select(x => x.Income), JsonOptions),
            TrendExpenseJson = JsonSerializer.Serialize(trends.Select(x => x.Expense), JsonOptions),
            CategoryLabelsJson = JsonSerializer.Serialize(categorySlices.Select(x => x.Title), JsonOptions),
            CategoryValuesJson = JsonSerializer.Serialize(categorySlices.Select(x => x.ActualExpense), JsonOptions),
            CategoryColorsJson = JsonSerializer.Serialize(categorySlices.Select(x => x.Color), JsonOptions)
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static WorkLogRowViewModel MapWorkLog(WorkLog log) => new()
    {
        Id = log.Id,
        WorkDate = log.WorkDate,
        Hours = log.Hours,
        HourlyRateUsed = log.HourlyRateUsed,
        Income = log.Income,
        Description = log.Description
    };

    private static ExpenseRowViewModel MapExpense(Expense expense) => new()
    {
        Id = expense.Id,
        Amount = expense.Amount,
        OccurredOn = expense.OccurredOn,
        CategoryTitle = expense.Category.Title,
        CategoryColor = expense.Category.Color,
        Description = expense.Description
    };
}
