using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Tests.ViewModels;

public class ResultsViewModelTests
{
    private readonly IWeatherService _weather = Substitute.For<IWeatherService>();
    private readonly IWeatherCache _cache = Substitute.For<IWeatherCache>();
    private readonly IConnectivityService _connectivity = Substitute.For<IConnectivityService>();

    private static readonly GeocodingResult Lisbon =
        new("Lisbon", 38.71667, -9.13333, "Portugal", "Lisbon");

    private ResultsViewModel CreateViewModel(bool connected = true)
    {
        _connectivity.IsConnected.Returns(connected);
        return new ResultsViewModel(_weather, _cache, _connectivity);
    }

    private static WeatherResult SevenDayForecast()
    {
        var current = new CurrentWeather(24.3, 25.1, 2, "Partly cloudy", 55, 12.4, IsDay: true);
        var days = Enumerable.Range(0, 7)
            .Select(i => new DailyForecast(
                new DateOnly(2026, 6, 23).AddDays(i), 0, "Clear sky", 28 - i, 17 - i))
            .ToList();
        return new WeatherResult(current, days);
    }

    [Fact]
    public void Daily_BeforeLoad_IsEmpty_AndCurrentIsNull()
    {
        var vm = CreateViewModel();

        Assert.Empty(vm.Daily);
        Assert.Null(vm.Current);
    }

    [Fact]
    public async Task Load_Success_MapsCurrent_AndDisplaysExactlyFiveDays()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(SevenDayForecast());
        var vm = CreateViewModel();

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Success, vm.State);
        Assert.Equal(ResultsViewModel.DisplayedForecastDays, vm.Daily.Count);
        Assert.Equal(5, vm.Daily.Count);
        Assert.NotNull(vm.Current);
        Assert.Equal(24.3, vm.Current!.Temperature, precision: 3);
        Assert.Equal("Lisbon", vm.Location!.Name);
    }

    [Fact]
    public async Task Load_Forecast_ExcludesToday_AndStartsTomorrow()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(SevenDayForecast());
        var vm = CreateViewModel();

        await vm.LoadAsync(Lisbon);

        var today = new DateOnly(2026, 6, 23);
        Assert.Equal(today, vm.Today!.Date);
        Assert.Equal(today.AddDays(1), vm.Daily[0].Date);
        Assert.DoesNotContain(vm.Daily, day => day.Date == today);
    }

    [Fact]
    public async Task Load_NetworkError_SetsErrorNetwork_AndClearsBusy()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("down"));
        var vm = CreateViewModel();

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Error, vm.State);
        Assert.Equal(ErrorKind.Network, vm.ErrorKind);
        Assert.False(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Load_InvalidResponse_SetsErrorKindFromWeatherException()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new WeatherException(ErrorKind.InvalidResponse, "error:true"));
        var vm = CreateViewModel();

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Error, vm.State);
        Assert.Equal(ErrorKind.InvalidResponse, vm.ErrorKind);
    }

    [Fact]
    public async Task Load_TransitionsThroughLoadingToSuccess()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(SevenDayForecast());
        var vm = CreateViewModel();
        var states = new List<ViewState>();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.State))
            {
                states.Add(vm.State);
            }
        };

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Loading, states[0]);
        Assert.Equal(ViewState.Success, states[^1]);
    }

    [Fact]
    public async Task Load_TransitionsThroughLoadingToError_OnNetworkFailure()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("down"));
        var vm = CreateViewModel();
        var states = new List<ViewState>();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.State))
            {
                states.Add(vm.State);
            }
        };

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Loading, states[0]);
        Assert.Equal(ViewState.Error, states[^1]);
    }

    [Fact]
    public async Task Retry_AfterError_ReloadsSuccessfully()
    {
        var attempts = 0;
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                attempts++;
                return attempts == 1
                    ? throw new HttpRequestException("transient")
                    : Task.FromResult(SevenDayForecast());
            });
        var vm = CreateViewModel();

        await vm.LoadAsync(Lisbon);
        Assert.Equal(ViewState.Error, vm.State);

        await vm.RetryCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Success, vm.State);
        Assert.Equal(5, vm.Daily.Count);
    }

    [Fact]
    public async Task Refresh_ReloadsContent_AndResetsIsRefreshing()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(SevenDayForecast());
        var vm = CreateViewModel();
        await vm.LoadAsync(Lisbon);
        vm.IsRefreshing = true;

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.False(vm.IsRefreshing);
        Assert.Equal(ViewState.Success, vm.State);
        Assert.NotNull(vm.Weather);
    }

    [Fact]
    public async Task Load_Online_Success_WritesToCache()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(SevenDayForecast());
        var vm = CreateViewModel(connected: true);

        await vm.LoadAsync(Lisbon);

        Assert.False(vm.IsOffline);
        await _cache.Received(1).SetAsync(Lisbon, Arg.Any<WeatherResult>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Load_Offline_WithCache_ServesCached_AndFlagsOffline()
    {
        _cache.GetAsync(Lisbon, Arg.Any<CancellationToken>())
            .Returns(new CachedWeather(SevenDayForecast(), DateTimeOffset.UtcNow));
        var vm = CreateViewModel(connected: false);

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Success, vm.State);
        Assert.True(vm.IsOffline);
        Assert.Equal(5, vm.Daily.Count);
        await _weather.DidNotReceive().GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Load_Offline_WithoutCache_SetsOfflineError()
    {
        _cache.GetAsync(Arg.Any<GeocodingResult>(), Arg.Any<CancellationToken>())
            .Returns((CachedWeather?)null);
        var vm = CreateViewModel(connected: false);

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Error, vm.State);
        Assert.Equal(ErrorKind.Offline, vm.ErrorKind);
    }

    [Fact]
    public async Task Load_Online_RemoteFailure_IsSurfaced_NotMaskedByCache()
    {
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("down"));
        _cache.GetAsync(Arg.Any<GeocodingResult>(), Arg.Any<CancellationToken>())
            .Returns(new CachedWeather(SevenDayForecast(), DateTimeOffset.UtcNow));
        var vm = CreateViewModel(connected: true);

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ViewState.Error, vm.State);
        Assert.Equal(ErrorKind.Network, vm.ErrorKind);
        await _cache.DidNotReceive().GetAsync(Arg.Any<GeocodingResult>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reconnect_WhenOffline_AutoRefreshesFromNetwork()
    {
        _cache.GetAsync(Lisbon, Arg.Any<CancellationToken>())
            .Returns(new CachedWeather(SevenDayForecast(), DateTimeOffset.UtcNow));
        _weather.GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>())
            .Returns(SevenDayForecast());
        var vm = CreateViewModel(connected: false);
        await vm.LoadAsync(Lisbon);
        Assert.True(vm.IsOffline);

        _connectivity.IsConnected.Returns(true);
        _connectivity.ConnectivityChanged += Raise.Event<EventHandler>(_connectivity, EventArgs.Empty);

        Assert.False(vm.IsOffline);
        Assert.Equal(ViewState.Success, vm.State);
        await _weather.Received().GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Detach_StopsReactingToConnectivity()
    {
        _cache.GetAsync(Lisbon, Arg.Any<CancellationToken>())
            .Returns(new CachedWeather(SevenDayForecast(), DateTimeOffset.UtcNow));
        var vm = CreateViewModel(connected: false);
        await vm.LoadAsync(Lisbon);

        vm.Detach();
        _connectivity.IsConnected.Returns(true);
        _connectivity.ConnectivityChanged += Raise.Event<EventHandler>(_connectivity, EventArgs.Empty);

        Assert.True(vm.IsOffline);
        await _weather.DidNotReceive().GetForecastAsync(Arg.Any<Coordinate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Load_Offline_WithoutCache_ShowsSpecificUserMessage()
    {
        _cache.GetAsync(Arg.Any<GeocodingResult>(), Arg.Any<CancellationToken>())
            .Returns((CachedWeather?)null);
        var vm = CreateViewModel(connected: false);

        await vm.LoadAsync(Lisbon);

        Assert.Equal(ErrorKind.Offline, vm.ErrorKind);
        Assert.Contains("no saved forecast", vm.ErrorMessage!);
    }

    [Fact]
    public async Task Load_Offline_FreshCache_IsNotStale()
    {
        _cache.GetAsync(Lisbon, Arg.Any<CancellationToken>())
            .Returns(new CachedWeather(SevenDayForecast(), DateTimeOffset.UtcNow));
        var vm = CreateViewModel(connected: false);

        await vm.LoadAsync(Lisbon);

        Assert.True(vm.IsOffline);
        Assert.False(vm.IsStaleCache);
        Assert.Equal("Offline — showing the last saved forecast", vm.OfflineBannerText);
    }

    [Fact]
    public async Task Load_Offline_ExpiredCache_FlagsStale_AndUpdatesBanner()
    {
        _cache.GetAsync(Lisbon, Arg.Any<CancellationToken>())
            .Returns(new CachedWeather(SevenDayForecast(), DateTimeOffset.UtcNow.AddHours(-2)));
        var vm = CreateViewModel(connected: false);

        await vm.LoadAsync(Lisbon);

        Assert.True(vm.IsOffline);
        Assert.True(vm.IsStaleCache);
        Assert.Equal("Offline — showing an older saved forecast", vm.OfflineBannerText);
    }
}
