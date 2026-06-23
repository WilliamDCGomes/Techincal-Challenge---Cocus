namespace WeatherApp.Core.Models;

public sealed record DailyForecast(
    DateOnly Date,
    int WeatherCode,
    string Description,
    double TemperatureMax,
    double TemperatureMin);
