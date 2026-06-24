using System.Text.Json.Serialization;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.Models;

public sealed record CurrentWeather(
    double Temperature,
    double ApparentTemperature,
    int WeatherCode,
    string Description,
    int RelativeHumidity,
    double WindSpeed,
    bool IsDay)
{
    [JsonIgnore]
    public string Icon => WeatherIcons.Glyph(WeatherCode, IsDay);
}
