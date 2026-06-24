using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly IGeocodingService _geocodingService;
    private readonly INavigationService _navigationService;
    private readonly IWeatherCache _cache;
    private readonly IThemeService _theme;

    public SearchViewModel(
        IGeocodingService geocodingService,
        INavigationService navigationService,
        IWeatherCache cache,
        IThemeService theme)
    {
        _geocodingService = geocodingService;
        _navigationService = navigationService;
        _cache = cache;
        _theme = theme;
        City = string.Empty;
        Recents.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasRecents));
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial string City { get; set; }

    partial void OnCityChanged(string value)
    {
        if (State is ViewState.Empty or ViewState.Error)
        {
            ResetToIdle();
        }
    }

    public ObservableCollection<GeocodingResult> Recents { get; } = [];

    public bool HasRecents => Recents.Count > 0;

    public string ThemeGlyph => _theme.IsDark ? "🌙" : "☀️";

    public bool IsDarkTheme => _theme.IsDark;

    public Task PrepareAsync()
    {
        ResetToIdle();
        return LoadRecentsAsync();
    }

    public async Task LoadRecentsAsync()
    {
        var recents = await _cache.GetRecentLocationsAsync();
        SyncRecents(recents);
    }

    private void SyncRecents(IReadOnlyList<GeocodingResult> fresh)
    {
        for (var i = Recents.Count - 1; i >= 0; i--)
        {
            if (!fresh.Contains(Recents[i]))
            {
                Recents.RemoveAt(i);
            }
        }

        for (var i = 0; i < fresh.Count; i++)
        {
            var location = fresh[i];
            var current = Recents.IndexOf(location);
            if (current < 0)
            {
                Recents.Insert(i, location);
            }
            else if (current != i)
            {
                Recents.Move(current, i);
            }
        }
    }

    private bool CanSearch => !IsBusy && !string.IsNullOrWhiteSpace(City);

    public bool CanEditSearch => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private Task SearchAsync(CancellationToken cancellationToken) => ExecuteSearchAsync(cancellationToken);

    [RelayCommand]
    private Task OpenRecentAsync(GeocodingResult location) => _navigationService.GoToResultsAsync(location);

    [RelayCommand]
    private void ToggleTheme()
    {
        _theme.Toggle();
        OnPropertyChanged(nameof(ThemeGlyph));
        OnPropertyChanged(nameof(IsDarkTheme));
    }

    private Task ExecuteSearchAsync(CancellationToken cancellationToken) =>
        RunGuardedAsync(async token =>
        {
            var result = await _geocodingService.SearchAsync(City, token);
            if (result is null)
            {
                return ViewState.Empty;
            }

            await _navigationService.GoToResultsAsync(result);
            return ViewState.Success;
        }, cancellationToken);

    protected override void OnBusyChanged()
    {
        SearchCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanEditSearch));
    }
}
