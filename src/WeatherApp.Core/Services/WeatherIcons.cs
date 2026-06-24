namespace WeatherApp.Core.Services;

public static class WeatherIcons
{
    public static string Glyph(int weatherCode, bool isDay)
    {
        if (!isDay)
        {
            switch (weatherCode)
            {
                case 0:
                case 1:
                    return "🌙";
                case 2:
                    return "☁️";
            }
        }

        return weatherCode switch
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
            _ => "🌡️",
        };
    }
}
