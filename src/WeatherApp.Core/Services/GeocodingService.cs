using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Dtos;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Mapping;
using WeatherApp.Core.Models;
using WeatherApp.Core.Serialization;

namespace WeatherApp.Core.Services;

public sealed class GeocodingService : IGeocodingService
{
    public const string BaseAddress = "https://geocoding-api.open-meteo.com/v1/";

    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GeocodingResult?> SearchAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return null;
        }

        var requestUri = BuildSearchUri(city.Trim());

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        GeocodingResponseDto? dto;
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            dto = await JsonSerializer
                .DeserializeAsync(stream, WeatherJsonContext.Default.GeocodingResponseDto, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse geocoding response for {City}.", city);
            throw new WeatherException(ErrorKind.InvalidResponse, "The geocoding response was not valid JSON.", ex);
        }

        if (dto is null)
        {
            throw new WeatherException(ErrorKind.InvalidResponse, "The geocoding response was empty.");
        }

        return WeatherMapper.ToGeocodingResult(dto);
    }

    private static string BuildSearchUri(string city) =>
        $"search?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
}
