namespace WeatherApp.Core.Services;

public interface IThemeService
{
    bool IsDark { get; }

    void Apply();

    void Toggle();
}
