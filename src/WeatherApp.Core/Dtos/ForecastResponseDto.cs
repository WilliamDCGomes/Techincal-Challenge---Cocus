using System.Text.Json.Serialization;

namespace WeatherApp.Core.Dtos;

internal sealed class ForecastResponseDto
{
    [JsonPropertyName("error")]
    public bool? Error { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }

    [JsonPropertyName("current")]
    public CurrentWeatherDto? Current { get; init; }

    [JsonPropertyName("daily")]
    public DailyForecastDto? Daily { get; init; }
}

internal sealed class CurrentWeatherDto
{
    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("interval")]
    public int Interval { get; init; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; init; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; init; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; init; }

    [JsonPropertyName("relative_humidity_2m")]
    public int RelativeHumidity { get; init; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; init; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; init; }
}

internal sealed class DailyForecastDto
{
    [JsonPropertyName("time")]
    public string[]? Time { get; init; }

    [JsonPropertyName("weather_code")]
    public int[]? WeatherCode { get; init; }

    [JsonPropertyName("temperature_2m_max")]
    public double[]? TemperatureMax { get; init; }

    [JsonPropertyName("temperature_2m_min")]
    public double[]? TemperatureMin { get; init; }
}
