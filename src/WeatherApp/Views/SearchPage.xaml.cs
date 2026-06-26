using WeatherApp.Core.ViewModels;

namespace WeatherApp.Views;

public partial class SearchPage : ContentPage
{
    private const double ThemeThumbTravel = 32d;

    private bool _hasNavigatedTo;

    public SearchViewModel ViewModel { get; }

    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = ViewModel.PrepareAsync();
        MoveThemeThumb(animated: false);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Skip the initial navigation (app launch). On every return from the results
        // page, PrepareAsync has already cleared the previous query, so focus the
        // field — raising the keyboard — ready to type a new search straight away.
        if (_hasNavigatedTo)
        {
            Dispatcher.Dispatch(() => CityEntry.Focus());
        }

        _hasNavigatedTo = true;
    }

    private void OnThemeSwitchTapped(object? sender, TappedEventArgs e)
    {
        ViewModel.ToggleThemeCommand.Execute(null);
        MoveThemeThumb(animated: true);
    }

    private void MoveThemeThumb(bool animated)
    {
        var target = ViewModel.IsDarkTheme ? ThemeThumbTravel : 0d;
        if (animated)
        {
            _ = ThemeThumb.TranslateToAsync(target, 0, 180, Easing.CubicInOut);
        }
        else
        {
            ThemeThumb.TranslationX = target;
        }
    }
}
