using System.Globalization;
using WeatherApp.Core.Dtos;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.Mapping;

internal static class WeatherMapper
{
    private const string DailyDateFormat = "yyyy-MM-dd";

    public static GeocodingResult? ToGeocodingResult(GeocodingResponseDto dto)
    {
        var match = dto.Results is { Count: > 0 } results ? results[0] : null;
        if (match is null || string.IsNullOrWhiteSpace(match.Name))
        {
            return null;
        }

        return new GeocodingResult(
            Name: match.Name,
            Latitude: match.Latitude,
            Longitude: match.Longitude,
            Country: match.Country,
            Admin1: match.Admin1);
    }

    public static WeatherResult ToWeatherResult(ForecastResponseDto dto)
    {
        if (dto.Current is not { } current)
        {
            throw new InvalidOperationException("Forecast response is missing the 'current' section.");
        }

        var currentWeather = new CurrentWeather(
            Temperature: current.Temperature,
            ApparentTemperature: current.ApparentTemperature,
            WeatherCode: current.WeatherCode,
            Description: WeatherCodeDescriptions.Describe(current.WeatherCode),
            RelativeHumidity: current.RelativeHumidity,
            WindSpeed: current.WindSpeed,
            IsDay: current.IsDay == 1);

        return new WeatherResult(currentWeather, ZipDaily(dto.Daily));
    }

    private static IReadOnlyList<DailyForecast> ZipDaily(DailyForecastDto? daily)
    {
        if (daily?.Time is not { Length: > 0 } times
            || daily.WeatherCode is not { } codes
            || daily.TemperatureMax is not { } maxima
            || daily.TemperatureMin is not { } minima)
        {
            return [];
        }

        var count = Math.Min(times.Length, Math.Min(codes.Length, Math.Min(maxima.Length, minima.Length)));
        var days = new List<DailyForecast>(count);
        for (var i = 0; i < count; i++)
        {
            var date = DateOnly.ParseExact(times[i], DailyDateFormat, CultureInfo.InvariantCulture);
            days.Add(new DailyForecast(
                Date: date,
                WeatherCode: codes[i],
                Description: WeatherCodeDescriptions.Describe(codes[i]),
                TemperatureMax: maxima[i],
                TemperatureMin: minima[i]));
        }

        return days;
    }
}
