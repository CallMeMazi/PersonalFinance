using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.Helpers;

public sealed class PersianDateAttribute : ValidationAttribute
{
    public PersianDateAttribute()
    {
        ErrorMessage = "تاریخ شمسی نامعتبر است. قالب درست مانند ۱۴۰۴/۰۸/۱۰ است.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        return PersianDateHelper.TryParse(text, out _);
    }
}
