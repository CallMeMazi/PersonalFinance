using System.Globalization;

namespace PersonalFinance.Helpers;

public static class FormatHelper
{
    private static readonly char[] PersianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
    private static readonly char[] ArabicIndicDigits = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];

    public static string ToPersianDigits(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= '0' and <= '9')
            {
                chars[i] = PersianDigits[chars[i] - '0'];
            }
        }

        return new string(chars);
    }

    public static string ToEnglishDigits(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            for (var d = 0; d < 10; d++)
            {
                if (c == PersianDigits[d] || c == ArabicIndicDigits[d])
                {
                    chars[i] = (char)('0' + d);
                    break;
                }
            }

            if (chars[i] == '٫')
            {
                chars[i] = '.';
            }
        }

        return new string(chars);
    }

    public static string Toman(decimal value)
    {
        var formatted = value.ToString("#,0.##", CultureInfo.InvariantCulture).Replace(",", "٬");
        return ToPersianDigits(formatted);
    }

    public static string TomanWithUnit(decimal value) => $"{Toman(value)} تومان";

    public static string Hours(decimal value)
        => ToPersianDigits(value.ToString("0.##", CultureInfo.InvariantCulture).Replace(".", "٫"));

    public static string Percent(decimal value)
        => $"{ToPersianDigits(value.ToString("0.#", CultureInfo.InvariantCulture).Replace(".", "٫"))}٪";

    public static string SignedToman(decimal value)
    {
        var sign = value > 0 ? "+" : string.Empty;
        return sign + TomanWithUnit(value);
    }
}
