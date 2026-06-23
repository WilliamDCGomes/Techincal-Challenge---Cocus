namespace WeatherApp.Core.Models;

public sealed record WeatherResult(
    CurrentWeather Current,
    IReadOnlyList<DailyForecast> Daily);
