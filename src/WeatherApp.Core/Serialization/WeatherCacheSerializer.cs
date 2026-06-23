using System.Text.Json;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Serialization;

public static class WeatherCacheSerializer
{
    public static string SerializeWeather(CachedWeather cached) =>
        JsonSerializer.Serialize(cached, CacheJsonContext.Default.CachedWeather);

    public static CachedWeather? DeserializeWeather(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(json, CacheJsonContext.Default.CachedWeather);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string SerializeRecents(IReadOnlyList<GeocodingResult> recents) =>
        JsonSerializer.Serialize([.. recents], CacheJsonContext.Default.ListGeocodingResult);

    public static IReadOnlyList<GeocodingResult> DeserializeRecents(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize(json, CacheJsonContext.Default.ListGeocodingResult) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
