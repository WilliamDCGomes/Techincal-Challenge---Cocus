using System.Text.Json.Serialization;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.Models;

public sealed record DailyForecast(
    DateOnly Date,
    int WeatherCode,
    string Description,
    double TemperatureMax,
    double TemperatureMin)
{
    [JsonIgnore]
    public string Icon => WeatherIcons.Glyph(WeatherCode, isDay: true);
}
