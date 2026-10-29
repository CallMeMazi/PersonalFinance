using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class MonthlyGoalsController : Controller
{
    private readonly ApplicationDbContext _db;

    public MonthlyGoalsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int? year, int? month, CancellationToken cancellationToken)
    {
        ViewBag.Active = "monthly";
        var (y, m) = ResolveMonth(year, month);
        return View(await BuildAsync(y, m, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MonthlyGoalPageViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "monthly";
        var (year, month) = ResolveMonth(model.Year, model.Month);
        if (!ModelState.IsValid || model.TargetHours is null)
        {
            var page = await BuildAsync(year, month, cancellationToken);
            page.TargetHours = model.TargetHours;
            if (model.TargetHours is null)
            {
                ModelState.AddModelError(nameof(model.TargetHours), "ساعات هدف ماه را وارد کنید.");
            }

            return View(page);
        }

        var goal = await _db.MonthlyGoals.FirstOrDefaultAsync(
            x => x.PersianYear == year && x.PersianMonth == month, cancellationToken);
        if (goal is null)
        {
            _db.MonthlyGoals.Add(new MonthlyGoal
            {
                PersianYear = year,
                PersianMonth = month,
                TargetHours = model.TargetHours.Value
            });
        }
        else
        {
            goal.TargetHours = model.TargetHours.Value;
        }

        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "هدف ساعات این ماه ذخیره شد.";
        return RedirectToAction(nameof(Index), new { year, month });
    }

    private async Task<MonthlyGoalPageViewModel> BuildAsync(int year, int month, CancellationToken cancellationToken)
    {
        var from = PersianDateHelper.GetMonthStart(year, month);
        var to = PersianDateHelper.GetMonthEndExclusive(year, month);
        var goal = await _db.MonthlyGoals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PersianYear == year && x.PersianMonth == month, cancellationToken);
        var report = await ReportBuilder.BuildAsync(
            _db, from, to, PersianDateHelper.MonthTitle(year, month),
            $"{PersianDateHelper.ToShamsiDisplay(from)} تا {PersianDateHelper.ToShamsiDisplay(to.AddDays(-1))}",
            goal?.TargetHours, cancellationToken);

        return new MonthlyGoalPageViewModel
        {
            Year = year,
            Month = month,
            TargetHours = goal?.TargetHours,
            GoalExists = goal != null,
            Report = report,
            MonthOptions = PersianDateHelper.GetMonthOptions(DateTime.Today, 18, 2)
        };
    }

    private static (int Year, int Month) ResolveMonth(int? year, int? month)
    {
        var (currentYear, currentMonth, _) = PersianDateHelper.ToPersian(DateTime.Today);
        if (year is < 1200 or > 1600 || month is < 1 or > 12)
        {
            return (currentYear, currentMonth);
        }

        return (year ?? currentYear, month ?? currentMonth);
    }
}
