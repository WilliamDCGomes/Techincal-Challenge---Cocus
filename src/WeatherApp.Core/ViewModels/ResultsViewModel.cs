using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class ResultsViewModel : BaseViewModel
{
    public const string GeoQueryKey = "geo";

    public const int DisplayedForecastDays = 5;

    private const double FeelsLikeThresholdCelsius = 1.0;

    private readonly IWeatherService _weatherService;
    private readonly IWeatherCache _cache;
    private readonly IConnectivityService _connectivity;

    public ResultsViewModel(
        IWeatherService weatherService,
        IWeatherCache cache,
        IConnectivityService connectivity)
    {
        _weatherService = weatherService;
        _cache = cache;
        _connectivity = connectivity;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    [ObservableProperty]
    public partial GeocodingResult? Location { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(Current), nameof(Daily), nameof(Today), nameof(IsForecastEmpty),
        nameof(HasFeelsLike), nameof(HighLowText), nameof(FeelsLikeText))]
    public partial WeatherResult? Weather { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial bool IsOffline { get; set; }

    public CurrentWeather? Current => Weather?.Current;

    public IReadOnlyList<DailyForecast> Daily { get; private set; } = [];

    public DailyForecast? Today => Daily.Count > 0 ? Daily[0] : null;

    public bool IsForecastEmpty => Weather is not null && Daily.Count == 0;

    public bool HasFeelsLike =>
        Current is not null && Math.Abs(Current.ApparentTemperature - Current.Temperature) >= FeelsLikeThresholdCelsius;

    public string HighLowText =>
        Today is null ? string.Empty : $"H:{Today.TemperatureMax:0}°  L:{Today.TemperatureMin:0}°";

    public string FeelsLikeText =>
        Current is null ? string.Empty : $"Feels like {Current.ApparentTemperature:0}°";

    partial void OnWeatherChanged(WeatherResult? value) =>
        Daily = value is null ? [] : [.. value.Daily.Take(DisplayedForecastDays)];

    public Task LoadAsync(GeocodingResult location, CancellationToken cancellationToken = default)
    {
        Location = location;
        return LoadForecastAsync(showLoading: true, cancellationToken);
    }

    public void Detach() => _connectivity.ConnectivityChanged -= OnConnectivityChanged;

    [RelayCommand]
    private Task RetryAsync(CancellationToken cancellationToken) =>
        Location is null ? Task.CompletedTask : LoadForecastAsync(showLoading: true, cancellationToken);

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (Location is not null)
            {
                await LoadForecastAsync(showLoading: false, cancellationToken);
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private Task LoadForecastAsync(bool showLoading, CancellationToken cancellationToken) =>
        RunGuardedAsync(LoadForecastCoreAsync, showLoading, cancellationToken);

    private async Task<ViewState> LoadForecastCoreAsync(CancellationToken cancellationToken)
    {
        var location = Location!;

        if (!_connectivity.IsConnected)
        {
            var cached = await _cache.GetAsync(location, cancellationToken);
            if (cached is null)
            {
                throw new WeatherException(ErrorKind.Offline, "You're offline and there's no saved forecast for this city yet.");
            }

            Weather = cached.Weather;
            IsOffline = true;
            return ViewState.Success;
        }

        var weather = await _weatherService.GetForecastAsync(location.Coordinate, cancellationToken);
        Weather = weather;
        IsOffline = false;
        await _cache.SetAsync(location, weather, cancellationToken);
        return ViewState.Success;
    }

    private async void OnConnectivityChanged(object? sender, EventArgs e)
    {
        if (_connectivity.IsConnected && IsOffline && Location is not null)
        {
            await LoadForecastAsync(showLoading: false, CancellationToken.None);
        }
    }
}
