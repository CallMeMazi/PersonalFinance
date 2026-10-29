using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class WeeklyPlansController : Controller
{
    private readonly ApplicationDbContext _db;

    public WeeklyPlansController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? week, CancellationToken cancellationToken)
    {
        ViewBag.Active = "weekly";
        var weekStart = ResolveWeek(week);
        return View(await BuildAsync(weekStart, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(WeeklyPlanPageViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "weekly";
        var weekStart = PersianDateHelper.GetWeekStart(model.WeekStart);
        if (!ModelState.IsValid || model.TargetHours is null)
        {
            var page = await BuildAsync(weekStart, cancellationToken);
            page.TargetHours = model.TargetHours;
            if (model.TargetHours is null)
            {
                ModelState.AddModelError(nameof(model.TargetHours), "ساعات هدف هفته را وارد کنید.");
            }

            return View(page);
        }

        var plan = await _db.WeeklyPlans.FirstOrDefaultAsync(x => x.WeekStart == weekStart, cancellationToken);
        if (plan is null)
        {
            _db.WeeklyPlans.Add(new WeeklyPlan
            {
                WeekStart = weekStart,
                TargetHours = model.TargetHours.Value
            });
        }
        else
        {
            plan.TargetHours = model.TargetHours.Value;
        }

        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "هدف ساعات این هفته ذخیره شد.";
        return RedirectToAction(nameof(Index), new { week = weekStart.ToString("yyyy-MM-dd") });
    }

    private async Task<WeeklyPlanPageViewModel> BuildAsync(DateTime weekStart, CancellationToken cancellationToken)
    {
        weekStart = PersianDateHelper.GetWeekStart(weekStart);
        var weekEnd = PersianDateHelper.GetWeekEndExclusive(weekStart);
        var plan = await _db.WeeklyPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WeekStart == weekStart, cancellationToken);
        var report = await ReportBuilder.BuildAsync(
            _db, weekStart, weekEnd, "گزارش هفته", PersianDateHelper.WeekRangeLabel(weekStart),
            plan?.TargetHours, cancellationToken);

        return new WeeklyPlanPageViewModel
        {
            WeekStart = weekStart,
            TargetHours = plan?.TargetHours,
            GoalExists = plan != null,
            Report = report,
            WeekOptions = PersianDateHelper.GetWeekOptions(DateTime.Today, 16, 2)
        };
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
