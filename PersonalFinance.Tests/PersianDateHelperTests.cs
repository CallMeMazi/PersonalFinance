using PersonalFinance.Helpers;

namespace PersonalFinance.Tests;

public class PersianDateHelperTests
{
    [Fact]
    public void Farvardin1404_MapsTo_21March2025()
    {
        var date = PersianDateHelper.ToGregorian(1404, 1, 1);
        Assert.Equal(new DateTime(2025, 3, 21), date);
        Assert.Equal("1404/01/01", PersianDateHelper.ToShamsi(date));
    }

    [Fact]
    public void WeekStartsOnSaturday_AndEndsOnFriday()
    {
        var friday = new DateTime(2025, 3, 21);
        var start = PersianDateHelper.GetWeekStart(friday);
        var endExclusive = PersianDateHelper.GetWeekEndExclusive(friday);

        Assert.Equal(DayOfWeek.Saturday, start.DayOfWeek);
        Assert.Equal(new DateTime(2025, 3, 15), start);
        Assert.Equal(new DateTime(2025, 3, 22), endExclusive);
        Assert.Equal(DayOfWeek.Friday, endExclusive.AddDays(-1).DayOfWeek);
    }

    [Fact]
    public void Saturday_IsItsOwnWeekStart()
    {
        var saturday = new DateTime(2025, 3, 15);
        Assert.Equal(saturday, PersianDateHelper.GetWeekStart(saturday));
    }

    [Fact]
    public void LeapYearEsfand_HasThirtyDays()
    {
        Assert.True(PersianDateHelper.IsLeapYear(1403));
        Assert.Equal(30, PersianDateHelper.GetDaysInMonth(1403, 12));
        Assert.True(PersianDateHelper.TryParse("1403/12/30", out var date));
        Assert.Equal(12, PersianDateHelper.ToPersian(date).Month);
        Assert.Equal(30, PersianDateHelper.ToPersian(date).Day);
    }

    [Fact]
    public void NonLeapEsfand_RejectsDay30()
    {
        Assert.False(PersianDateHelper.IsLeapYear(1404));
        Assert.Equal(29, PersianDateHelper.GetDaysInMonth(1404, 12));
        Assert.False(PersianDateHelper.TryParse("1404/12/30", out _));
        Assert.False(PersianDateHelper.TryParse("1404/13/01", out _));
        Assert.False(PersianDateHelper.TryParse("abc", out _));
    }

    [Fact]
    public void ParsesPersianDigits()
    {
        Assert.True(PersianDateHelper.TryParse("۱۴۰۴/۰۸/۱۰", out var date));
        var persian = PersianDateHelper.ToPersian(date);
        Assert.Equal(1404, persian.Year);
        Assert.Equal(8, persian.Month);
        Assert.Equal(10, persian.Day);
    }

    [Fact]
    public void MonthBoundaries_UsePersianCalendar()
    {
        var start = PersianDateHelper.GetMonthStart(1404, 1);
        var end = PersianDateHelper.GetMonthEndExclusive(1404, 1);
        Assert.Equal(new DateTime(2025, 3, 21), start);
        Assert.Equal(PersianDateHelper.GetMonthStart(1404, 2), end);
        Assert.Equal(31, (end - start).Days);
    }
}
