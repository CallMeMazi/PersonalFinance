using System.ComponentModel.DataAnnotations;
using PersonalFinance.Helpers;

namespace PersonalFinance.ViewModels;

public class HourlyRateViewModel
{
    [Display(Name = "نرخ درآمد ساعتی (تومان)")]
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "نرخ ساعتی نمی‌تواند منفی باشد.")]
    public decimal HourlyRate { get; set; }

    public decimal CurrentRate { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal AllTimeHours { get; set; }
    public decimal AllTimeIncome { get; set; }
    public PeriodReportViewModel Month { get; set; } = new();
}

public class WorkLogFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "تاریخ شمسی")]
    [Required(ErrorMessage = "تاریخ را وارد کنید.")]
    [PersianDate]
    public string PersianDate { get; set; } = string.Empty;

    [Display(Name = "ساعات کارکرد")]
    [Required(ErrorMessage = "ساعات کارکرد را وارد کنید.")]
    [Range(typeof(decimal), "0.01", "24", ErrorMessage = "ساعات کاری باید بین ۰٫۰۱ تا ۲۴ باشد.")]
    public decimal Hours { get; set; }

    [Display(Name = "توضیحات")]
    [MaxLength(500, ErrorMessage = "توضیحات حداکثر ۵۰۰ کاراکتر است.")]
    public string? Description { get; set; }
}

public class WorkLogRowViewModel
{
    public int Id { get; set; }
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public decimal HourlyRateUsed { get; set; }
    public decimal Income { get; set; }
    public string? Description { get; set; }
}

public class WorkLogListViewModel
{
    public string Filter { get; set; } = "month";
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime WeekStart { get; set; }
    public string RangeLabel { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal TotalIncome { get; set; }
    public List<WorkLogRowViewModel> Items { get; set; } = [];
    public List<(int Year, int Month, string Title)> MonthOptions { get; set; } = [];
    public List<(DateTime WeekStart, string Label)> WeekOptions { get; set; } = [];
}

public class WeeklyPlanPageViewModel
{
    public DateTime WeekStart { get; set; }

    [Display(Name = "ساعات هدف هفته")]
    [Range(typeof(decimal), "0.01", "168", ErrorMessage = "ساعات هدف هفته باید بین ۰٫۰۱ تا ۱۶۸ باشد.")]
    public decimal? TargetHours { get; set; }

    public bool GoalExists { get; set; }
    public PeriodReportViewModel Report { get; set; } = new();
    public List<(DateTime WeekStart, string Label)> WeekOptions { get; set; } = [];
}

public class MonthlyGoalPageViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }

    [Display(Name = "ساعات هدف ماه")]
    [Range(typeof(decimal), "0.01", "744", ErrorMessage = "ساعات هدف ماه باید بین ۰٫۰۱ تا ۷۴۴ باشد.")]
    public decimal? TargetHours { get; set; }

    public bool GoalExists { get; set; }
    public PeriodReportViewModel Report { get; set; } = new();
    public List<(int Year, int Month, string Title)> MonthOptions { get; set; } = [];
}
