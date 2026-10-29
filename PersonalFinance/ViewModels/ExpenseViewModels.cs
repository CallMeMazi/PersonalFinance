using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinance.Helpers;

namespace PersonalFinance.ViewModels;

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "عنوان دسته را وارد کنید.")]
    [MaxLength(80, ErrorMessage = "عنوان حداکثر ۸۰ کاراکتر است.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "رنگ")]
    [Required(ErrorMessage = "رنگ را انتخاب کنید.")]
    [MaxLength(16)]
    public string Color { get; set; } = "#7c6cff";

    [Display(Name = "آیکون")]
    [MaxLength(16)]
    public string? Icon { get; set; }

    [Display(Name = "توضیحات")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Display(Name = "درصد تخصیص از درآمد")]
    [Range(typeof(decimal), "0", "100", ErrorMessage = "درصد تخصیص باید بین ۰ تا ۱۰۰ باشد.")]
    public decimal AllocationPercent { get; set; }
}

public class CategoryListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Color { get; set; } = "#7c6cff";
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public decimal AllocationPercent { get; set; }
    public int ExpenseCount { get; set; }
}

public class CategoryListViewModel
{
    public List<CategoryListItemViewModel> Items { get; set; } = [];
    public decimal AllocatedPercent { get; set; }
    public decimal UnallocatedPercent { get; set; }
}

public class ExpenseFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "مبلغ (تومان)")]
    [Required(ErrorMessage = "مبلغ را وارد کنید.")]
    [Range(typeof(decimal), "1", "999999999999", ErrorMessage = "مبلغ باید بزرگ‌تر از صفر باشد.")]
    public decimal Amount { get; set; }

    [Display(Name = "تاریخ شمسی")]
    [Required(ErrorMessage = "تاریخ را وارد کنید.")]
    [PersianDate]
    public string PersianDate { get; set; } = string.Empty;

    [Display(Name = "دسته‌بندی")]
    [Range(1, int.MaxValue, ErrorMessage = "دسته‌بندی را انتخاب کنید.")]
    public int CategoryId { get; set; }

    [Display(Name = "توضیحات")]
    [MaxLength(500)]
    public string? Description { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];
}

public class ExpenseRowViewModel
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime OccurredOn { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#7c6cff";
    public string? Description { get; set; }
}

public class ExpenseListViewModel
{
    public string? FromPersian { get; set; }
    public string? ToPersian { get; set; }
    public int? CategoryId { get; set; }
    public string? Query { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ExpenseRowViewModel> Items { get; set; } = [];
    public List<SelectListItem> Categories { get; set; } = [];
}
