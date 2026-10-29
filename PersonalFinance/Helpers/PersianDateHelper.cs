using System.Globalization;

namespace PersonalFinance.Helpers;

public static class PersianDateHelper
{
    private static readonly PersianCalendar Calendar = new();

    public static readonly string[] MonthNames =
    [
        "", "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    ];

    public static readonly string[] WeekdayNames =
    [
        "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه"
    ];

    public static (int Year, int Month, int Day) ToPersian(DateTime date)
    {
        date = date.Date;
        return (Calendar.GetYear(date), Calendar.GetMonth(date), Calendar.GetDayOfMonth(date));
    }

    public static DateTime ToGregorian(int year, int month, int day)
        => Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);

    public static bool IsLeapYear(int year) => Calendar.IsLeapYear(year);

    public static int GetDaysInMonth(int year, int month) => Calendar.GetDaysInMonth(year, month);

    public static string ToShamsi(DateTime date)
    {
        var (year, month, day) = ToPersian(date);
        return $"{year:0000}/{month:00}/{day:00}";
    }

    public static string ToShamsiDisplay(DateTime date) => FormatHelper.ToPersianDigits(ToShamsi(date));

    public static string MonthTitle(int year, int month)
        => $"{MonthNames[month]} {FormatHelper.ToPersianDigits(year.ToString(CultureInfo.InvariantCulture))}";

    public static int WeekdayIndex(DateTime date) => ((int)date.DayOfWeek + 1) % 7;

    public static string WeekdayName(DateTime date) => WeekdayNames[WeekdayIndex(date)];

    public static DateTime GetWeekStart(DateTime date)
    {
        date = date.Date;
        return date.AddDays(-WeekdayIndex(date));
    }

    public static DateTime GetWeekEndExclusive(DateTime date) => GetWeekStart(date).AddDays(7);

    public static DateTime GetMonthStart(int year, int month) => ToGregorian(year, month, 1);

    public static DateTime GetMonthStart(DateTime date)
    {
        var (year, month, _) = ToPersian(date);
        return GetMonthStart(year, month);
    }

    public static DateTime GetMonthEndExclusive(int year, int month)
    {
        var (nextYear, nextMonth) = AddMonths(year, month, 1);
        return GetMonthStart(nextYear, nextMonth);
    }

    public static DateTime GetMonthEndExclusive(DateTime date)
    {
        var (year, month, _) = ToPersian(date);
        return GetMonthEndExclusive(year, month);
    }

    public static (int Year, int Month) AddMonths(int year, int month, int delta)
    {
        var total = year * 12 + (month - 1) + delta;
        var y = Math.DivRem(total, 12, out var m);
        if (m < 0)
        {
            y--;
            m += 12;
        }

        return (y, m + 1);
    }

    public static bool TryParse(string? input, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = FormatHelper.ToEnglishDigits(input.Trim())
            .Replace('-', '/')
            .Replace('.', '/')
            .Replace('\\', '/');

        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var month) ||
            !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var day))
        {
            return false;
        }

        if (year is < 1200 or > 1600 || month is < 1 or > 12)
        {
            return false;
        }

        var daysInMonth = GetDaysInMonth(year, month);
        if (day < 1 || day > daysInMonth)
        {
            return false;
        }

        try
        {
            date = ToGregorian(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static string WeekRangeLabel(DateTime anyDayInWeek)
    {
        var start = GetWeekStart(anyDayInWeek);
        var end = start.AddDays(6);
        return $"{WeekdayName(start)} {ToShamsiDisplay(start)} تا {WeekdayName(end)} {ToShamsiDisplay(end)}";
    }

    public static string FullDateLabel(DateTime date)
        => $"{WeekdayName(date)} {ToShamsiDisplay(date)}";

    public static List<(int Year, int Month, string Title)> GetMonthOptions(DateTime around, int back, int forward)
    {
        var (year, month, _) = ToPersian(around);
        var list = new List<(int, int, string)>();
        for (var i = -back; i <= forward; i++)
        {
            var (y, m) = AddMonths(year, month, i);
            list.Add((y, m, MonthTitle(y, m)));
        }

        return list;
    }

    public static List<(DateTime WeekStart, string Label)> GetWeekOptions(DateTime around, int back, int forward)
    {
        var current = GetWeekStart(around);
        var list = new List<(DateTime, string)>();
        for (var i = -back; i <= forward; i++)
        {
            var start = current.AddDays(i * 7);
            list.Add((start, WeekRangeLabel(start)));
        }

        return list;
    }
}
