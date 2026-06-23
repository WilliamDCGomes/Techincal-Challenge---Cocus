using System.Globalization;
using WeatherApp.Core.Models;
using WeatherApp.Core.Serialization;
using WeatherApp.Core.Services;

namespace WeatherApp.Services;

public sealed class PreferencesWeatherCache : IWeatherCache
{
    private const string RecentsKey = "weather.recents";
    private const int MaxRecents = 6;

    private readonly IPreferences _preferences;

    public PreferencesWeatherCache(IPreferences preferences) => _preferences = preferences;

    public Task<CachedWeather?> GetAsync(GeocodingResult location, CancellationToken cancellationToken = default)
    {
        var json = _preferences.Get(WeatherKey(location), string.Empty);
        return Task.FromResult(WeatherCacheSerializer.DeserializeWeather(json));
    }

    public Task SetAsync(GeocodingResult location, WeatherResult weather, CancellationToken cancellationToken = default)
    {
        var cached = new CachedWeather(weather, DateTimeOffset.UtcNow);
        _preferences.Set(WeatherKey(location), WeatherCacheSerializer.SerializeWeather(cached));
        UpdateRecents(location);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GeocodingResult>> GetRecentLocationsAsync(CancellationToken cancellationToken = default)
    {
        var json = _preferences.Get(RecentsKey, string.Empty);
        return Task.FromResult(WeatherCacheSerializer.DeserializeRecents(json));
    }

    private void UpdateRecents(GeocodingResult location)
    {
        var recents = WeatherCacheSerializer
            .DeserializeRecents(_preferences.Get(RecentsKey, string.Empty))
            .ToList();

        recents.RemoveAll(r => SameLocation(r, location));
        recents.Insert(0, location);
        if (recents.Count > MaxRecents)
        {
            recents = recents.Take(MaxRecents).ToList();
        }

        _preferences.Set(RecentsKey, WeatherCacheSerializer.SerializeRecents(recents));
    }

    private static bool SameLocation(GeocodingResult a, GeocodingResult b) =>
        a.Latitude == b.Latitude && a.Longitude == b.Longitude;

    private static string WeatherKey(GeocodingResult location) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"weather.{location.Latitude:F4}_{location.Longitude:F4}");
}
