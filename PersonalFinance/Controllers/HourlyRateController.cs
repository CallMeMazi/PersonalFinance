using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class HourlyRateController : Controller
{
    private readonly ApplicationDbContext _db;

    public HourlyRateController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Active = "income";
        return View(await BuildAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HourlyRateViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "income";
        if (!ModelState.IsValid)
        {
            var page = await BuildAsync(cancellationToken);
            page.HourlyRate = model.HourlyRate;
            return View(page);
        }

        var settings = await DbSeeder.GetSettingsAsync(_db, cancellationToken);
        settings.HourlyRate = model.HourlyRate;
        settings.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "نرخ درآمد ساعتی ذخیره شد. ثبت‌های قبلی با نرخ زمان خودشان باقی می‌مانند.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<HourlyRateViewModel> BuildAsync(CancellationToken cancellationToken)
    {
        var settings = await DbSeeder.GetSettingsAsync(_db, cancellationToken);
        var today = DateTime.Today;
        var (year, month, _) = PersianDateHelper.ToPersian(today);
        var monthStart = PersianDateHelper.GetMonthStart(year, month);
        var monthEnd = PersianDateHelper.GetMonthEndExclusive(year, month);
        var monthGoal = await _db.MonthlyGoals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PersianYear == year && x.PersianMonth == month, cancellationToken);
        var monthReport = await ReportBuilder.BuildAsync(
            _db, monthStart, monthEnd, PersianDateHelper.MonthTitle(year, month),
            PersianDateHelper.MonthTitle(year, month), monthGoal?.TargetHours, cancellationToken);

        var allLogs = await _db.WorkLogs.AsNoTracking().ToListAsync(cancellationToken);
        return new HourlyRateViewModel
        {
            HourlyRate = settings.HourlyRate,
            CurrentRate = settings.HourlyRate,
            UpdatedAt = settings.UpdatedAt,
            AllTimeHours = FinanceMath.TotalHours(allLogs),
            AllTimeIncome = FinanceMath.ActualIncome(allLogs),
            Month = monthReport
        };
    }
}
