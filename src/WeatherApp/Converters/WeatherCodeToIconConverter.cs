using System.Globalization;

namespace WeatherApp.Converters;

public sealed class WeatherCodeToIconConverter : IValueConverter
{
    private const string Fallback = "🌡️";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int code)
        {
            return Fallback;
        }

        return code switch
        {
            0 => "☀️",
            1 => "🌤️",
            2 => "⛅",
            3 => "☁️",
            45 or 48 => "🌫️",
            51 or 53 or 55 => "🌦️",
            56 or 57 => "🌧️",
            61 or 63 or 65 => "🌧️",
            66 or 67 => "🌧️",
            71 or 73 or 75 or 77 => "🌨️",
            80 or 81 or 82 => "🌧️",
            85 or 86 => "🌨️",
            95 => "⛈️",
            96 or 99 => "⛈️",
            _ => Fallback,
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(WeatherCodeToIconConverter)} is one-way only.");
}
