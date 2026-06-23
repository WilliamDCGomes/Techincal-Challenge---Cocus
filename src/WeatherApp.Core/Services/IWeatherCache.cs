using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface IWeatherCache
{
    Task<CachedWeather?> GetAsync(GeocodingResult location, CancellationToken cancellationToken = default);

    Task SetAsync(GeocodingResult location, WeatherResult weather, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeocodingResult>> GetRecentLocationsAsync(CancellationToken cancellationToken = default);
}
