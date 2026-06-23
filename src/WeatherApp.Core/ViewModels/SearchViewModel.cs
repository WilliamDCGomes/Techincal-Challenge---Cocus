using System.Collections.ObjectModel;
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

    public SearchViewModel(
        IGeocodingService geocodingService,
        INavigationService navigationService,
        IWeatherCache cache)
    {
        _geocodingService = geocodingService;
        _navigationService = navigationService;
        _cache = cache;
        City = string.Empty;
        Recents.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasRecents));
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial string City { get; set; }

    public ObservableCollection<GeocodingResult> Recents { get; } = [];

    public bool HasRecents => Recents.Count > 0;

    public Task PrepareAsync()
    {
        ResetToIdle();
        return LoadRecentsAsync();
    }

    public async Task LoadRecentsAsync()
    {
        var recents = await _cache.GetRecentLocationsAsync();
        Recents.Clear();
        foreach (var location in recents)
        {
            Recents.Add(location);
        }
    }

    private bool CanSearch => !IsBusy && !string.IsNullOrWhiteSpace(City);

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private Task SearchAsync(CancellationToken cancellationToken) => ExecuteSearchAsync(cancellationToken);

    [RelayCommand]
    private Task OpenRecentAsync(GeocodingResult location) => _navigationService.GoToResultsAsync(location);

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

    protected override void OnBusyChanged() => SearchCommand.NotifyCanExecuteChanged();
}
