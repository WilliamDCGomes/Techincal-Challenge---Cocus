using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface IWeatherService
{
    Task<WeatherResult> GetForecastAsync(Coordinate coordinate, CancellationToken cancellationToken = default);
}
