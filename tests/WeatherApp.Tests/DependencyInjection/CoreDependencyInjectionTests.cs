using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using WeatherApp.Core.DependencyInjection;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Tests.DependencyInjection;

public class CoreDependencyInjectionTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddWeatherCore();
        services.AddSingleton(Substitute.For<INavigationService>());
        services.AddSingleton(Substitute.For<IWeatherCache>());
        services.AddSingleton(Substitute.For<IConnectivityService>());
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Resolves_SearchViewModel_WithDependencies()
    {
        using var provider = BuildProvider();
        Assert.NotNull(provider.GetRequiredService<SearchViewModel>());
    }

    [Fact]
    public void Resolves_ResultsViewModel_WithDependencies()
    {
        using var provider = BuildProvider();
        Assert.NotNull(provider.GetRequiredService<ResultsViewModel>());
    }

    [Fact]
    public void Resolves_TypedGeocodingService()
    {
        using var provider = BuildProvider();
        Assert.IsType<GeocodingService>(provider.GetRequiredService<IGeocodingService>());
    }

    [Fact]
    public void Resolves_TypedWeatherService()
    {
        using var provider = BuildProvider();
        Assert.IsType<WeatherService>(provider.GetRequiredService<IWeatherService>());
    }
}
