using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Data;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance.Controllers;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Active = "categories";
        var items = await _db.ExpenseCategories.AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new CategoryListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Color = x.Color,
                Icon = x.Icon,
                Description = x.Description,
                AllocationPercent = x.AllocationPercent,
                ExpenseCount = x.Expenses.Count
            })
            .ToListAsync(cancellationToken);

        var allocated = items.Sum(x => x.AllocationPercent);
        return View(new CategoryListViewModel
        {
            Items = items,
            AllocatedPercent = allocated,
            UnallocatedPercent = Math.Max(0, 100 - allocated)
        });
    }

    public IActionResult Create()
    {
        ViewBag.Active = "categories";
        return View(new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "categories";
        if (!await ValidateCategoryAsync(model, null, cancellationToken))
        {
            return View(model);
        }

        _db.ExpenseCategories.Add(new ExpenseCategory
        {
            Title = model.Title.Trim(),
            Color = model.Color,
            Icon = Trim(model.Icon),
            Description = Trim(model.Description),
            AllocationPercent = model.AllocationPercent
        });
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "دسته‌بندی جدید ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewBag.Active = "categories";
        var item = await _db.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new CategoryFormViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Color = item.Color,
            Icon = item.Icon,
            Description = item.Description,
            AllocationPercent = item.AllocationPercent
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.Active = "categories";
        var item = await _db.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (!await ValidateCategoryAsync(model, id, cancellationToken))
        {
            return View(model);
        }

        item.Title = model.Title.Trim();
        item.Color = model.Color;
        item.Icon = Trim(model.Icon);
        item.Description = Trim(model.Description);
        item.AllocationPercent = model.AllocationPercent;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "دسته‌بندی به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await _db.ExpenseCategories
            .Include(x => x.Expenses)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (item.Expenses.Count > 0)
        {
            TempData["Error"] = "این دسته هزینه وابسته دارد و قابل حذف نیست. ابتدا هزینه‌های آن را حذف یا جابه‌جا کنید.";
            return RedirectToAction(nameof(Index));
        }

        _db.ExpenseCategories.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "دسته‌بندی حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateCategoryAsync(CategoryFormViewModel model, int? id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        var title = model.Title.Trim();
        var duplicate = await _db.ExpenseCategories
            .AnyAsync(x => x.Title == title && x.Id != id, cancellationToken);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Title), "دسته‌بندی با این عنوان از قبل وجود دارد.");
            return false;
        }

        var otherPercent = await _db.ExpenseCategories
            .Where(x => x.Id != id)
            .SumAsync(x => (decimal?)x.AllocationPercent, cancellationToken) ?? 0;
        if (otherPercent + model.AllocationPercent > 100)
        {
            ModelState.AddModelError(nameof(model.AllocationPercent),
                $"مجموع درصد تخصیص نمی‌تواند بیشتر از ۱۰۰ باشد. حداکثر مجاز برای این دسته {100 - otherPercent} درصد است.");
            return false;
        }

        return true;
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
