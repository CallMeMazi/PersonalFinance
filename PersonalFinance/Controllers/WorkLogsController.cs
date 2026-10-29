using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class WorkLogsController : Controller
{
    private readonly ApplicationDbContext _db;

    public WorkLogsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string filter = "month", int? year = null, int? month = null, string? week = null, CancellationToken cancellationToken = default)
    {
        ViewBag.Active = "worklogs";
        var today = DateTime.Today;
        var (currentYear, currentMonth, _) = PersianDateHelper.ToPersian(today);
        year ??= currentYear;
        month ??= currentMonth;
        if (month is < 1 or > 12 || year is < 1200 or > 1600)
        {
            year = currentYear;
            month = currentMonth;
        }

        DateTime from;
        DateTime to;
        DateTime weekStart;
        string rangeLabel;
        if (string.Equals(filter, "week", StringComparison.OrdinalIgnoreCase))
        {
            weekStart = ParseWeek(week) ?? PersianDateHelper.GetWeekStart(today);
            from = weekStart;
            to = PersianDateHelper.GetWeekEndExclusive(weekStart);
            rangeLabel = PersianDateHelper.WeekRangeLabel(weekStart);
            filter = "week";
        }
        else
        {
            from = PersianDateHelper.GetMonthStart(year.Value, month.Value);
            to = PersianDateHelper.GetMonthEndExclusive(year.Value, month.Value);
            weekStart = PersianDateHelper.GetWeekStart(today);
            rangeLabel = PersianDateHelper.MonthTitle(year.Value, month.Value);
            filter = "month";
        }

        var items = await _db.WorkLogs.AsNoTracking()
            .Where(x => x.WorkDate >= from && x.WorkDate < to)
            .OrderByDescending(x => x.WorkDate)
            .ToListAsync(cancellationToken);

        var model = new WorkLogListViewModel
        {
            Filter = filter,
            Year = year.Value,
            Month = month.Value,
            WeekStart = weekStart,
            RangeLabel = rangeLabel,
            TotalHours = FinanceMath.TotalHours(items),
            TotalIncome = FinanceMath.ActualIncome(items),
            Items = items.Select(x => new WorkLogRowViewModel
            {
                Id = x.Id,
                WorkDate = x.WorkDate,
                Hours = x.Hours,
                HourlyRateUsed = x.HourlyRateUsed,
                Income = x.Income,
                Description = x.Description
            }).ToList(),
            MonthOptions = PersianDateHelper.GetMonthOptions(today, 18, 2),
            WeekOptions = PersianDateHelper.GetWeekOptions(today, 16, 2)
        };

        return View(model);
    }

    public IActionResult Create()
    {
        ViewBag.Active = "worklogs";
        return View(new WorkLogFormViewModel
        {
            PersianDate = PersianDateHelper.ToShamsi(DateTime.Today),
            Hours = 8
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkLogFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "worklogs";
        if (!TryResolveDate(model, out var workDate))
        {
            return View(model);
        }

        if (await _db.WorkLogs.AnyAsync(x => x.WorkDate == workDate, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.PersianDate), "برای این روز قبلاً ساعت کاری ثبت شده است. همان مورد را ویرایش کنید.");
            return View(model);
        }

        var settings = await DbSeeder.GetSettingsAsync(_db, cancellationToken);
        _db.WorkLogs.Add(new WorkLog
        {
            WorkDate = workDate,
            Hours = model.Hours,
            Description = Trim(model.Description),
            HourlyRateUsed = settings.HourlyRate
        });
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "ساعت کاری روز ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewBag.Active = "worklogs";
        var item = await _db.WorkLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new WorkLogFormViewModel
        {
            Id = item.Id,
            PersianDate = PersianDateHelper.ToShamsi(item.WorkDate),
            Hours = item.Hours,
            Description = item.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkLogFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "worklogs";
        var item = await _db.WorkLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (!TryResolveDate(model, out var workDate))
        {
            return View(model);
        }

        if (await _db.WorkLogs.AnyAsync(x => x.WorkDate == workDate && x.Id != id, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.PersianDate), "برای این روز قبلاً ساعت کاری دیگری ثبت شده است.");
            return View(model);
        }

        item.WorkDate = workDate;
        item.Hours = model.Hours;
        item.Description = Trim(model.Description);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "ثبت ساعت کاری به‌روزرسانی شد. نرخ ساعتی این روز تغییر نکرد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await _db.WorkLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        _db.WorkLogs.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "ثبت ساعت کاری حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private bool TryResolveDate(WorkLogFormViewModel model, out DateTime workDate)
    {
        workDate = default;
        if (!ModelState.IsValid)
        {
            return false;
        }

        if (!PersianDateHelper.TryParse(model.PersianDate, out workDate))
        {
            ModelState.AddModelError(nameof(model.PersianDate), "تاریخ شمسی نامعتبر است.");
            return false;
        }

        return true;
    }

    private static DateTime? ParseWeek(string? week)
    {
        if (string.IsNullOrWhiteSpace(week))
        {
            return null;
        }

        if (DateTime.TryParse(week, out var gregorian))
        {
            return PersianDateHelper.GetWeekStart(gregorian);
        }

        return PersianDateHelper.TryParse(week, out var shamsi) ? PersianDateHelper.GetWeekStart(shamsi) : null;
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
