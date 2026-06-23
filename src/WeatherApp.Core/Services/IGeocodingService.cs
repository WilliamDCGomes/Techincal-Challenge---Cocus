using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface IGeocodingService
{
    Task<GeocodingResult?> SearchAsync(string city, CancellationToken cancellationToken = default);
}
