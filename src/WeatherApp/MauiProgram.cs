using Microsoft.Extensions.Logging;
using WeatherApp.Core.DependencyInjection;
using WeatherApp.Core.Services;
using WeatherApp.Handlers;
using WeatherApp.Services;
using WeatherApp.Views;

namespace WeatherApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        EntryHandlerCustomizations.Apply();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddWeatherCore();

        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<IWeatherCache, PreferencesWeatherCache>();

        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ResultsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
