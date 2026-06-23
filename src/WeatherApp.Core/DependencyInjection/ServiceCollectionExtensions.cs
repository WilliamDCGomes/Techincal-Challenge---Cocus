using Microsoft.Extensions.DependencyInjection;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherCore(this IServiceCollection services)
    {
        services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
                client.BaseAddress = new Uri(GeocodingService.BaseAddress))
            .AddStandardResilienceHandler();

        services.AddHttpClient<IWeatherService, WeatherService>(client =>
                client.BaseAddress = new Uri(WeatherService.BaseAddress))
            .AddStandardResilienceHandler();

        services.AddTransient<SearchViewModel>();
        services.AddTransient<ResultsViewModel>();

        return services;
    }
}
