using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Tests.ViewModels;

public class SearchViewModelTests
{
    private readonly IGeocodingService _geocoding = Substitute.For<IGeocodingService>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();
    private readonly IWeatherCache _cache = Substitute.For<IWeatherCache>();
    private readonly IThemeService _theme = Substitute.For<IThemeService>();

    private static readonly GeocodingResult Lisbon =
        new("Lisbon", 38.71667, -9.13333, "Portugal", "Lisbon");

    private SearchViewModel CreateViewModel() =>
        new(_geocoding, _navigation, _cache, _theme, NullLogger<SearchViewModel>.Instance);

    [Fact]
    public void SearchCommand_IsDisabled_WhenCityIsBlank()
    {
        var vm = CreateViewModel();

        vm.City = "   ";

        Assert.False(vm.SearchCommand.CanExecute(null));
    }

    [Fact]
    public void SearchCommand_IsEnabled_WhenCityIsProvided()
    {
        var vm = CreateViewModel();

        vm.City = "Lisbon";

        Assert.True(vm.SearchCommand.CanExecute(null));
    }

    [Fact]
    public async Task Search_ValidCity_NavigatesAndSetsSuccess()
    {
        _geocoding.SearchAsync("Lisbon", Arg.Any<CancellationToken>()).Returns(Lisbon);
        var vm = CreateViewModel();
        vm.City = "Lisbon";

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Success, vm.State);
        await _navigation.Received(1).GoToResultsAsync(Lisbon);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Search_NoResult_SetsEmpty_AndDoesNotNavigate()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GeocodingResult?)null);
        var vm = CreateViewModel();
        vm.City = "asdfghjkl";

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Empty, vm.State);
        await _navigation.DidNotReceive().GoToResultsAsync(Arg.Any<GeocodingResult>());
    }

    [Fact]
    public async Task Search_NetworkError_SetsErrorNetwork_AndClearsBusy()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("boom"));
        var vm = CreateViewModel();
        vm.City = "Lisbon";

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Error, vm.State);
        Assert.Equal(ErrorKind.Network, vm.ErrorKind);
        Assert.False(string.IsNullOrEmpty(vm.ErrorMessage));
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Search_InvalidResponse_SetsErrorKindFromWeatherException()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new WeatherException(ErrorKind.InvalidResponse, "bad json"));
        var vm = CreateViewModel();
        vm.City = "Lisbon";

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Error, vm.State);
        Assert.Equal(ErrorKind.InvalidResponse, vm.ErrorKind);
    }

    [Fact]
    public async Task Search_TransitionsThroughLoadingToSuccess()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Lisbon);
        var vm = CreateViewModel();
        vm.City = "Lisbon";
        var states = new List<ViewState>();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.State))
            {
                states.Add(vm.State);
            }
        };

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Loading, states[0]);
        Assert.Equal(ViewState.Success, states[^1]);
    }

    [Fact]
    public async Task Search_TransitionsThroughLoadingToError_OnNetworkFailure()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("boom"));
        var vm = CreateViewModel();
        vm.City = "Lisbon";
        var states = new List<ViewState>();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BaseViewModel.State))
            {
                states.Add(vm.State);
            }
        };

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Loading, states[0]);
        Assert.Equal(ViewState.Error, states[^1]);
    }

    [Fact]
    public async Task SearchCommand_IsDisabledWhileRunning_AndReEnabledAfter()
    {
        var gate = new TaskCompletionSource<GeocodingResult?>();
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(gate.Task);
        var vm = CreateViewModel();
        vm.City = "Lisbon";

        Assert.True(vm.SearchCommand.CanExecute(null));
        var execution = vm.SearchCommand.ExecuteAsync(null);
        Assert.False(vm.SearchCommand.CanExecute(null));

        gate.SetResult(null);
        await execution;

        Assert.False(vm.IsBusy);
        Assert.True(vm.SearchCommand.CanExecute(null));
    }

    [Fact]
    public async Task LoadRecents_PopulatesFromCache()
    {
        _cache.GetRecentLocationsAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { Lisbon });
        var vm = CreateViewModel();

        await vm.LoadRecentsAsync();

        Assert.Single(vm.Recents);
        Assert.Equal("Lisbon", vm.Recents[0].Name);
        Assert.True(vm.HasRecents);
    }

    [Fact]
    public async Task PrepareAsync_ResetsToIdle_AndLoadsRecents()
    {
        _geocoding.SearchAsync("Lisbon", Arg.Any<CancellationToken>()).Returns(Lisbon);
        _cache.GetRecentLocationsAsync(Arg.Any<CancellationToken>()).Returns(new[] { Lisbon });
        var vm = CreateViewModel();
        vm.City = "Lisbon";
        await vm.SearchCommand.ExecuteAsync(null);
        Assert.Equal(ViewState.Success, vm.State);

        await vm.PrepareAsync();

        Assert.Equal(ViewState.Idle, vm.State);
        Assert.Contains(vm.Recents, r => r.Name == "Lisbon");
    }

    [Fact]
    public void ThemeGlyph_ShowsCurrentMode_MoonInDark_SunInLight()
    {
        _theme.IsDark.Returns(true);
        Assert.Equal("🌙", CreateViewModel().ThemeGlyph);

        _theme.IsDark.Returns(false);
        Assert.Equal("☀️", CreateViewModel().ThemeGlyph);
    }

    [Fact]
    public void ToggleTheme_DelegatesToThemeService_AndNotifiesGlyph()
    {
        var vm = CreateViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.ToggleThemeCommand.Execute(null);

        _theme.Received(1).Toggle();
        Assert.Contains(nameof(SearchViewModel.ThemeGlyph), changed);
    }

    [Fact]
    public async Task EditingCity_AfterNoMatch_ClearsTheStaleResult()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GeocodingResult?)null);
        var vm = CreateViewModel();
        vm.City = "Lisbo";
        await vm.SearchCommand.ExecuteAsync(null);
        Assert.Equal(ViewState.Empty, vm.State);

        vm.City = "Lisbon";

        Assert.Equal(ViewState.Idle, vm.State);
    }

    [Fact]
    public async Task EditingCity_AfterError_ClearsTheStaleResult()
    {
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("down"));
        var vm = CreateViewModel();
        vm.City = "Lisbon";
        await vm.SearchCommand.ExecuteAsync(null);
        Assert.Equal(ViewState.Error, vm.State);

        vm.City = "Madrid";

        Assert.Equal(ViewState.Idle, vm.State);
    }

    [Fact]
    public async Task OpenRecent_NavigatesDirectly_WithoutGeocoding()
    {
        var porto = new GeocodingResult("Porto", 41.15, -8.61, "Portugal", "Porto");
        var vm = CreateViewModel();

        await vm.OpenRecentCommand.ExecuteAsync(porto);

        await _navigation.Received(1).GoToResultsAsync(porto);
        await _geocoding.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_Retry_AfterError_SucceedsOnSecondAttempt()
    {
        var attempts = 0;
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                attempts++;
                return attempts == 1
                    ? throw new HttpRequestException("transient")
                    : Task.FromResult<GeocodingResult?>(Lisbon);
            });
        var vm = CreateViewModel();
        vm.City = "Lisbon";

        await vm.SearchCommand.ExecuteAsync(null);
        Assert.Equal(ViewState.Error, vm.State);

        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Success, vm.State);
        await _navigation.Received(1).GoToResultsAsync(Lisbon);
    }

    [Fact]
    public async Task LoadRecents_Reorder_UpdatesOrderInPlace_PreservingItems()
    {
        var london = new GeocodingResult("London", 51.5, -0.12, "United Kingdom", "England");
        var paris = new GeocodingResult("Paris", 48.85, 2.35, "France", "Île-de-France");
        var vm = CreateViewModel();

        _cache.GetRecentLocationsAsync(Arg.Any<CancellationToken>()).Returns(new[] { london, paris });
        await vm.LoadRecentsAsync();
        Assert.Equal(new[] { "London", "Paris" }, vm.Recents.Select(r => r.Name));

        _cache.GetRecentLocationsAsync(Arg.Any<CancellationToken>()).Returns(new[] { paris, london });
        await vm.LoadRecentsAsync();

        Assert.Equal(new[] { "Paris", "London" }, vm.Recents.Select(r => r.Name));
        Assert.Equal(2, vm.Recents.Count);
    }

    [Fact]
    public async Task LoadRecents_DropsRemoved_AndInsertsNew()
    {
        var london = new GeocodingResult("London", 51.5, -0.12, "United Kingdom", "England");
        var paris = new GeocodingResult("Paris", 48.85, 2.35, "France", "Île-de-France");
        var tokyo = new GeocodingResult("Tokyo", 35.68, 139.69, "Japan", "Tokyo");
        var vm = CreateViewModel();

        _cache.GetRecentLocationsAsync(Arg.Any<CancellationToken>()).Returns(new[] { london, paris });
        await vm.LoadRecentsAsync();

        _cache.GetRecentLocationsAsync(Arg.Any<CancellationToken>()).Returns(new[] { tokyo, london });
        await vm.LoadRecentsAsync();

        Assert.Equal(new[] { "Tokyo", "London" }, vm.Recents.Select(r => r.Name));
    }

    [Fact]
    public async Task CanEditSearch_IsFalseWhileSearching_AndTrueOtherwise()
    {
        var gate = new TaskCompletionSource<GeocodingResult?>();
        _geocoding.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(gate.Task);
        var vm = CreateViewModel();
        vm.City = "Lisbon";

        Assert.True(vm.CanEditSearch);

        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        var execution = vm.SearchCommand.ExecuteAsync(null);
        Assert.False(vm.CanEditSearch);
        Assert.Contains(nameof(SearchViewModel.CanEditSearch), changed);

        gate.SetResult(null);
        await execution;

        Assert.True(vm.CanEditSearch);
    }
}
