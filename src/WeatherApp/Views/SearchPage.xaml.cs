using WeatherApp.Core.ViewModels;

namespace WeatherApp.Views;

public partial class SearchPage : ContentPage
{
    private const double ThemeThumbTravel = 32d;

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
            _ = ThemeThumb.TranslateTo(target, 0, 180, Easing.CubicInOut);
        }
        else
        {
            ThemeThumb.TranslationX = target;
        }
    }
}
