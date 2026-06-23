using System.ComponentModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.ViewModels;
using ViewState = WeatherApp.Core.Enums.ViewState;

namespace WeatherApp.Views;

public partial class ResultsPage : ContentPage, IQueryAttributable
{
    private readonly ResultsViewModel _viewModel;
    private bool _entranceShown;
    private bool _isFlashing;

    public ResultsPage(ResultsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(ResultsViewModel.GeoQueryKey, out var value)
            && value is GeocodingResult location)
        {
            _ = _viewModel.LoadAsync(location);
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.Detach();
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResultsViewModel.State)
            && _viewModel.State == ViewState.Success
            && !_entranceShown)
        {
            _entranceShown = true;
            await AnimateEntranceAsync();
        }
        else if (e.PropertyName == nameof(ResultsViewModel.Weather)
                 && _entranceShown
                 && _viewModel.Weather is not null)
        {
            await FlashTemperatureAsync();
        }
    }

    private async Task AnimateEntranceAsync()
    {
        SuccessContent.Opacity = 0;
        SuccessContent.TranslationY = 24;
        await Task.WhenAll(
            SuccessContent.FadeToAsync(1, 350, Easing.CubicOut),
            SuccessContent.TranslateToAsync(0, 0, 350, Easing.CubicOut));
    }

    private async Task FlashTemperatureAsync()
    {
        if (_isFlashing)
        {
            return;
        }

        _isFlashing = true;
        try
        {
            await TemperatureLabel.FadeToAsync(0.35, 120, Easing.CubicOut);
            await TemperatureLabel.FadeToAsync(1, 220, Easing.CubicIn);
        }
        finally
        {
            _isFlashing = false;
        }
    }
}
