using Microsoft.Maui.Storage;
using WeatherApp.Core.Services;

namespace WeatherApp.Services;

public sealed class ThemeService : IThemeService
{
    private const string PreferenceKey = "app_theme";

    private readonly IPreferences _preferences;

    public ThemeService(IPreferences preferences) => _preferences = preferences;

    public bool IsDark => ResolvedTheme() == AppTheme.Dark;

    public void Apply()
    {
        var saved = _preferences.Get(PreferenceKey, string.Empty);
        if (saved == "dark")
        {
            SetTheme(AppTheme.Dark);
        }
        else if (saved == "light")
        {
            SetTheme(AppTheme.Light);
        }
    }

    public void Toggle()
    {
        var next = IsDark ? AppTheme.Light : AppTheme.Dark;
        _preferences.Set(PreferenceKey, next == AppTheme.Dark ? "dark" : "light");
        SetTheme(next);
    }

    private static AppTheme ResolvedTheme()
    {
        if (Application.Current is not { } app)
        {
            return AppTheme.Light;
        }

        return app.UserAppTheme == AppTheme.Unspecified ? app.RequestedTheme : app.UserAppTheme;
    }

    private static void SetTheme(AppTheme theme)
    {
        if (Application.Current is { } app)
        {
            app.UserAppTheme = theme;
        }
    }
}
