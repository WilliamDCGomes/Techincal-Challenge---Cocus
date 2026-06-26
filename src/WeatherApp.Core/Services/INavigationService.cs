using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface INavigationService
{
    Task GoToResultsAsync(GeocodingResult location);
}
