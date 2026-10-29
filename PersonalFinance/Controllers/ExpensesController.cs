using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Data;
using PersonalFinance.Helpers;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ExpensesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? from, string? to, int? categoryId, string? q, CancellationToken cancellationToken)
    {
        ViewBag.Active = "expenses";
        var today = DateTime.Today;
        var monthStart = PersianDateHelper.GetMonthStart(today);
        var monthEnd = PersianDateHelper.GetMonthEndExclusive(today);

        var fromDate = PersianDateHelper.TryParse(from, out var parsedFrom) ? parsedFrom : monthStart;
        var toDate = PersianDateHelper.TryParse(to, out var parsedTo) ? parsedTo : monthEnd.AddDays(-1);
        if (toDate < fromDate)
        {
            (fromDate, toDate) = (toDate, fromDate);
        }

        var toExclusive = toDate.AddDays(1);
        var query = _db.Expenses.AsNoTracking().Include(x => x.Category)
            .Where(x => x.OccurredOn >= fromDate && x.OccurredOn < toExclusive);

        if (categoryId is > 0)
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Description != null && x.Description.Contains(term));
        }

        var items = await query
            .OrderByDescending(x => x.OccurredOn)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        var model = new ExpenseListViewModel
        {
            FromPersian = PersianDateHelper.ToShamsi(fromDate),
            ToPersian = PersianDateHelper.ToShamsi(toDate),
            CategoryId = categoryId,
            Query = q,
            TotalAmount = items.Sum(x => x.Amount),
            Items = items.Select(x => new ExpenseRowViewModel
            {
                Id = x.Id,
                Amount = x.Amount,
                OccurredOn = x.OccurredOn,
                CategoryTitle = x.Category.Title,
                CategoryColor = x.Category.Color,
                Description = x.Description
            }).ToList(),
            Categories = await CategoryOptionsAsync(categoryId, cancellationToken)
        };

        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Active = "expenses";
        return View(new ExpenseFormViewModel
        {
            PersianDate = PersianDateHelper.ToShamsi(DateTime.Today),
            Categories = await CategoryOptionsAsync(null, cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "expenses";
        model.Categories = await CategoryOptionsAsync(model.CategoryId, cancellationToken);
        var created = await TryResolveAsync(model, cancellationToken);
        if (!created.Ok)
        {
            return View(model);
        }

        _db.Expenses.Add(new Expense
        {
            Amount = model.Amount,
            OccurredOn = created.Date,
            CategoryId = model.CategoryId,
            Description = Trim(model.Description)
        });
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "هزینه ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewBag.Active = "expenses";
        var item = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new ExpenseFormViewModel
        {
            Id = item.Id,
            Amount = item.Amount,
            PersianDate = PersianDateHelper.ToShamsi(item.OccurredOn),
            CategoryId = item.CategoryId,
            Description = item.Description,
            Categories = await CategoryOptionsAsync(item.CategoryId, cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "expenses";
        var item = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        model.Categories = await CategoryOptionsAsync(model.CategoryId, cancellationToken);
        var updated = await TryResolveAsync(model, cancellationToken);
        if (!updated.Ok)
        {
            return View(model);
        }

        item.Amount = model.Amount;
        item.OccurredOn = updated.Date;
        item.CategoryId = model.CategoryId;
        item.Description = Trim(model.Description);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "هزینه به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        _db.Expenses.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "هزینه حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<(bool Ok, DateTime Date)> TryResolveAsync(ExpenseFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return (false, default);
        }

        if (!PersianDateHelper.TryParse(model.PersianDate, out var occurredOn))
        {
            ModelState.AddModelError(nameof(model.PersianDate), "تاریخ شمسی نامعتبر است.");
            return (false, default);
        }

        var exists = await _db.ExpenseCategories.AnyAsync(x => x.Id == model.CategoryId, cancellationToken);
        if (!exists)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "دسته‌بندی انتخاب‌شده معتبر نیست.");
            return (false, default);
        }

        return (true, occurredOn);
    }

    private async Task<List<SelectListItem>> CategoryOptionsAsync(int? selectedId, CancellationToken cancellationToken)
    {
        var items = await _db.ExpenseCategories.AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
            .ToListAsync(cancellationToken);

        if (selectedId is > 0)
        {
            var selected = items.FirstOrDefault(x => x.Value == selectedId.ToString());
            if (selected != null)
            {
                selected.Selected = true;
            }
        }

        return items;
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
