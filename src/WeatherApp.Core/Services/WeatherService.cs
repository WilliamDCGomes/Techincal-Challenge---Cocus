using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Dtos;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Mapping;
using WeatherApp.Core.Models;
using WeatherApp.Core.Serialization;

namespace WeatherApp.Core.Services;

public sealed class WeatherService : IWeatherService
{
    public const string BaseAddress = "https://api.open-meteo.com/v1/";

    private const int ForecastDays = 7;
    private const string CurrentFields =
        "temperature_2m,weather_code,apparent_temperature,relative_humidity_2m,wind_speed_10m,is_day";
    private const string DailyFields = "weather_code,temperature_2m_max,temperature_2m_min";

    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherResult> GetForecastAsync(Coordinate coordinate, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildForecastUri(coordinate);

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        ForecastResponseDto? dto;
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            dto = await JsonSerializer
                .DeserializeAsync(stream, WeatherJsonContext.Default.ForecastResponseDto, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            response.EnsureSuccessStatusCode();
            _logger.LogWarning(ex, "Failed to parse forecast response.");
            throw new WeatherException(ErrorKind.InvalidResponse, "The weather response was not valid JSON.", ex);
        }

        if (dto?.Error == true)
        {
            throw new WeatherException(
                ErrorKind.InvalidResponse,
                dto.Reason ?? "The weather API returned an error.");
        }

        response.EnsureSuccessStatusCode();

        if (dto is null)
        {
            throw new WeatherException(ErrorKind.InvalidResponse, "The weather response was empty.");
        }

        try
        {
            return WeatherMapper.ToWeatherResult(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Forecast response was missing required fields.");
            throw new WeatherException(ErrorKind.InvalidResponse, "The weather response was missing required fields.", ex);
        }
    }

    private static string BuildForecastUri(Coordinate coordinate)
    {
        var latitude = coordinate.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = coordinate.Longitude.ToString(CultureInfo.InvariantCulture);
        return $"forecast?latitude={latitude}&longitude={longitude}" +
               $"&current={CurrentFields}&daily={DailyFields}" +
               $"&timezone=auto&forecast_days={ForecastDays}";
    }
}
