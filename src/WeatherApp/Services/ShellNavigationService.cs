using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Services;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToResultsAsync(GeocodingResult location) =>
        Shell.Current.GoToAsync(Routes.Results, new ShellNavigationQueryParameters
        {
            [ResultsViewModel.GeoQueryKey] = location,
        });
}
